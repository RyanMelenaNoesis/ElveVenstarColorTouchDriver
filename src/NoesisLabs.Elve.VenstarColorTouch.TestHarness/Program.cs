using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CodecoreTechnologies.Elve.DriverFramework.DriverTestHarness;
using CodecoreTechnologies.Elve.DriverFramework.Scripting;


namespace NoesisLabs.Elve.VenstarColorTouch.TestHarness
{
	class Program
	{
		static void Main(string[] args)
		{
			// Prepare any needed configuration files (this is rare).
			Dictionary<string, byte[]> configFiles = new Dictionary<string, byte[]>();
			//configFiles.Add("myfile.xml", ...);

			// Prepare any settings (if the device requires that settings be set)
			TestDeviceSettingDictionary settings = new TestDeviceSettingDictionary();
			//settings.Add(new TestDeviceSetting("SerialPortSetting", "COM1"));

			// Prepare any rules (if you wish to test with rules)
			TestRuleDictionary rules = new TestRuleDictionary();
			//rules.Add(new TestHarnessDriverRule("my rule", true, "TheEventMemberName", new StringDictionary()));

			//**************************************************
			// Create and Start the device.
			//**************************************************
			// TODO: Change the "MyDriver" type below below to the type name of your driver.
			VenstarColorTouchDriver device;
			try
			{
				device = (VenstarColorTouchDriver)DeviceFactory.CreateAndStartDevice(typeof(VenstarColorTouchDriver), configFiles, settings, rules, null);
			}
			catch (Exception ex)
			{
				// An exception occurred while creating or starting the device.
				throw;
			}

			// Test any properties or method here.
			ScriptNumber stage = device.DeviceLifecycleStage;

			// Sleep until the user presses enter.
			Console.ReadLine();

			// Stop the device gracefully.
			device.StopDriver();
		}
	}
}