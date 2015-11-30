using NoesisLabs.Elve.VenstarColorTouch.Enums;
using System;

namespace NoesisLabs.Elve.VenstarColorTouch.Models
{
	public class CommercialThermostatInfo : ThermostatInfo
	{
		public ForceUnoccupied ForceUnoccupied { get; set; }
		public HolidayState HolidayState { get; set; }
		public OverrideState OverrideState { get; set; }
	}
}