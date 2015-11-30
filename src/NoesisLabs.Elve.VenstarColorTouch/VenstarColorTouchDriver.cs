using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.Communication;
using CodecoreTechnologies.Elve.DriverFramework.DriverInterfaces;
using CodecoreTechnologies.Elve.DriverFramework.Scripting;
using Newtonsoft.Json;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using NoesisLabs.Elve.VenstarColorTouch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Timers;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	[Driver("Venstar ColorTouch Driver", "A driver for monitoring and controlling Venstar ColorTouch thermostats.", "Ryan Melena", "Climate Control", "", "ColorTouch", DriverCommunicationPort.Network, DriverMultipleInstances.OnePerDriverService, 0, 2, DriverReleaseStages.Production, "Venstar", "http://www.venstar.com/", null)]
	public class VenstarColorTouchDriver : Driver, IClimateControlDriver
	{
		#region Constants

		private const int MAX_THERMOSTATS = 256;
		private const int MAX_ZONE_NUMBER = 99;
		private const int MIN_ZONE_NUMBER = 1;

		#endregion Constants

		#region Fields

		private ICommunication multicastComm;
		private Timer refreshTimer;
		private List<ThermostatIdentifier> thermostatIdentifiers = new List<ThermostatIdentifier>();
		private List<Thermostat> thermostats = new List<Thermostat>();
		private ICommunication unicastComm;

		#endregion Fields

		#region DriverSettings

		[DriverSetting("Refresh Interval", "Interval in seconds between status update requests.  Values update asynchronously when changed via Elve.", 1D, double.MaxValue, "60", true)]
		public int RefreshIntervalSetting { get; set; }

		//[DriverSettingArrayNames("Thermostat Mac Addresses", "MAC address for each source.", typeof(ArrayItemsDriverSettingEditor), "MacAddresses", 1, 256, "", true)]
		//public string MacAddressesSetting
		//{
		//	set
		//	{
		//		if (!string.IsNullOrEmpty(value))
		//		{
		//			XElement element = XElement.Parse(value);
		//			element.Elements("Item").Select(e => e.Attribute("MacAddress").Value).ToList().Except(this.thermostats.Select(t => t.MacAddress));
		//		}
		//	}
		//}

		//[DriverSetting("Zone Count", "Number of zones supported by device.", new string[] { "4", "6" }, "4", true)]
		//public int ZoneCountSetting { get; set; }

		//[DriverSettingArrayNames("Zone Names", "User-defined friendly names for each zone.", typeof(ArrayItemsDriverSettingEditor), "ZoneNames", MIN_ZONE_NUMBER, MAX_ZONE_NUMBER, "", false)]
		//public string ZoneNamesSetting
		//{
		//	set
		//	{
		//		if (!string.IsNullOrEmpty(value))
		//		{
		//			XElement element = XElement.Parse(value);
		//			//this._zoneNames = element.Elements("Item").Select(e => e.Attribute("Name").Value).ToArray();
		//		}
		//	}
		//}

		[DriverSetting("Thermostats", "Venstar ColorTouch Thermostats", typeof(ThermostatIdentifiersDriverSettingEditor), null, true)]
		public string ThermostatIdentifiersSetting
		{
			set
			{
				this.thermostatIdentifiers = value.DeserializeToThermostatIdentifiers().ToList();
				this.UpdateThermostatList();
				this.Refresh();
			}
		}

		#endregion DriverSettings

		[ScriptObjectPropertyAttribute("Paged List Thermostats", "Provides the list of thermostats to be shown in a Touch Screen Interface's Paged List control. The item value has the following properties: ID. ID is the thermostat id.")]
		[SupportsDriverPropertyBinding]
		public ScriptPagedListCollection PagedListThermostats
		{
			get
			{
				return new ScriptPagedListCollection(this.thermostats.Select(t =>
					{
						var thermostat = new ScriptExpandoObject();
						thermostat.SetProperty("ID", new ScriptNumber(this.thermostats.IndexOf(t)));
						string subtitle = t.Sensors.First().Temperature.ToString() + "\u00B0  (" + t.CoolTemp.ToString() + "\u00B0/" + t.HeatTemp.ToString() + "\u00B0) Mode: " + t.Mode.ToString();
						return new ScriptPagedListItem(t.Name, subtitle, thermostat);
					}));
			}
		}

		[ScriptObjectPropertyAttribute("Thermostat Away State Values", "Gets an array of all thermostats' away state value.", "the {NAME} current away state value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatAwayStates
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (t is ResidentialThermostat) ? (int)((ResidentialThermostat)t).AwayState : -1), new ScriptArraySetScriptNumberCallback(this.SetThermostatAwayState), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Away State Names", "Gets an array of all thermostats' away state name.", "the {NAME} current away state name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatAwayStateTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => (t is ResidentialThermostat) ? ((ResidentialThermostat)t).AwayState.ToString() : "N/A"), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Cool Set Points", "Gets an array of all thermostats' cool set point.", "the {NAME} cool set point for item #{INDEX|1}", null)]
		public IScriptArray ThermostatCoolSetPoints
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => t.CoolTemp), new ScriptArraySetScriptNumberCallback(this.SetThermostatCoolSetPoint), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Current Temperatures", "Gets an array of all thermostats' current temperature.", "the {NAME} current temperature for item #{INDEX|1}", null)]
		public IScriptArray ThermostatCurrentTemperatures
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.SpaceTemp), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Fan Mode Values", "Gets an array of all thermostats' fan mode value.", "the {NAME} current fan mode value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatFanModes
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (int)t.FanSetting), new ScriptArraySetScriptNumberCallback(this.SetThermostatFanMode), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Fan Mode Names", "Gets an array of all thermostats' fan mode name.", "the {NAME} current fan mode name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatFanModeTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.FanSetting.ToString()), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Heat Set Points", "Gets an array of all thermostats' heat set point.", "the {NAME} heat set point for item #{INDEX|1}", null)]
		public IScriptArray ThermostatHeatSetPoints
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => t.HeatTemp), new ScriptArraySetScriptNumberCallback(this.SetThermostatHeatSetPoint), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Hold Values", "Gets an array of all thermostats' hold setting value.", "the {NAME} current hold setting value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatHolds
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => "N/A"), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Mode Values", "Gets an array of all thermostats' mode value.", "the {NAME} current mode value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatModes
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (int)t.Mode), new ScriptArraySetScriptNumberCallback(this.SetThermostatMode), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Mode Names", "Gets an array of all thermostats' mode name.", "the {NAME} current mode name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatModeTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.Mode.ToString()), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Names", "Gets an array of all thermostats' name.", "the {NAME} current name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatNames
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.Name), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Schedule Mode Values", "Gets an array of all thermostats' schedule mode value.", "the {NAME} current schedule mode value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatScheduleModes
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (int)t.ScheduleSetting), new ScriptArraySetScriptNumberCallback(this.SetThermostatScheduleMode), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Schedule Mode Names", "Gets an array of all thermostats' schedule mode name.", "the {NAME} current schedule mode name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatScheduleModeTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.ScheduleSetting.ToString()), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Schedule Parts", "Gets an array of all thermostats' schedule part.", "the {NAME} current schedule part for item #{INDEX|1}", null)]
		public IScriptArray ThermostatScheduleParts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.SchedulePart.ToString()), 1); }
		}

		[ScriptObjectMethod("Set Thermostat Cool Set Point", "Set the cool set point on a thermostat.", "Set the cool set point to {PARAM|1|72} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("SetPoint", "The cool set point.", 0, 100)]
		public void SetThermostatCoolSetPoint(ScriptNumber thermostatID, ScriptNumber setPoint)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats[index] != null)
			{
				this.thermostats[index].SetCoolTemp(setPoint.ToPrimitiveInt32());
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatCoolSetPoint call.");
			}
		}

		[ScriptObjectMethod("Set Thermostat Away State", "Set the away state on a residential thermostat.", "Set the away state to {PARAM|1|72} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("AwayState", "The away state. Valid values are: 0 for 'Home' and 1 for 'Away'", 0, 1)]
		public void SetThermostatAwayState(ScriptNumber thermostatID, ScriptNumber awayState)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats[index] != null)
			{
				if (this.thermostats[index] is ResidentialThermostat)
				{
					((ResidentialThermostat)this.thermostats[index]).SetAwayState((AwayState)awayState.ToPrimitiveInt32());
				}
				else
				{
					this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatAwayState call.  Only Residential thermostats support Away State.");
				}
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatAwayState call.");
			}
		}

		[ScriptObjectMethod("Set Thermostat Fan Mode", "Set the fan mode on a thermostat.", "Set the fan mode to {PARAM|1|72} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("FanMode", "The fan mode.  Valid values are: 0 for 'Auto' and 1 for 'On'.", 0, 1)]
		public void SetThermostatFanMode(ScriptNumber thermostatID, ScriptNumber fanMode)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].SetFanSetting((FanSetting)fanMode.ToPrimitiveInt32());
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatFanMode call.");
			}
		}

		[ScriptObjectMethod("Set Thermostat Heat Set Point", "Set the heat set point on a thermostat.", "Set the heat set point to {PARAM|1|72} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("SetPoint", "The heat set point.", 0, 100)]
		public void SetThermostatHeatSetPoint(ScriptNumber thermostatID, ScriptNumber setPoint)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].SetHeatTemp(setPoint.ToPrimitiveInt32());
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatHeatSetPoint call.");
			}
		}

		public void SetThermostatHold(ScriptNumber thermostatID, ScriptBoolean hold)
		{
			Logger.Error("Not supported.");
		}

		[ScriptObjectMethod("Set Thermostat Mode", "Set mode on a thermostat.", "Set mode to {PARAM|1|3} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("Mode", "Thermostat mode.  Valid values are: 0 for 'Off', 1 for 'Heat', 2 for 'Cool', and 3 for 'Auto'.", 0, 3)]
		public void SetThermostatMode(ScriptNumber thermostatID, ScriptNumber mode)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].SetMode((Mode)mode.ToPrimitiveInt32());
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatMode call.");
			}
		}

		[ScriptObjectMethod("Set Thermostat Schedule Mode", "Set schedule mode on a thermostat.", "Set schedule mode to {PARAM|1|3} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("Mode", "Schedule mode.  Valid values are: 0 for 'Off', 1 for 'On'.", 0, 1)]
		public void SetThermostatScheduleMode(ScriptNumber thermostatID, ScriptNumber mode)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].SetScheduleSetting((ScheduleSetting)mode.ToPrimitiveInt32());
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatScheduleMode call.");
			}
		}
		public override bool StartDriver(Dictionary<string, byte[]> configFileData)
		{
			Logger.Debug("Starting Venstar ColorTouch Driver.");

			//TODO: Set this.thermostatIndex from count of thermostats in config

			try
			{
				this.UpdateThermostatList();

				this.refreshTimer = new System.Timers.Timer();
				this.refreshTimer.Interval = this.RefreshIntervalSetting * 1000;
				this.refreshTimer.AutoReset = false;
				this.refreshTimer.Elapsed += new ElapsedEventHandler(this.TimedRefresh);
				this.TimedRefresh(this, null);

				return true;
			}
			catch (Exception ex)
			{
				Logger.Error("Venstar ColorTouch Driver initialization failed.", ex);
				return false;
			}
			finally
			{
				//if (this.refreshTimer != null) { this.refreshTimer.Dispose(); }
				//if (this.http != null) { this.http.Dispose(); }
				//if (this.multicastComm != null) { this.multicastComm.Dispose(); }
				//if (this.unicastComm != null) { this.unicastComm.Dispose(); }
			}
		}

		protected void ThermostatCoolSetPointChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatCoolSetPoints", index, this.ThermostatCoolSetPoints[index + 1]);
		}

		protected void ThermostatCurrentTemperatureChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatCurrentTemperatures", index, this.ThermostatCurrentTemperatures[index + 1]);
		}

		protected void ThermostatFanModeChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatFanModes", index, this.ThermostatFanModes[index + 1]);
		}

		protected void ThermostatFanModeTextChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatFanModeTexts", index, this.ThermostatFanModeTexts[index + 1]);
		}

		protected void ThermostatHeatSetPointChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatHeatSetPoints", index, this.ThermostatHeatSetPoints[index + 1]);
		}

		protected void ThermostatHoldChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatHolds", index, this.ThermostatHolds[index + 1]);
		}

		protected void ThermostatModeChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatModes", index, this.ThermostatModes[index + 1]);
		}

		protected void ThermostatModeTextChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatModeTexts", index, this.ThermostatModeTexts[index + 1]);
		}

		protected void ThermostatNameChanged(object sender, EventArgs e)
		{
			Thermostat thermostat = sender as Thermostat;
			int index = this.thermostats.IndexOf(thermostat);
			DevicePropertyChangeNotification("ThermostatNames", index, this.ThermostatNames[index + 1]);
		}

		private void AddThermostatIdentifier(ThermostatIdentifier thermostatIdentifier)
		{
			if (!this.thermostatIdentifiers.Select(t => t.MacAddress).Contains(thermostatIdentifier.MacAddress))
			{
				List<ThermostatIdentifier> newThermostatIdentifiers = new List<ThermostatIdentifier>(this.thermostatIdentifiers);
				newThermostatIdentifiers.Add(thermostatIdentifier);

				this.ThermostatIdentifiersSetting = newThermostatIdentifiers.Serialize();
			}
		}

		private void Refresh()
		{
			this.Logger.Debug("Refreshing Thermostat Values.");

			foreach (Thermostat thermostat in this.thermostats)
			{
				thermostat.UpdateStatus();
			}
		}

		private void TimedRefresh(object sender, ElapsedEventArgs e)
		{
			try
			{
				this.refreshTimer.Stop();
				this.Refresh();
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error refreshing thermostat values.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private void UpdateThermostatList()
		{
			this.Logger.Debug("Updating Thermostat List.");

			WebClient http = new WebClient();
			foreach (ThermostatIdentifier thermostatIdentifier in this.thermostatIdentifiers)
			{
				Thermostat thermostat = this.thermostats.SingleOrDefault(t => t.MacAddress == thermostatIdentifier.MacAddress);

				if (thermostat == null)
				{
					ApiInformation apiInfo = JsonConvert.DeserializeObject<ApiInformation>(http.DownloadString(thermostatIdentifier.Url));

					thermostat = (apiInfo.Type == "commercial") ?
						(Thermostat)new CommercialThermostat(thermostatIdentifier.MacAddress, thermostatIdentifier.Name, thermostatIdentifier.Url, this.Logger) :
						(Thermostat)new ResidentialThermostat(thermostatIdentifier.MacAddress, thermostatIdentifier.Name, thermostatIdentifier.Url, this.Logger);

					thermostat.CoolTempChanged += new EventHandler(this.ThermostatCoolSetPointChanged);
					thermostat.SensorsChanged += new EventHandler(this.ThermostatCurrentTemperatureChanged);
					thermostat.FanSettingChanged += new EventHandler(this.ThermostatFanModeChanged);
					thermostat.FanSettingChanged += new EventHandler(this.ThermostatFanModeTextChanged);
					thermostat.HeatTempChanged += new EventHandler(this.ThermostatHeatSetPointChanged);
					thermostat.ScheduleSettingChanged += new EventHandler(this.ThermostatHoldChanged);
					thermostat.ModeChanged += new EventHandler(this.ThermostatModeTextChanged);
					thermostat.ModeChanged += new EventHandler(this.ThermostatModeChanged);

					this.thermostats.Add(thermostat);
				}
				else
				{
					thermostat.Name = thermostatIdentifier.Name;
					thermostat.Url = thermostatIdentifier.Url;
				}
			}
		}
	}
}