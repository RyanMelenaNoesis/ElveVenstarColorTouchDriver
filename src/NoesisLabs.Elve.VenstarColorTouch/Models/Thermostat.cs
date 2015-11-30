using CodecoreTechnologies.Elve.DriverFramework;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;
using System.Collections.Generic;
using System.Net;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public abstract class Thermostat
	{
		protected const string SUCCESS_KEYWORD = "success";

		protected readonly ILogger logger;

		private ICollection<Alert> _alerts;
		private double _apiVersion;
		private double _coolTemp;
		private double _coolTempMax;
		private double _coolTempMin;
		private string _deviceName;
		private FanSetting _fanSetting;
		private FanState _fanState;
		private double _heatTemp;
		private double _heatTempMax;
		private double _heatTempMin;
		private int _humidity;
		private DateTime _lastSeen;
		private Mode _mode;
		private ScheduledPart _schedulePart;
		private ScheduleSetting _scheduleSetting;
		private ICollection<Sensor> _sensors;
		private double _setPointDelta;
		private double _spaceTemp;
		private TempUnits _tempUnits;

		public Thermostat(string macAddress, string name, string url, ILogger logger)
		{
			this.MacAddress = macAddress;
			this.Name = name;
			this.Url = url;
			this.logger = logger;
		}

		public event EventHandler AlertsChanged;

		public event EventHandler ApiVersionChange;

		public event EventHandler CoolTempChanged;

		public event EventHandler CoolTempMaxChanged;

		public event EventHandler CoolTempMinChanged;

		public event EventHandler DeviceNameChanged;

		public event EventHandler FanSettingChanged;

		public event EventHandler FanStateChanged;

		public event EventHandler HeatTempChanged;

		public event EventHandler HeatTempMaxChanged;

		public event EventHandler HeatTempMinChanged;

		public event EventHandler HumidityChanged;

		public event EventHandler LastSeenChanged;

		public event EventHandler ModeChanged;

		public event EventHandler SchedulePartChanged;

		public event EventHandler ScheduleSettingChanged;

		public event EventHandler SensorsChanged;

		public event EventHandler SetPointDeltaChanged;

		public event EventHandler SpaceTempChanged;

		public event EventHandler TempUnitsChanged;

		public ICollection<Alert> Alerts
		{
			get { return this._alerts; }
			set
			{
				var comparer = new MultiSetComparer<Alert>();

				if (!comparer.Equals(this._alerts, value))
				{
					this._alerts = value;
					this.OnAlertsChanged();
				}
			}
		}

		public double ApiVersion
		{
			get { return this._apiVersion; }
			set
			{
				if (this._apiVersion != value)
				{
					this._apiVersion = value;
					this.OnApiVersionChanged();
				}
			}
		}

		public double CoolTemp
		{
			get { return this._coolTemp; }
			set
			{
				if (this._coolTemp != value)
				{
					this._coolTemp = value;
					this.OnCoolTempChanged();
				}
			}
		}

		public double CoolTempMax
		{
			get { return this._coolTempMax; }
			set
			{
				if (this._coolTempMax != value)
				{
					this._coolTempMax = value;
					this.OnCoolTempMaxChanged();
				}
			}
		}

		public double CoolTempMin
		{
			get { return this._coolTempMin; }
			set
			{
				if (this._coolTempMin != value)
				{
					this._coolTempMin = value;
					this.OnCoolTempMinChanged();
				}
			}
		}

		public string DeviceName
		{
			get { return this._deviceName; }
			set
			{
				if (this._deviceName != value)
				{
					this._deviceName = value;
					this.OnDeviceNameChanged();
				}
			}
		}

		public FanSetting FanSetting
		{
			get { return this._fanSetting; }
			set
			{
				if (this._fanSetting != value)
				{
					this._fanSetting = value;
					this.OnFanSettingChanged();
				}
			}
		}

		public FanState FanState
		{
			get { return this._fanState; }
			set
			{
				if (this._fanState != value)
				{
					this._fanState = value;
					this.OnFanStateChanged();
				}
			}
		}

		public double HeatTemp
		{
			get { return this._heatTemp; }
			set
			{
				if (this._heatTemp != value)
				{
					this._heatTemp = value;
					this.OnHeatTempChanged();
				}
			}
		}

		public double HeatTempMax
		{
			get { return this._heatTempMax; }
			set
			{
				if (this._heatTempMax != value)
				{
					this._heatTempMax = value;
					this.OnHeatTempMaxChanged();
				}
			}
		}

		public double HeatTempMin
		{
			get { return this._heatTempMin; }
			set
			{
				if (this._heatTempMin != value)
				{
					this._heatTempMin = value;
					this.OnHeatTempMinChanged();
				}
			}
		}

		public int Humidity
		{
			get { return this._humidity; }
			set
			{
				if (this._humidity != value)
				{
					this._humidity = value;
					this.OnHumidityChanged();
				}
			}
		}

		public DateTime LastSeen
		{
			get { return _lastSeen; }
			set
			{
				if (this._lastSeen != value)
				{
					this._lastSeen = value;
					this.OnLastSeenChanged();
				}
			}
		}

		public string MacAddress { get; private set; }

		public Mode Mode
		{
			get { return this._mode; }
			set
			{
				if (this._mode != value)
				{
					this._mode = value;
					this.OnModeChanged();
				}
			}
		}

		public string Name { get; set; }

		public ScheduledPart SchedulePart
		{
			get { return this._schedulePart; }
			set
			{
				if (this._schedulePart != value)
				{
					this._schedulePart = value;
					this.OnSchedulePartChanged();
				}
			}
		}

		public ScheduleSetting ScheduleSetting
		{
			get { return this._scheduleSetting; }
			set
			{
				if (this._scheduleSetting != value)
				{
					this._scheduleSetting = value;
					this.OnScheduleSettingChanged();
				}
			}
		}

		public ICollection<Sensor> Sensors
		{
			get { return this._sensors; }
			set
			{
				var comparer = new MultiSetComparer<Sensor>();

				if (!comparer.Equals(this._sensors, value))
				{
					this._sensors = value;
					this.OnSensorsChanged();
				}
			}
		}

		public double SetPointDelta
		{
			get { return this._setPointDelta; }
			set
			{
				if (this._setPointDelta != value)
				{
					this._setPointDelta = value;
					this.OnSetPointDeltaChanged();
				}
			}
		}

		public double SpaceTemp
		{
			get { return this._spaceTemp; }
			set
			{
				if (this._spaceTemp != value)
				{
					this._spaceTemp = value;
					this.OnSpaceTempChanged();
				}
			}
		}

		public TempUnits TempUnits
		{
			get { return this._tempUnits; }
			set
			{
				if (this._tempUnits != value)
				{
					this._tempUnits = value;
					this.OnTempUnitsChanged();
				}
			}
		}

		public string Url { get; set; }

		public void SetCoolTemp(int temperature)
		{
			if (temperature < this.CoolTempMin || temperature > this.CoolTempMax)
			{
				this.logger.Error(String.Format("Error setting cool temp.  Provided temp [{0}] is not within the allowed range of [{1} to {2}].", temperature.ToString(), this.CoolTempMin.ToString(), this.CoolTempMax.ToString()));
			}
			else
			{
				WebClient http = new WebClient();
				string response = http.UploadString(this.Url + "/control", String.Format("cooltemp={0}&heattemp={1}", temperature, this.HeatTemp));

				if (response.Contains(SUCCESS_KEYWORD))
				{
					this.CoolTemp = temperature;
				}
				else
				{
					this.logger.Error(String.Format("Error setting cool temp. {0}.", response));
				}
			}
		}

		//public bool IsExpired()
		//{
		//	return DateTime.Now.Subtract(this.LastSeen).TotalSeconds > this.MaxAgeSeconds;
		//}

		public void SetFanSetting(FanSetting setting)
		{
			WebClient http = new WebClient();
			string response = http.UploadString(this.Url + "/control", String.Format("fan={0}", (int)setting));

			if (response.Contains(SUCCESS_KEYWORD))
			{
				this.FanSetting = setting;
			}
			else
			{
				this.logger.Error(String.Format("Error setting fan setting. {0}.", response));
			}
		}

		public void SetScheduleSetting(ScheduleSetting setting)
		{
			WebClient http = new WebClient();
			string response = http.UploadString(this.Url + "/setting", String.Format("schedule={0}", (int)setting));

			if (response.Contains(SUCCESS_KEYWORD))
			{
				this.ScheduleSetting = setting;
			}
			else
			{
				this.logger.Error(String.Format("Error setting schedule setting. {0}.", response));
			}
		}

		public void SetHeatTemp(int temperature)
		{
			if (temperature < this.HeatTempMin || temperature > this.HeatTempMax)
			{
				this.logger.Error(String.Format("Error setting heat temp.  Provided temp [{0}] is not within the allowed range of [{1} to {2}].", temperature.ToString(), this.HeatTempMin.ToString(), this.HeatTempMax.ToString()));
			}
			else
			{
				WebClient http = new WebClient();
				string response = http.UploadString(this.Url + "/control", String.Format("heattemp={0}&cooltemp={1}", temperature, this.CoolTemp));

				if (response.Contains(SUCCESS_KEYWORD))
				{
					this.HeatTemp = temperature;
				}
				else
				{
					this.logger.Error(String.Format("Error setting heat temp. {0}.", response));
				}
			}
		}

		public void SetMode(Mode mode)
		{
			WebClient http = new WebClient();
			string response = http.UploadString(this.Url + "/control", String.Format("mode={0}", (int)mode));

			if (response.Contains(SUCCESS_KEYWORD))
			{
				this.Mode = mode;
			}
			else
			{
				this.logger.Error(String.Format("Error setting mode. {0}.", response));
			}
		}

		public abstract void UpdateStatus();

		protected virtual void UpdateStatus(ThermostatInfo info, ICollection<Alert> alerts, ICollection<Sensor> sensors)
		{
			this.Alerts = alerts;
			this.ApiVersion = info.ApiVersion;
			this.CoolTemp = info.CoolTemp;
			this.CoolTempMax = info.CoolTempMax;
			this.CoolTempMin = info.CoolTempMin;
			this.DeviceName = info.Name;
			this.FanSetting = info.FanSetting;
			this.FanState = info.FanState;
			this.HeatTemp = info.HeatTemp;
			this.HeatTempMax = info.HeatTempMax;
			this.HeatTempMin = info.HeatTempMin;
			this.Humidity = info.Humidity;
			this.Mode = info.Mode;
			this.SchedulePart = info.SchedulePart;
			this.ScheduleSetting = info.ScheduleSetting;
			this.Sensors = sensors;
			this.SetPointDelta = info.SetPointDelta;
			this.SpaceTemp = info.SpaceTemp;
			this.TempUnits = info.TempUnits;
		}

		private void OnAlertsChanged()
		{
			if (this.AlertsChanged != null)
			{
				this.AlertsChanged(this, new EventArgs());
			}
		}

		private void OnApiVersionChanged()
		{
			if (this.ApiVersionChange != null)
			{
				this.ApiVersionChange(this, new EventArgs());
			}
		}

		private void OnCoolTempChanged()
		{
			if (this.CoolTempChanged != null)
			{
				this.CoolTempChanged(this, new EventArgs());
			}
		}

		private void OnCoolTempMaxChanged()
		{
			if (this.CoolTempMaxChanged != null)
			{
				this.CoolTempMaxChanged(this, new EventArgs());
			}
		}

		private void OnCoolTempMinChanged()
		{
			if (this.CoolTempMinChanged != null)
			{
				this.CoolTempMinChanged(this, new EventArgs());
			}
		}

		private void OnDeviceNameChanged()
		{
			if (this.DeviceNameChanged != null)
			{
				this.DeviceNameChanged(this, new EventArgs());
			}
		}

		private void OnFanSettingChanged()
		{
			if (this.FanSettingChanged != null)
			{
				this.FanSettingChanged(this, new EventArgs());
			}
		}

		private void OnFanStateChanged()
		{
			if (this.FanStateChanged != null)
			{
				this.FanStateChanged(this, new EventArgs());
			}
		}

		private void OnHeatTempChanged()
		{
			if (this.HeatTempChanged != null)
			{
				this.HeatTempChanged(this, new EventArgs());
			}
		}

		private void OnHeatTempMaxChanged()
		{
			if (this.HeatTempMaxChanged != null)
			{
				this.HeatTempMaxChanged(this, new EventArgs());
			}
		}

		private void OnHeatTempMinChanged()
		{
			if (this.HeatTempMinChanged != null)
			{
				this.HeatTempMinChanged(this, new EventArgs());
			}
		}

		private void OnHumidityChanged()
		{
			if (this.HumidityChanged != null)
			{
				this.HumidityChanged(this, new EventArgs());
			}
		}

		private void OnLastSeenChanged()
		{
			if (this.LastSeenChanged != null)
			{
				this.LastSeenChanged(this, new EventArgs());
			}
		}

		private void OnModeChanged()
		{
			if (this.ModeChanged != null)
			{
				this.ModeChanged(this, new EventArgs());
			}
		}

		private void OnSchedulePartChanged()
		{
			if (this.SchedulePartChanged != null)
			{
				this.SchedulePartChanged(this, new EventArgs());
			}
		}

		private void OnScheduleSettingChanged()
		{
			if (this.ScheduleSettingChanged != null)
			{
				this.ScheduleSettingChanged(this, new EventArgs());
			}
		}

		private void OnSensorsChanged()
		{
			if (this.SensorsChanged != null)
			{
				this.SensorsChanged(this, new EventArgs());
			}
		}

		private void OnSetPointDeltaChanged()
		{
			if (this.SetPointDeltaChanged != null)
			{
				this.SetPointDeltaChanged(this, new EventArgs());
			}
		}

		private void OnSpaceTempChanged()
		{
			if (this.SpaceTempChanged != null)
			{
				this.SpaceTempChanged(this, new EventArgs());
			}
		}

		private void OnTempUnitsChanged()
		{
			if (this.TempUnitsChanged != null)
			{
				this.TempUnitsChanged(this, new EventArgs());
			}
		}
	}
}