using CodecoreTechnologies.Elve.DriverFramework;
using Newtonsoft.Json;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;
using System.Collections.Generic;
using System.Net;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class CommercialThermostat : Thermostat
	{
		private ForceUnoccupied _forceUnoccupied;
		private HolidayState _holidayState;
		private OverrideState _overrideState;
		private ICollection<CommercialRuntime> _runtimes;

		public CommercialThermostat(string macAddress, string name, string url, ILogger logger)
			: base(macAddress, name, url, logger)
		{ }

		public event EventHandler ForceUnoccupiedChanged;

		public event EventHandler HolidyStateChanged;

		public event EventHandler OverrideStateChanged;

		public event EventHandler RuntimesChanged;

		public ForceUnoccupied ForceUnoccupied
		{
			get { return this._forceUnoccupied; }
			set
			{
				if (this._forceUnoccupied != value)
				{
					this._forceUnoccupied = value;
					this.OnForceUnoccupiedChanged();
				}
			}
		}

		public HolidayState HolidayState
		{
			get { return this._holidayState; }
			set
			{
				if (this._holidayState != value)
				{
					this._holidayState = value;
					this.OnHolidyStateChanged();
				}
			}
		}

		public OverrideState OverrideState
		{
			get { return this._overrideState; }
			set
			{
				if (this._overrideState != value)
				{
					this._overrideState = value;
					this.OnOverrideStateChanged();
				}
			}
		}

		public ICollection<CommercialRuntime> Runtimes
		{
			get { return this._runtimes; }
			set
			{
				var comparer = new MultiSetComparer<CommercialRuntime>();

				if (!comparer.Equals(this._runtimes, value))
				{
					this._runtimes = value;
					this.OnRuntimesChanged();
				}
			}
		}

		public override void UpdateStatus()
		{
			CommercialThermostatInfo info;
			List<Alert> alerts;
			List<CommercialRuntime> runtimes;
			List<Sensor> sensors;

			using (WebClient http = new WebClient())
			{

				info = JsonConvert.DeserializeObject<CommercialThermostatInfo>(http.DownloadString(this.Url + "/query/info"));
				alerts = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/alerts"), new { alerts = new List<Alert>() }).alerts;
				runtimes = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/runtimes"), new { runtimes = new List<CommercialRuntime>() }).runtimes;
				sensors = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/sensors"), new { sensors = new List<Sensor>() }).sensors;
			}

			this.ForceUnoccupied = info.ForceUnoccupied;
			this.HolidayState = info.HolidayState;
			this.Runtimes = runtimes;
			this.OverrideState = info.OverrideState;

			base.UpdateStatus(info, alerts, sensors);
		}

		private void OnForceUnoccupiedChanged()
		{
			if (this.ForceUnoccupiedChanged != null)
			{
				this.ForceUnoccupiedChanged(this, new EventArgs());
			}
		}

		private void OnHolidyStateChanged()
		{
			if (this.HolidyStateChanged != null)
			{
				this.HolidyStateChanged(this, new EventArgs());
			}
		}

		private void OnOverrideStateChanged()
		{
			if (this.OverrideStateChanged != null)
			{
				this.OverrideStateChanged(this, new EventArgs());
			}
		}

		private void OnRuntimesChanged()
		{
			if (this.RuntimesChanged != null)
			{
				this.RuntimesChanged(this, new EventArgs());
			}
		}
	}
}