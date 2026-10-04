using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace BSE.Windows.Forms;

[ToolStripItemDesignerAvailability(ToolStripItemDesignerAvailability.ToolStrip | ToolStripItemDesignerAvailability.StatusStrip)]
[ToolboxBitmap(typeof(System.Windows.Forms.ProgressBar))]
public class ToolStripProgressBar : ToolStripControlHost
{
	public ProgressBar ProgressBar => Control as ProgressBar;

	public override RightToLeft RightToLeft
	{
		get
		{
			return ProgressBar.RightToLeft;
		}
		set
		{
			ProgressBar.RightToLeft = value;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The color used for the background rectangle of this control.")]
	public Color BackgroundColor
	{
		get
		{
			return ProgressBar.BackgroundColor;
		}
		set
		{
			ProgressBar.BackgroundColor = value;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The end color of the gradient used for the value rectangle of this control.")]
	public Color ValueColor
	{
		get
		{
			return ProgressBar.ValueColor;
		}
		set
		{
			ProgressBar.ValueColor = value;
		}
	}

	[Browsable(true)]
	[Category("Appearance")]
	[Description("The Foreground color used to display text on the progressbar")]
	public override Color ForeColor
	{
		get
		{
			return ProgressBar.ForeColor;
		}
		set
		{
			ProgressBar.ForeColor = value;
		}
	}

	[Category("Appearance")]
	[Description("The font used to display text on the progressbar")]
	public override Font Font
	{
		get
		{
			return ProgressBar.Font;
		}
		set
		{
			ProgressBar.Font = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(100)]
	[RefreshProperties(RefreshProperties.All)]
	[Description("The upper bound of the range this progressbar is working with.")]
	public int Maximum
	{
		get
		{
			return ProgressBar.Maximum;
		}
		set
		{
			ProgressBar.Maximum = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(0)]
	[RefreshProperties(RefreshProperties.All)]
	[Description("The lower bound of the range this progressbar is working with.")]
	public int Minimum
	{
		get
		{
			return ProgressBar.Minimum;
		}
		set
		{
			ProgressBar.Minimum = value;
		}
	}

	[Category("Behavior")]
	[DefaultValue(0)]
	[RefreshProperties(RefreshProperties.All)]
	[Description("The current value for the progressbar, in the range specified by the minimum and maximum.")]
	public int Value
	{
		get
		{
			return ProgressBar.Value;
		}
		set
		{
			ProgressBar.Value = value;
		}
	}

	[Category("Behavior")]
	[Description("The text to display on the progressbar")]
	public override string Text
	{
		get
		{
			return ProgressBar.Text;
		}
		set
		{
			ProgressBar.Text = value;
		}
	}

	protected override Size DefaultSize => new Size(100, 15);

	protected override Padding DefaultMargin
	{
		get
		{
			if (Owner != null && Owner is StatusStrip)
			{
				return new Padding(1, 3, 1, 3);
			}
			return new Padding(1, 1, 1, 2);
		}
	}

	public ToolStripProgressBar()
		: base(CreateControlInstance())
	{
	}

	protected override void OnOwnerChanged(EventArgs e)
	{
		if (Owner != null)
		{
			Owner.RendererChanged += OwnerRendererChanged;
		}
		base.OnOwnerChanged(e);
	}

	private static Control CreateControlInstance()
	{
		return new ProgressBar
		{
			Size = new Size(100, 15)
		};
	}

	private void OwnerRendererChanged(object sender, EventArgs e)
	{
		ToolStripRenderer renderer = Owner.Renderer;
		if (renderer == null)
		{
			return;
		}
		if (renderer is BseRenderer)
		{
			if (renderer is ToolStripProfessionalRenderer toolStripProfessionalRenderer)
			{
				ProgressBar.BorderColor = toolStripProfessionalRenderer.ColorTable.ToolStripBorder;
			}
			if (Owner.GetType() != typeof(StatusStrip))
			{
				Margin = new Padding(1, 1, 1, 3);
			}
		}
		else
		{
			Margin = DefaultMargin;
		}
	}
}
