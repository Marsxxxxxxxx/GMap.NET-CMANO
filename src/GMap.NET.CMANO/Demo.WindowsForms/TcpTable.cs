using System.Collections;
using System.Collections.Generic;

namespace Demo.WindowsForms;

public struct TcpTable(IEnumerable<TcpRow> tcpRows) : IEnumerable<TcpRow>, IEnumerable
{
	private IEnumerable<TcpRow> tcpRows = tcpRows;

	public IEnumerable<TcpRow> Rows => tcpRows;

	public IEnumerator<TcpRow> GetEnumerator()
	{
		return tcpRows.GetEnumerator();
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return tcpRows.GetEnumerator();
	}
}
