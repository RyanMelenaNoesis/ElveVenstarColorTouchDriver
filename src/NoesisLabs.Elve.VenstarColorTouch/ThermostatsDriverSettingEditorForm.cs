using CodecoreTechnologies.Elve.DriverFramework;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public partial class ThermostatsDriverSettingEditorForm : Form
	{
		private DriverSettingAttribute attribute;
		private string parameterName;

		public ThermostatsDriverSettingEditorForm()
		{
			InitializeComponent();
		}

		public ThermostatsDriverSettingEditorForm(string parameterName, DriverSettingAttribute attribute, string initialXml)
		{
			this.InitializeComponent();

			if (string.IsNullOrEmpty(initialXml)) { return; }

			this.parameterName = parameterName;
			this.attribute = attribute;

			XElement xelement = XElement.Parse(initialXml);

			foreach (XElement thermostat in xelement.Elements((XName) "Thermostats"))
			{
				string macAddress = thermostat.Element((XName)"MacAddress").Value;
				string name = thermostat.Element((XName)"Name").Value;
				string url = thermostat.Element((XName)"Ulr").Value;

				this.AddDataGridViewRow(macAddress, name, url);
			}
		}

		private void AddDataGridViewRow(string macAddress, string name, string url)
		{
			this.dataGridView1.Rows.Add((object)((object)macAddress, (object)name, (object)url);
			this._knownUIContexts.Add((object)uiContext, (object)null);
		}
	}
}
