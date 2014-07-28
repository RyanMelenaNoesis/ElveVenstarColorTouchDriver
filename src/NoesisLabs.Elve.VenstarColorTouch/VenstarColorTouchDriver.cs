using CodecoreTechnologies.Elve.DriverFramework;
using CodecoreTechnologies.Elve.DriverFramework.Communication;
using CodecoreTechnologies.Elve.DriverFramework.DriverInterfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace NoesisLabs.Elve.VenstarColorTouch
{
	public class VenstarColorTouchDriver : Driver, IClimateControlDriver
	{
		private const string SSDP_DISCOVERY_MESSAGE = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nMan: ssdp:discover\r\nST: colortouch:ecp\r\n";

		private HttpClient http;
		private ICommunication multicastComm;
		private Timer refreshTimer;
		private ICommunication unicastComm;

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

		public override bool StartDriver(Dictionary<string, byte[]> configFileData)
		{
			Logger.Debug("Starting Venstar ColorTouch Driver.");

			try
			{
				var discoveryEndpoint = new IPEndPoint(IPAddress.Parse("239.255.255.250"), 1900);

				var localMulticastEndPoint = new IPEndPoint(IPAddress.Any, 1900);
				var multicastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				multicastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(discoveryEndpoint.Address, IPAddress.Any));
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);
				multicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);
				multicastSocket.Bind(localMulticastEndPoint);
				this.multicastComm = new UdpCommunication(multicastSocket, localMulticastEndPoint);
				this.multicastComm.ReceivedString += Comm_ReceivedString;

				var localUnicastEndPoint = new IPEndPoint(IPAddress.Any, 0);
				var unicastSocket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
				unicastSocket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				unicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 4);
				unicastSocket.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastLoopback, false);
				unicastSocket.Bind(localUnicastEndPoint);
				this.unicastComm = new UdpCommunication(unicastSocket, localUnicastEndPoint);
				this.unicastComm.ReceivedString += Comm_ReceivedString;

				this.multicastComm.Open();
				this.unicastComm.Open();

				this.SendDiscoveryMessage();

				this.http = new HttpClient();

				this.refreshTimer = new System.Timers.Timer();
				this.refreshTimer.Interval = 10 * 1000;
				this.refreshTimer.AutoReset = false;
				this.refreshTimer.Elapsed += new ElapsedEventHandler(this.Refresh);
				this.Refresh(this, (ElapsedEventArgs)ElapsedEventArgs.Empty);

				return true;
			}
			catch(Exception ex)
			{
				Logger.Error("Venstar ColorTouch Driver initialization failed.", ex);
				return false;
			}
			finally
			{
				if (this.refreshTimer != null) { this.refreshTimer.Dispose(); }
				if (this.http != null) { this.http.Dispose(); }
				if (this.multicastComm != null) { this.multicastComm.Dispose(); }
				if (this.unicastComm != null) { this.unicastComm.Dispose(); }
			}
		}

		private void SendDiscoveryMessage()
		{
			try
			{
				this.Logger.Debug("Sending Discovery Message.");

				this.unicastComm.Send(SSDP_DISCOVERY_MESSAGE);
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error sending discovery message.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private void Refresh(object sender, ElapsedEventArgs e)
		{
			try
			{
				this.Logger.Debug("Refreshing Thermostat Values.");

				//TODO: Clean up thermostats past max-age

				//TODO: Refresh thermostat values
			}
			catch (Exception ex)
			{
				this.Logger.Error("Error refreshing thermostat values.", ex);
			}
			finally { this.refreshTimer.Start(); }
		}

		private void Comm_ReceivedString(object sender, ReceivedStringEventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}