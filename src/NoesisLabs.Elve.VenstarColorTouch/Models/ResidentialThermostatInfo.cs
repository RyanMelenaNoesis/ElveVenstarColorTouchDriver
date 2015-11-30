using Newtonsoft.Json;
using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class ResidentialThermostatInfo : ThermostatInfo
	{
		[JsonProperty("away")]
		public AwayState AwayState { get; set; }
	}
}