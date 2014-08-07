using NoesisLabs.Elve.VenstarColorTouch.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public static class Extensions
	{
		public static string Serialize(this IEnumerable<Thermostat> thermostats)
		{
			var root = new XElement();
		}
	}
}
