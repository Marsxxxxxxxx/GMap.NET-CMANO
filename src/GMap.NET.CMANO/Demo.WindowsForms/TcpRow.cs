using System.Net;
using System.Net.NetworkInformation;

namespace Demo.WindowsForms;

public struct TcpRow
{
	private IPEndPoint localEndPoint;

	private IPEndPoint remoteEndPoint;

	private TcpState state;

	private int processId;

	public IPEndPoint LocalEndPoint => localEndPoint;

	public IPEndPoint RemoteEndPoint => remoteEndPoint;

	public TcpState State => state;

	public int ProcessId => processId;

	public TcpRow(IpHelper.TcpRow tcpRow)
	{
		state = tcpRow.state;
		processId = tcpRow.owningPid;
		checked
		{
			int port = (tcpRow.localPort1 << 8) + tcpRow.localPort2 + (tcpRow.localPort3 << 24) + (tcpRow.localPort4 << 16);
			long address = tcpRow.localAddr;
			localEndPoint = new IPEndPoint(address, port);
			int port2 = (tcpRow.remotePort1 << 8) + tcpRow.remotePort2 + (tcpRow.remotePort3 << 24) + (tcpRow.remotePort4 << 16);
			long address2 = tcpRow.remoteAddr;
			remoteEndPoint = new IPEndPoint(address2, port2);
		}
	}
}
