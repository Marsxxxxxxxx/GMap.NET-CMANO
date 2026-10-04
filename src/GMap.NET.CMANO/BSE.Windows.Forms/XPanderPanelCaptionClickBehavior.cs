using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design.Behavior;

namespace BSE.Windows.Forms;

internal class XPanderPanelCaptionClickBehavior : Behavior
{
	private XPanderPanel m_xpanderPanel;

	public XPanderPanelCaptionClickBehavior(XPanderPanel xpanderPanel)
	{
		m_xpanderPanel = xpanderPanel;
	}

	public override bool OnMouseDown(Glyph g, MouseButtons button, Point mouseLoc)
	{
		if (m_xpanderPanel != null && !m_xpanderPanel.Expand && m_xpanderPanel.Parent is XPanderPanelList xPanderPanelList)
		{
			xPanderPanelList.Expand(m_xpanderPanel);
			m_xpanderPanel.Invalidate(invalidateChildren: false);
		}
		return true;
	}
}
