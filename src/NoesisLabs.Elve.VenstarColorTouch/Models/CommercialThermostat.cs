﻿using CodecoreTechnologies.Elve.DriverFramework;
using NoesisLabs.Elve.VenstarColorTouch.Enums;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class CommercialThermostat : Thermostat
	{
		public ForceUnoccupied ForceUnoccupied { get; set; }
		public HolidayState HolidayState { get; set; }
		public OverrideState OverrideState { get; set; }
		public CommercialRuntime Runtime { get; set; }

		public CommercialThermostat(string id, string ssdpResponse, ILogger logger)
			: base(id, ssdpResponse, logger)
		{

		}
	}
}