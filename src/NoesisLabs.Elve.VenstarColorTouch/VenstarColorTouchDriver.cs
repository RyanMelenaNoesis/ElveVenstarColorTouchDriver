using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.Communication;
using CodecoreTechnologies.Elve.DriverFramework.DeviceSettingEditors;
using CodecoreTechnologies.Elve.DriverFramework.DriverInterfaces;
using CodecoreTechnologies.Elve.DriverFramework.Scripting;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using NoesisLabs.Elve.VenstarColorTouch.Models;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Xml.Linq;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	[Driver("Venstar ColorTouch Driver", "A driver for monitoring and controlling Venstar ColorTouch thermostats.", "Ryan Melena", "Climate Control", "", "ColorTouch", DriverCommunicationPort.Network, DriverMultipleInstances.OnePerDriverService, 0, 1, DriverReleaseStages.Development, "Venstar", "http://www.venstar.com/", null)]
	public class VenstarColorTouchDriver : Driver, IClimateControlDriver
	{
		#region Constants

		private const string COLORTOUCH_SSDP_COMMERCIAL_MODEL_KEYWORD = "type:commercial";
		private const string COLORTOUCH_SSDP_IDENTIFIER_TOKEN = "ecp:";
		private const int COLORTOUCH_SSDP_IDENTIFIER_LENGTH = 17;
		private const string COLORTOUCH_SSDP_KEYWORD = "colortouch:ecp";
		private const string COLORTOUCH_SSDP_RESIDENTIAL_MODEL_KEYWORD = "type:residential";
		private const int MAX_THERMOSTATS = 256;
		private const string SSDP_DISCOVERY_MESSAGE = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nMan: ssdp:discover\r\nST: colortouch:ecp\r\n";

		#endregion

		#region Fields

		private HttpClient http;
		private ICommunication multicastComm;
		private Timer refreshTimer;
		private List<Thermostat> thermostats = new List<Thermostat>();
		private ICommunication unicastComm;

		#endregion

		#region DriverSettings

		[DriverSetting("Refresh Interval", "Interval in seconds between status update requests.  Values update asynchronously when changed via Elve.", 1D, double.MaxValue, "1", true)]
		public int RefreshIntervalSetting { get; set; }

		[DriverSettingArrayNames("Thermostat Mac Addresses", "MAC address for each source.", typeof(ArrayItemsDriverSettingEditor), "MacAddresses", 1, 256, "", true)]
		public string MacAddressesSetting
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					XElement element = XElement.Parse(value);
					element.Elements("Item").Select(e => e.Attribute("MacAddress").Value).ToList().Except(this.thermostats.Select(t => t.MacAddress));
				}
			}
		}

		[DriverSetting("Zone Count", "Number of zones supported by device.", new string[] { "4", "6" }, "4", true)]
		public int ZoneCountSetting { get; set; }

		[DriverSettingArrayNames("Zone Names", "User-defined friendly names for each zone.", typeof(ArrayItemsDriverSettingEditor), "ZoneNames", MIN_ZONE_NUMBER, MAX_ZONE_NUMBER, "", false)]
		public string ZoneNamesSetting
		{
			set
			{
				if (!string.IsNullOrEmpty(value))
				{
					XElement element = XElement.Parse(value);
					this._zoneNames = element.Elements("Item").Select(e => e.Attribute("Name").Value).ToArray();
				}
			}
		}

		#endregion

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

		[ScriptObjectMethod("Set Thermostat Fan Mode", "Set the fan mode on a thermostat.", "Set the fan mode to {PARAM|1|72} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("FanMode", "The fan mode.  Valid values are: 0 for 'Auto' and 1 for 'On'.", 0, 1)]
		public void SetThermostatFanMode(ScriptNumber thermostatID, ScriptNumber fanMode)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].FanSetting = (FanSetting)fanMode.ToPrimitiveInt32();
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

		[ScriptObjectMethod("Set Thermostat Hold", "Set temperature hold on a thermostat.", "Set temperature hold to {PARAM|1|0} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("Hold", "Temperature hold.")]
		public void SetThermostatHold(ScriptNumber thermostatID, ScriptBoolean hold)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				ScheduleSetting scheduleSettings = (hold.ToPrimitiveBoolean()) ? ScheduleSetting.Off : ScheduleSetting.On;
				this.thermostats[index].ScheduleSetting = scheduleSettings;
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatHold call.");
			}
		}

		[ScriptObjectMethod("Set Thermostat Mode", "Set mode on a thermostat.", "Set mode to {PARAM|1|3} for thermostat {PARAM|0|1} on {NAME}.")]
		[ScriptObjectMethodParameter("ThermostatID", "The Id of the thermostat.", 1, 256)]
		[ScriptObjectMethodParameter("Mode", "Thermostat mode.  Valid values are: 0 for 'Off', 1 for 'Heat', 2 for 'Cool', and 3 for 'Auto'.", 0, 3)]
		public void SetThermostatMode(ScriptNumber thermostatID, ScriptNumber mode)
		{
			int index = thermostatID.ToPrimitiveInt32() - 1;
			if (this.thermostats.Count > index)
			{
				this.thermostats[index].Mode = (Mode)mode.ToPrimitiveInt32();
			}
			else
			{
				this.Logger.Error("Invalid ThermostatID [" + thermostatID.ToString() + "] in SetThermostatMode call.");
			}
		}

		[ScriptObjectPropertyAttribute("Thermostat Cool Set Points", "Gets an array of all thermostats' cool set point.", "the {NAME} cool set point for item #{INDEX|1}", null)]
		public IScriptArray ThermostatCoolSetPoints
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => t.CoolTemp), new ScriptArraySetScriptNumberCallback(this.SetThermostatCoolSetPoint), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Current Temperatures", "Gets an array of all thermostats' current temperature.", "the {NAME} current temperature for item #{INDEX|1}", null)]
		public IScriptArray ThermostatCurrentTemperatures
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.Sensors.First().Temperature), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Fan Mode Names", "Gets an array of all thermostats' fan mode name.", "the {NAME} current fan mode name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatFanModeTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.FanSetting.ToString()), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Fan Mode Values", "Gets an array of all thermostats' fan mode value.", "the {NAME} current fan mode value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatFanModes
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (int)t.FanSetting), new ScriptArraySetScriptNumberCallback(this.SetThermostatFanMode), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Heat Set Points", "Gets an array of all thermostats' heat set point.", "the {NAME} heat set point for item #{INDEX|1}", null)]
		public IScriptArray ThermostatHeatSetPoints
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => t.HeatTemp), new ScriptArraySetScriptNumberCallback(this.SetThermostatHeatSetPoint), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Hold Setting Values", "Gets an array of all thermostats' hold setting value.", "the {NAME} current hold setting value for item #{INDEX|1}", null)]
		public IScriptArray ThermostatHolds
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (t.ScheduleSetting == ScheduleSetting.Off)), new ScriptArraySetScriptBooleanCallback(this.SetThermostatHold), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Hold Setting Names", "Gets an array of all thermostats' hold setting name.", "the {NAME} current hold setting name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatModeTexts
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => "Schedule " + t.ScheduleSetting.ToString()), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Modes", "Gets an array of all thermostats' mode.", "the {NAME} current mode for item #{INDEX|1}", null)]
		public IScriptArray ThermostatModes
		{
			get { return new ScriptArrayMarshalByReference(this.thermostats.Select(t => (int)t.Mode), new ScriptArraySetScriptNumberCallback(this.SetThermostatMode), 1); }
		}

		[ScriptObjectPropertyAttribute("Thermostat Names", "Gets an array of all thermostats' name.", "the {NAME} current name for item #{INDEX|1}", null)]
		public IScriptArray ThermostatNames
		{
			get { return new ScriptArrayMarshalByValue(this.thermostats.Select(t => t.Name), 1); }
		}

		public override bool StartDriver(Dictionary<string, byte[]> configFileData)
		{
			Logger.Debug("Starting Venstar ColorTouch Driver.");

			//TODO: Set this.thermostatIndex from count of thermostats in config

			try
			{
				var discoveryEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

				var localMulticastEndPoint = new IPEndPoint(IPAddress.Any, 1900);
				var multicastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				multicastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(discoveryEndpoint.Address, IPAddress.Any));
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);
				multicastSocket.Bind(localMulticastEndPoint);
				this.multicastComm = new UdpCommunication(multicastSocket, discoveryEndpoint);
				this.multicastComm.ReceivedBytes += Comm_ReceivedBytes;

				var localUnicastEndPoint = new IPEndPoint(IPAddress.Any, 0);
				var unicastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				unicastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				unicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 4);
				unicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);
				unicastSocket.Bind(localUnicastEndPoint);
				this.unicastComm = new UdpCommunication(unicastSocket, discoveryEndpoint);
				this.unicastComm.ReceivedBytes += Comm_ReceivedBytes;

				this.SendDiscoveryMessage();

				this.http = new HttpClient();

				this.refreshTimer = new System.Timers.Timer();
				this.refreshTimer.Interval = 10 * 1000;
				this.refreshTimer.AutoReset = false;
				this.refreshTimer.Elapsed += new ElapsedEventHandler(this.Refresh);
				this.Refresh(this, (ElapsedEventArgs)ElapsedEventArgs.Empty);

				return true;
			}
			catch(Exception ex)
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

		private void AddThermostat(Thermostat thermostat)
		{
			this.thermostats.Add(thermostat);
			//TODO: Save list of thermostats to configuration
		}

		private void Comm_ReceivedBytes(object sender, ReceivedBytesEventArgs e)
		{
			string message = System.Text.Encoding.Default.GetString(e.ReceiveBuffer);

			if(message.Contains(COLORTOUCH_SSDP_KEYWORD))
			{
				var identifier = this.GetIdentifierFromSsdp(message);

				Thermostat thermostat;

				lock (this.thermostats)
				{
					if (this.thermostats.Select(t => t.MacAddress).Contains(identifier))
					{
						thermostat = this.thermostats.Single(t => t.MacAddress == identifier);
						thermostat.LastSeen = DateTime.Now;
					}
					else
					{
						if (message.Contains(COLORTOUCH_SSDP_RESIDENTIAL_MODEL_KEYWORD))
						{
							this.AddThermostat(new ResidentialThermostat(identifier, message, this.Logger));
						}
						else if (message.Contains(COLORTOUCH_SSDP_COMMERCIAL_MODEL_KEYWORD))
						{
							this.AddThermostat(new CommercialThermostat(identifier, message, this.Logger));
						}
						else
						{
							this.Logger.Error(String.Format("Unable to identify thermostat model (Residential or Commercial) from SSDP Response [{0}].", message));
						}
					}
				}
			}
		}

		private void SendDiscoveryMessage()
		{
			try
			{
				this.Logger.Debug("Sending Discovery Message.");

				this.unicastComm.Send(SSDP_DISCOVERY_MESSAGE);
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error sending discovery message.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private void Refresh(object sender, ElapsedEventArgs e)
		{
			try
			{
				this.Logger.Debug("Refreshing Thermostat Values.");

				//TODO: Refresh thermostat values
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error refreshing thermostat values.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private string GetIdentifierFromSsdp(string ssdp)
		{
			try
			{
				return ssdp.Substring(ssdp.IndexOf(COLORTOUCH_SSDP_IDENTIFIER_TOKEN) + COLORTOUCH_SSDP_IDENTIFIER_TOKEN.Length, COLORTOUCH_SSDP_IDENTIFIER_LENGTH);
			}
			catch(Exception ex)
			{
				this.Logger.Error(String.Format("Error parsing thermostat identifier from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}
	}
}