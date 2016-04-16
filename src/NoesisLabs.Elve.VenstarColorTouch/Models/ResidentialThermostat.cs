using CodecoreTechnologies.Elve.DriverFramework;
using Newtonsoft.Json;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;
using System.Collections.Generic;
using System.Net;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class ResidentialThermostat : Thermostat
	{
		private AwayState _awayState;
		private ICollection<ResidentialRuntime> _runtimes;

		public ResidentialThermostat(string macAddress, string name, string url, ILogger logger)
			: base(macAddress, name, url, logger)
		{ }

		public event EventHandler AwayStateChanged;

		public event EventHandler RuntimesChanged;

		public AwayState AwayState
		{
			get { return this._awayState; }
			set
			{
				if (this._awayState != value)
				{
					this._awayState = value;
					this.OnAwayStateChanged();
				}
			}
		}

		public ICollection<ResidentialRuntime> Runtimes
		{
			get { return this._runtimes; }
			set
			{
				var comparer = new MultiSetComparer<ResidentialRuntime>();

				if (!comparer.Equals(this._runtimes, value))
				{
					this._runtimes = value;
					this.OnRuntimesChanged();
				}
			}
		}

		public override void UpdateStatus()
		{
			ResidentialThermostatInfo info;
			List<Alert> alerts;
			List<ResidentialRuntime> runtimes;
			List<Sensor> sensors;

			using (WebClient http = new WebClient())
			{
				info = JsonConvert.DeserializeObject<ResidentialThermostatInfo>(http.DownloadString(this.Url + "/query/info"));
				alerts = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/alerts"), new { alerts = new List<Alert>() }).alerts;
				runtimes = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/runtimes"), new { runtimes = new List<ResidentialRuntime>() }).runtimes;
				sensors = JsonConvert.DeserializeAnonymousType(http.DownloadString(this.Url + "/query/sensors"), new { sensors = new List<Sensor>() }).sensors;
			}

			this.AwayState = info.AwayState;
			this.Runtimes = runtimes;

			base.UpdateStatus(info, alerts, sensors);
		}

		public void SetAwayState(AwayState state)
		{
			WebClient http = new WebClient();
			string response = http.UploadString(this.Url + "/settings", String.Format("away={0}", (int)state));

			if (response.Contains(SUCCESS_KEYWORD))
			{
				this.AwayState = state;
			}
			else
			{
				this.logger.Error(String.Format("Error setting away state. {0}.", response));
			}
		}

		private void OnAwayStateChanged()
		{
			if (this.AwayStateChanged != null)
			{
				this.AwayStateChanged(this, new EventArgs());
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