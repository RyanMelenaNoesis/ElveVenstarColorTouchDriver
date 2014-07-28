using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System.Collections.Generic;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public abstract class Thermostat
	{
		public ICollection<Alert> Alerts { get; set; }
		public decimal ApiVersion { get; set; }
		public decimal CoolTemp { get; set; }
		public decimal CoolTempMax { get; set; }
		public decimal CoolTempMin { get; set; }
		public FanSetting FanSetting { get; set; }
		public FanState FanState { get; set; }
		public decimal HeatTemp { get; set; }
		public decimal HeatTempMax { get; set; }
		public decimal HeatTempMin { get; set; }
		public int Humidity { get; set; }
		public Mode Mode { get; set; }
		public string Name { get; set; }
		public decimal SetPointDelta { get; set; }
		public ScheduledPart SchedulePart { get; set; }
		public ScheduleSetting ScheduleSetting { get; set; }
		public ICollection<Sensor> Sensors { get; set; }
		public decimal SpaceTemp { get; set; }
		public TempUnits TempUnits { get; set; }
	}
}
