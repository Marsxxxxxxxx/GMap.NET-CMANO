using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace BSE.Windows.Forms;

[DesignTimeVisible(true)]
[ToolboxBitmap(typeof(System.Windows.Forms.Splitter))]
public class Splitter : System.Windows.Forms.Splitter
{
	private IContainer components;

	public Splitter()
	{
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		InitializeComponent();
		BackColor = Color.Transparent;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		this.components = new System.ComponentModel.Container();
	}
}
