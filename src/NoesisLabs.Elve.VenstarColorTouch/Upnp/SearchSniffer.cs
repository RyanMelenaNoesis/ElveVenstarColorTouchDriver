/*
Copyright 2006 - 2010 Intel Corporation

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

   http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.

FILE HAS BEEN MODIFIED FROM ORIGINAL SOURCE
*/

using System;
using System.Collections;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace NoesisLabs.Elve.VenstarColorTouch.Upnp
{
	/// <summary>
	/// Summary description for UPnPSearchSniffer.
	/// </summary>
	public class SearchSniffer
	{
		public static IPAddress UpnpMulticastV4Addr = IPAddress.Parse("239.255.255.250");
		public static IPEndPoint UpnpMulticastV4EndPoint = new IPEndPoint(UpnpMulticastV4Addr, 1900);
		public static IPAddress UpnpMulticastV6Addr1 = IPAddress.Parse("FF05::C"); // Site local
		public static IPAddress UpnpMulticastV6Addr2 = IPAddress.Parse("FF02::C"); // Link local
		public static IPEndPoint UpnpMulticastV6EndPoint1 = new IPEndPoint(UpnpMulticastV6Addr1, 1900);
		public static IPEndPoint UpnpMulticastV6EndPoint2 = new IPEndPoint(UpnpMulticastV6Addr2, 1900);

		protected Hashtable SSDPSessions = new Hashtable();

		public SearchSniffer()
		{
			IPAddress[] LocalAddresses = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
			ArrayList temp = new ArrayList();
			foreach (IPAddress i in LocalAddresses) temp.Add(i);
			temp.Add(IPAddress.Loopback);
			LocalAddresses = (IPAddress[])temp.ToArray(typeof(IPAddress));

			for (int id = 0; id < LocalAddresses.Length; ++id)
			{
				try
				{
					var localEndPoint = new IPEndPoint(LocalAddresses[id], 1900);
					UdpClient ssdpSession = new UdpClient(localEndPoint);
					ssdpSession.MulticastLoopback = false;
					ssdpSession.EnableBroadcast = true;
					if (localEndPoint.AddressFamily == AddressFamily.InterNetwork)
					{
						ssdpSession.JoinMulticastGroup(UpnpMulticastV4Addr, LocalAddresses[id]);
					}

					uint IOC_IN = 0x80000000;
					uint IOC_VENDOR = 0x18000000;
					uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
					ssdpSession.Client.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

					ssdpSession.BeginReceive(new AsyncCallback(OnReceiveSink), new object[] { ssdpSession, localEndPoint });
					SSDPSessions[ssdpSession] = ssdpSession;
				}
				catch (Exception) { }
			}
		}

		public delegate void PacketHandler(object sender, string Packet, IPEndPoint Local, IPEndPoint From);

		public event PacketHandler OnPacket;

		public void OnReceiveSink(IAsyncResult ar)
		{
			IPEndPoint ep = null;
			UdpClient client = (UdpClient)((object[])ar.AsyncState)[0];
			IPEndPoint localEndPoint = (IPEndPoint)((object[])ar.AsyncState)[1];
			byte[] buf = null;
			try
			{
				buf = client.EndReceive(ar, ref ep);
			}
			catch (Exception) { }
			try
			{
				if (buf != null && OnPacket != null) OnPacket(this, UTF8Encoding.UTF8.GetString(buf, 0, buf.Length), (IPEndPoint)client.Client.LocalEndPoint, ep);
			}
			catch (Exception) { }
			try
			{
				client.BeginReceive(new AsyncCallback(OnReceiveSink), ar.AsyncState);
			}
			catch (Exception) { }
		}

		public void Search()
		{
			Search(UpnpMulticastV4EndPoint);
			Search(UpnpMulticastV6EndPoint1); // Site local
			Search(UpnpMulticastV6EndPoint2); // Link local
		}

		public void Search(IPEndPoint ep)
		{
			string message = "M-SEARCH * HTTP/1.1\r\nHost: 239.255.255.250:1900\r\nMan: ssdp:discover\r\nST: colortouch:ecp\r\n";

			SearchEx(System.Text.UTF8Encoding.UTF8.GetBytes(message), ep);
		}

		public void SearchEx(string text, IPEndPoint ep)
		{
			SearchEx(System.Text.UTF8Encoding.UTF8.GetBytes(text), ep);
		}

		public void SearchEx(byte[] buf, IPEndPoint ep)
		{
			foreach (UdpClient ssdpSession in SSDPSessions.Values)
			{
				try
				{
					if (ssdpSession.Client.AddressFamily != ep.AddressFamily) continue;
					if ((ssdpSession.Client.AddressFamily == AddressFamily.InterNetworkV6) && (((IPEndPoint)ssdpSession.Client.LocalEndPoint).Address.IsIPv6LinkLocal == true && ep.Address != UpnpMulticastV6Addr2)) continue;
					if ((ssdpSession.Client.AddressFamily == AddressFamily.InterNetworkV6) && (((IPEndPoint)ssdpSession.Client.LocalEndPoint).Address.IsIPv6LinkLocal == false && ep.Address != UpnpMulticastV6Addr1)) continue;

					IPEndPoint lep = (IPEndPoint)ssdpSession.Client.LocalEndPoint; // Seems can throw: System.Net.Sockets.SocketException: The requested address is not valid in its context
					if (ssdpSession.Client.AddressFamily == AddressFamily.InterNetwork)
					{
						ssdpSession.Client.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastInterface, lep.Address.GetAddressBytes());
					}
					else if (ssdpSession.Client.AddressFamily == AddressFamily.InterNetworkV6)
					{
						ssdpSession.Client.SetSocketOption(SocketOptionLevel.IPv6, SocketOptionName.MulticastInterface, BitConverter.GetBytes((int)lep.Address.ScopeId));
					}

					ssdpSession.Send(buf, buf.Length, ep);
					ssdpSession.Send(buf, buf.Length, ep);
				}
				catch (SocketException) { }
			}
		}

		public void SearchV4()
		{
			Search(UpnpMulticastV4EndPoint);
		}

		public void SearchV6(string SearchString)
		{
			Search(UpnpMulticastV6EndPoint1); // Site local
			Search(UpnpMulticastV6EndPoint2); // Link local
		}
	}
}