using Newtonsoft.Json;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class ThermostatInfo
	{
		public double ApiVersion { get; set; }
		public double CoolTemp { get; set; }
		public double CoolTempMax { get; set; }
		public double CoolTempMin { get; set; }
		[JsonProperty("fan")]
		public FanSetting FanSetting { get; set; }
		public FanState FanState { get; set; }
		public double HeatTemp { get; set; }
		public double HeatTempMax { get; set; }
		public double HeatTempMin { get; set; }
		public int Humidity { get; set; }
		public Mode Mode { get; set; }
		public string Name { get; set; }
		public ScheduledPart SchedulePart { get; set; }
		[JsonProperty("schedule")]
		public ScheduleSetting ScheduleSetting { get; set; }
		public double SetPointDelta { get; set; }
		public double SpaceTemp { get; set; }
		public TempUnits TempUnits { get; set; }
	}
}