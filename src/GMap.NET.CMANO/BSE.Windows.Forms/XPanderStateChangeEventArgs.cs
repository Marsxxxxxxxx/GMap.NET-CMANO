using System;

namespace BSE.Windows.Forms;

public class XPanderStateChangeEventArgs : EventArgs
{
	private bool m_bExpand;

	public bool Expand => m_bExpand;

	public XPanderStateChangeEventArgs(bool bExpand)
	{
		m_bExpand = bExpand;
	}
}
