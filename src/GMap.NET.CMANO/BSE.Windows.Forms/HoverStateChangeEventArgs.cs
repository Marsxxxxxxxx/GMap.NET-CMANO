using System;

namespace BSE.Windows.Forms;

public class HoverStateChangeEventArgs : EventArgs
{
	private HoverState m_hoverState;

	public HoverState HoverState => m_hoverState;

	public HoverStateChangeEventArgs(HoverState hoverState)
	{
		m_hoverState = hoverState;
	}
}
