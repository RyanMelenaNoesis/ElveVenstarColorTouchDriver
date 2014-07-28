﻿using NoesisLabs.Elve.VenstarColorTouch.Enums;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class ResidentialThermostat : Thermostat
	{
		public AwayState AwayState { get; set; }
		public ResidentialRuntime Runtime { get; set; }
	}
}