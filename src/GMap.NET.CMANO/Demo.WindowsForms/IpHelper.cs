using System;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

namespace Demo.WindowsForms;

public static class IpHelper
{
	public enum TcpTableType
	{
		BasicListener,
		BasicConnections,
		BasicAll,
		OwnerPidListener,
		OwnerPidConnections,
		OwnerPidAll,
		OwnerModuleListener,
		OwnerModuleConnections,
		OwnerModuleAll
	}

	public struct TcpTable
	{
		public uint Length;

		public TcpRow row;
	}

	public struct TcpRow
	{
		public TcpState state;

		public uint localAddr;

		public byte localPort1;

		public byte localPort2;

		public byte localPort3;

		public byte localPort4;

		public uint remoteAddr;

		public byte remotePort1;

		public byte remotePort2;

		public byte remotePort3;

		public byte remotePort4;

		public int owningPid;
	}

	public const string DllName = "iphlpapi.dll";

	public const int AfInet = 2;

	[DllImport("iphlpapi.dll", SetLastError = true)]
	public static extern uint GetExtendedTcpTable(IntPtr tcpTable, ref int tcpTableLength, bool sort, int ipVersion, TcpTableType tcpTableType, int reserved);
}
