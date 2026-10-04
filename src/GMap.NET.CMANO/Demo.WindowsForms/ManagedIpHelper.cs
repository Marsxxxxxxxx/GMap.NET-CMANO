using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Demo.WindowsForms;

public static class ManagedIpHelper
{
	public static readonly List<TcpRow> TcpRows = new List<TcpRow>();

	public static void UpdateExtendedTcpTable(bool sorted)
	{
		TcpRows.Clear();
		IntPtr intPtr = IntPtr.Zero;
		int tcpTableLength = 0;
		if (IpHelper.GetExtendedTcpTable(intPtr, ref tcpTableLength, sorted, 2, IpHelper.TcpTableType.OwnerPidConnections, 0) == 0)
		{
			return;
		}
		checked
		{
			try
			{
				intPtr = Marshal.AllocHGlobal(tcpTableLength);
				if (IpHelper.GetExtendedTcpTable(intPtr, ref tcpTableLength, sort: true, 2, IpHelper.TcpTableType.OwnerPidConnections, 0) == 0)
				{
					IpHelper.TcpTable tcpTable = (IpHelper.TcpTable)Marshal.PtrToStructure(intPtr, typeof(IpHelper.TcpTable));
					IntPtr intPtr2 = (IntPtr)((long)intPtr + Marshal.SizeOf((object)tcpTable.Length));
					for (int i = 0; i < tcpTable.Length; i++)
					{
						TcpRows.Add(new TcpRow((IpHelper.TcpRow)Marshal.PtrToStructure(intPtr2, typeof(IpHelper.TcpRow))));
						intPtr2 = (IntPtr)((long)intPtr2 + Marshal.SizeOf(typeof(IpHelper.TcpRow)));
					}
				}
			}
			finally
			{
				if (intPtr != IntPtr.Zero)
				{
					Marshal.FreeHGlobal(intPtr);
				}
			}
		}
	}
}
