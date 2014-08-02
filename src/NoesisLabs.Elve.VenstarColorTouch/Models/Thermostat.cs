using CodecoreTechnologies.Elve.DriverFramework;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;
using System.Collections.Generic;
using System.Globalization;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public abstract class Thermostat
	{
		private const string COLORTOUCH_SSDP_LOCATION_TOKEN = "Location: ";
		private const string COLORTOUCH_SSDP_MAX_AGE_TOKEN = "max-age=";
		private const string COLORTOUCH_SSDP_NAME_TOKEN = "name:";
		private readonly ILogger logger;

		public ICollection<Alert> Alerts { get; set; }
		public double ApiVersion { get; set; }
		public double CoolTemp { get; private set; }
		public double CoolTempMax { get; set; }
		public double CoolTempMin { get; set; }
		public FanSetting FanSetting { get; set; }
		public FanState FanState { get; set; }
		public double HeatTemp { get; private set; }
		public double HeatTempMax { get; set; }
		public double HeatTempMin { get; set; }
		public int Humidity { get; set; }
		public string Id { get; set; }
		public DateTime LastSeen { get; set; }
		public int MaxAgeSeconds { get; set; }
		public Mode Mode { get; set; }
		public string Name { get; set; }
		public double SetPointDelta { get; set; }
		public ScheduledPart SchedulePart { get; set; }
		public ScheduleSetting ScheduleSetting { get; set; }
		public ICollection<Sensor> Sensors { get; set; }
		public double SpaceTemp { get; set; }
		public TempUnits TempUnits { get; set; }
		public Uri Uri { get; set; }

		public Thermostat(string id, string ssdp, ILogger logger)
		{
			this.logger = logger;

			this.Id = id;
			this.LastSeen = DateTime.Now;
			this.MaxAgeSeconds = this.GetMaxAgeSecondsFromSsdp(ssdp);
			this.Name = this.GetNameFromSsdp(ssdp);
			this.Uri = this.GetUriFromSsdp(ssdp);
		}

		public bool IsExpired()
		{
			return DateTime.Now.Subtract(this.LastSeen).TotalSeconds > this.MaxAgeSeconds;
		}

		public void SetCoolTemp(int temp)
		{
			if (temp < this.CoolTempMin || temp > this.CoolTempMax)
			{
				this.logger.Error(String.Format("Error setting cool temp.  Provided temp [{0}] is not within the allowed range of [{1} to {2}].", temp.ToString(), this.CoolTempMin.ToString(), this.CoolTempMax.ToString()));
			}
			else
			{
				this.CoolTemp = temp;
			}
		}

		public void SetHeatTemp(int temp)
		{
			if (temp < this.HeatTempMin || temp > this.HeatTempMax)
			{
				this.logger.Error(String.Format("Error setting heat temp.  Provided temp [{0}] is not within the allowed range of [{1} to {2}].", temp.ToString(), this.HeatTempMin.ToString(), this.HeatTempMax.ToString()));
			}
			else
			{
				this.HeatTemp = temp;
			}
		}

		private int GetMaxAgeSecondsFromSsdp(string ssdp)
		{
			try
			{
				int start = ssdp.IndexOf(COLORTOUCH_SSDP_MAX_AGE_TOKEN) + COLORTOUCH_SSDP_MAX_AGE_TOKEN.Length;
				int end = ssdp.IndexOf('\r', start);

				return Int32.Parse(ssdp.Substring(start, end - start));
			}
			catch (Exception ex)
			{
				this.logger.Error(String.Format("Error parsing thermostat max-age from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}

		private string GetNameFromSsdp(string ssdp)
		{
			try
			{
				int start = ssdp.IndexOf(COLORTOUCH_SSDP_NAME_TOKEN) + COLORTOUCH_SSDP_NAME_TOKEN.Length;
				int end = ssdp.IndexOf(':', start);

				return ssdp.Substring(start, end - start);
			}
			catch(Exception ex)
			{
				this.logger.Error(String.Format("Error parsing thermostat name from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}

		private Uri GetUriFromSsdp(string ssdp)
		{
			try
			{
				int start = ssdp.IndexOf(COLORTOUCH_SSDP_LOCATION_TOKEN) + COLORTOUCH_SSDP_LOCATION_TOKEN.Length;
				int end = ssdp.IndexOf('\r', start);

				return new Uri(ssdp.Substring(start, end - start));
			}
			catch (Exception ex)
			{
				this.logger.Error(String.Format("Error parsing thermostat location from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}
	}
}