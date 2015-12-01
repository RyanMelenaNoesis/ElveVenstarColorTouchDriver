using CodecoreTechnologies.Elve.DriverFramework;
using NoesisLabs.Elve.VenstarColorTouch.Editor.Upnp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows.Forms;

namespace NoesisLabs.Elve.VenstarColorTouch.Editor
{
	public partial class ThermostatIdentifiersDriverSettingEditorForm : Form
	{
		private const int COLORTOUCH_SSDP_IDENTIFIER_LENGTH = 17;
		private const string COLORTOUCH_SSDP_IDENTIFIER_TOKEN = "ecp:";
		private const string COLORTOUCH_SSDP_KEYWORD = "colortouch:ecp";
		private const string COLORTOUCH_SSDP_LOCATION_TOKEN = "Location: ";
		private const string COLORTOUCH_SSDP_MAX_AGE_TOKEN = "max-age=";
		private const string COLORTOUCH_SSDP_NAME_TOKEN = "name:";

		private DriverSettingAttribute attribute;
		private ILogger logger;
		private string parameterName;
		private SearchSniffer sniffer;

		public ThermostatIdentifiersDriverSettingEditorForm()
		{
			InitializeComponent();

			this.sniffer = new SearchSniffer();
		}

		public ThermostatIdentifiersDriverSettingEditorForm(string parameterName, DriverSettingAttribute attribute, string initialXml, ILogger logger) : this()
		{
			this.logger = logger;

			if (string.IsNullOrEmpty(initialXml)) { return; }

			this.parameterName = parameterName;
			this.attribute = attribute;

			List<ThermostatIdentifier> thermostatIdentifiers = initialXml.DeserializeToThermostatIdentifiers().ToList();

			foreach (ThermostatIdentifier thermostatIdentifier in thermostatIdentifiers)
			{
				this.AddDataGridViewRow(thermostatIdentifier.MacAddress, thermostatIdentifier.Name, thermostatIdentifier.Url);
			}
		}

		public string Value
		{
			get
			{
				List<ThermostatIdentifier> thermostatIdentifiers = new List<ThermostatIdentifier>();

				foreach (DataGridViewRow row in this.dataGridView1.Rows)
				{
					if (row.IsNewRow) { continue; }

					ThermostatIdentifier thermostatIdentifier = new ThermostatIdentifier()
					{
						MacAddress = (string)row.Cells[0].Value,
						Name = (string)row.Cells[1].Value,
						Url = (string)row.Cells[2].Value
					};

					thermostatIdentifiers.Add(thermostatIdentifier);
				}

				return thermostatIdentifiers.Serialize();
			}
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{

				if ((components != null))
				{
					components.Dispose();
				}

				this.sniffer = null;
			}

			base.Dispose(disposing);
		}

		private static string GetMacAddressFromSsdp(string ssdp, ILogger logger)
		{
			try
			{
				return ssdp.Substring(ssdp.IndexOf(COLORTOUCH_SSDP_IDENTIFIER_TOKEN) + COLORTOUCH_SSDP_IDENTIFIER_TOKEN.Length, COLORTOUCH_SSDP_IDENTIFIER_LENGTH);
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Error parsing thermostat identifier from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}

		private static string GetNameFromSsdp(string ssdp, ILogger logger)
		{
			try
			{
				int start = ssdp.IndexOf(COLORTOUCH_SSDP_NAME_TOKEN) + COLORTOUCH_SSDP_NAME_TOKEN.Length;
				int end = ssdp.IndexOf(':', start);

				return ssdp.Substring(start, end - start);
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Error parsing thermostat name from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}

		private static Uri GetUriFromSsdp(string ssdp, ILogger logger)
		{
			try
			{
				int start = ssdp.IndexOf(COLORTOUCH_SSDP_LOCATION_TOKEN) + COLORTOUCH_SSDP_LOCATION_TOKEN.Length;
				int end = ssdp.IndexOf('\r', start);

				return new Uri(ssdp.Substring(start, end - start));
			}
			catch (Exception ex)
			{
				logger.Error(String.Format("Error parsing thermostat location from SSDP response [{0}].", ssdp), ex);
				throw ex;
			}
		}

		private void AddDataGridViewRow(string macAddress, string name, string url)
		{
			if (!this.dataGridView1.Rows.Cast<DataGridViewRow>().Any(r => r.Cells.Count > 0 && r.Cells[0].Value != null && r.Cells[0].Value.ToString() == macAddress))
			{
				this.dataGridView1.Rows.Add(new object[] { (object)macAddress, (object)name, (object)url });
			}
		}

		private void DiscoverButton_Click(object sender, EventArgs e)
		{
			this.sniffer.OnPacket += Sniffer_OnPacket;
			this.sniffer.SearchV4();
		}

		private void Sniffer_OnPacket(object sender, string packet, IPEndPoint local, IPEndPoint from)
		{
			if (packet.Contains(COLORTOUCH_SSDP_KEYWORD))
			{
				string macAddress = GetMacAddressFromSsdp(packet, this.logger);
				string name = GetNameFromSsdp(packet, this.logger);
				string url = GetUriFromSsdp(packet, this.logger).ToString();

				this.AddDataGridViewRow(macAddress, name, url);
			}
		}
	}
}