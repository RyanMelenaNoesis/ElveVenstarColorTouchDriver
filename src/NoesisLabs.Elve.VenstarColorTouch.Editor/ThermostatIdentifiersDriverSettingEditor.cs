using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.DeviceSettingEditors;
using System;
using System.Windows.Forms;

namespace NoesisLabs.Elve.VenstarColorTouch.Editor
{
	public class ThermostatIdentifiersDriverSettingEditor : IDriverSettingEditor
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
			ILogger logger = new StandardLogger(SharedLibrary.LoggerContextType.Driver, "Venstar", SharedLibrary.LoggerVerbosity.Detailed);
			logger.Info(deviceName);
			logger.Info(parameterName);
			logger.Info(initialValue);

			using (ThermostatIdentifiersDriverSettingEditorForm settingEditorForm = new ThermostatIdentifiersDriverSettingEditorForm(parameterName, attribute, initialValue, logger))
			{
				DialogResult dialogResult = settingEditorForm.ShowDialog(owner);
				this.value = dialogResult != DialogResult.OK ? (string)null : settingEditorForm.Value;
				return dialogResult;
			}
		}
	}
}