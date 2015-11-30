using NoesisLabs.Elve.VenstarColorTouch.Models;
using System;
using System.Collections.Generic;
using System.Xml.Linq;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public static class Extensions
	{
		public static IEnumerable<ThermostatIdentifier> DeserializeToThermostatIdentifiers(this string xml)
		{
			List<ThermostatIdentifier> thermostatIdentifiers = new List<ThermostatIdentifier>();

			foreach (XElement thermostat in XElement.Parse(xml).Elements((XName)"Thermostat"))
			{
				string macAddress = thermostat.Element((XName)"MacAddress").Value;
				string name = thermostat.Element((XName)"Name").Value;
				string url = thermostat.Element((XName)"Url").Value;

				thermostatIdentifiers.Add(new ThermostatIdentifier()
				{
					MacAddress = macAddress,
					Name = name,
					Url = url
				});
			}

			return thermostatIdentifiers;
		}

		public static string Serialize(this IEnumerable<ThermostatIdentifier> thermostatIdentifiers)
		{
			XElement thermostatsNode = new XElement((XName)"Thermostats");

			foreach (ThermostatIdentifier thermostat in thermostatIdentifiers)
			{
				XElement thermostatNode = new XElement((XName)"Thermostat");
				thermostatNode.Add(new XElement((XName)"MacAddress", thermostat.MacAddress));
				thermostatNode.Add(new XElement((XName)"Name", thermostat.Name));
				thermostatNode.Add(new XElement((XName)"Url", thermostat.Url));

				thermostatsNode.Add(thermostatNode);
			}

			return new XDocument(thermostatsNode).ToString();
		}
	}
}