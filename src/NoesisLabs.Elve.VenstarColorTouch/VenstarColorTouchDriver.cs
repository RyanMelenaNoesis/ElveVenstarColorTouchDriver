using CodecoreTechnologies.Elve.DriverFramework.Communication;
using CodecoreTechnologies.Elve.DriverFramework.DriverInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public class VenstarColorTouchDriver : IClimateControlDriver
    {
		private readonly ICommunication comm;

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptPagedListCollection PagedListThermostats
		{
			get { throw new NotImplementedException(); }
		}

		public void SetThermostatCoolSetPoint(CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber thermostatID, CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber setPoint)
		{
			throw new NotImplementedException();
		}

		public void SetThermostatFanMode(CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber thermostatID, CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber fanMode)
		{
			throw new NotImplementedException();
		}

		public void SetThermostatHeatSetPoint(CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber thermostatID, CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber setPoint)
		{
			throw new NotImplementedException();
		}

		public void SetThermostatHold(CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber thermostatID, CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptBoolean hold)
		{
			throw new NotImplementedException();
		}

		public void SetThermostatMode(CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber thermostatID, CodecoreTechnologies.Elve.DriverFramework.Scripting.ScriptNumber mode)
		{
			throw new NotImplementedException();
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatCoolSetPoints
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatCurrentTemperatures
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatFanModeTexts
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatFanModes
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatHeatSetPoints
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatHolds
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatModeTexts
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatModes
		{
			get { throw new NotImplementedException(); }
		}

		public CodecoreTechnologies.Elve.DriverFramework.Scripting.IScriptArray ThermostatNames
		{
			get { throw new NotImplementedException(); }
		}
	}
}
