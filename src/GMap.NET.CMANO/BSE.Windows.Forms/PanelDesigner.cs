using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BSE.Windows.Forms;

internal class PanelDesigner : ParentControlDesigner
{
	public override DesignerActionListCollection ActionLists => new DesignerActionListCollection
	{
		new PanelDesignerActionList(Component)
	};

	public override void Initialize(IComponent component)
	{
		base.Initialize(component);
	}

	protected override void OnPaintAdornments(PaintEventArgs e)
	{
		base.OnPaintAdornments(e);
	}
}
