using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.DeviceSettingEditors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public class ThermostatsDriverSettingEditor : IDriverSettingEditor
	{
		private string value;

		public string Value
		{
			get { return this.value; }
		}

		public string ValueDisplayText
		{
			get { return "Double click to review"; }
		}

		public DialogResult ShowDialog(IWin32Window owner, string deviceName, string parameterName, DriverSettingAttribute attribute, string initialValue)
		{
			using (ThermostatsDriverSettingEditorForm settingEditorForm = new ThermostatsDriverSettingEditorForm(parameterName, attribute, initialValue))
			{
				DialogResult dialogResult = settingEditorForm.ShowDialog(owner);
				this.value = dialogResult != DialogResult.OK ? (string)null : settingEditorForm.Value;
				return dialogResult;
			}
		}
	}
}