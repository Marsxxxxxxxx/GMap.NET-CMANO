using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Text;

namespace Demo.WindowsForms;

internal class TraceRoute
{
	private static readonly string Data;

	private static readonly byte[] DataBuffer;

	private static readonly int timeout;

	static TraceRoute()
	{
		Data = "aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa";
		timeout = 8888;
		DataBuffer = Encoding.ASCII.GetBytes(Data);
	}

	public static List<PingReply> GetTraceRoute(string hostNameOrAddress)
	{
		return GetTraceRoute(hostNameOrAddress, 1);
	}

	private static List<PingReply> GetTraceRoute(string hostNameOrAddress, int ttl)
	{
		List<PingReply> list = new List<PingReply>();
		using (Ping ping = new Ping())
		{
			PingOptions options = new PingOptions(ttl, dontFragment: true);
			PingReply pingReply = ping.Send(hostNameOrAddress, timeout, DataBuffer, options);
			if (pingReply.Status == IPStatus.Success)
			{
				list.Add(pingReply);
			}
			else if (pingReply.Status == IPStatus.TtlExpired)
			{
				list.Add(pingReply);
				list.AddRange(GetTraceRoute(hostNameOrAddress, checked(ttl + 1)));
			}
		}
		return list;
	}
}
