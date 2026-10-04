using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

[Designer(typeof(XPanderPanelDesigner))]
[DesignTimeVisible(false)]
public class XPanderPanel : BasePanel
{
	private Image m_imageChevron;

	private Image m_imageChevronUp;

	private Image m_imageChevronDown;

	private CustomXPanderPanelColors m_customColors;

	private Image m_imageClosePanel;

	private bool m_bIsClosable = true;

	private CaptionStyle m_captionStyle;

	private IContainer components;

	[Description("Gets or sets a value indicating whether the expand icon in a XPanderPanel is visible.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(false)]
	[Browsable(false)]
	[Category("Appearance")]
	public override bool ShowExpandIcon
	{
		get
		{
			return base.ShowExpandIcon;
		}
		set
		{
			base.ShowExpandIcon = value;
		}
	}

	[Description("Gets or sets a value indicating whether the close icon in a XPanderPanel is visible.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[DefaultValue(false)]
	[Browsable(false)]
	[Category("Appearance")]
	public override bool ShowCloseIcon
	{
		get
		{
			return base.ShowCloseIcon;
		}
		set
		{
			base.ShowCloseIcon = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("The custom colors which are used for the XPanderPanel.")]
	[Category("Appearance")]
	public CustomXPanderPanelColors CustomColors => m_customColors;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Browsable(false)]
	public CaptionStyle CaptionStyle
	{
		get
		{
			return m_captionStyle;
		}
		set
		{
			if (!value.Equals(m_captionStyle))
			{
				m_captionStyle = value;
				OnCaptionStyleChanged(this, EventArgs.Empty);
			}
		}
	}

	[Description("Gets or sets a value indicating whether this XPanderPanel is closable.")]
	[DefaultValue(true)]
	[Category("Appearance")]
	public bool IsClosable
	{
		get
		{
			return m_bIsClosable;
		}
		set
		{
			if (!value.Equals(m_bIsClosable))
			{
				m_bIsClosable = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Browsable(false)]
	public new Size Size
	{
		get
		{
			return base.Size;
		}
		set
		{
			base.Size = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override Rectangle DisplayRectangle
	{
		get
		{
			Padding padding = Padding;
			checked
			{
				Rectangle result = new Rectangle(padding.Left + 1, padding.Top + CaptionHeight, ClientRectangle.Width - padding.Left - padding.Right - 2, ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom);
				if (Controls.Count > 0 && Controls[0] is XPanderPanelList { Dock: DockStyle.Fill })
				{
					result = new Rectangle(padding.Left, padding.Top + CaptionHeight, ClientRectangle.Width - padding.Left - padding.Right, ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom - 1);
				}
				return result;
			}
		}
	}

	[Description("The CaptionStyleChanged event occurs when CaptionStyle flags have been changed.")]
	public event EventHandler<EventArgs> CaptionStyleChanged;

	public XPanderPanel()
	{
		InitializeComponent();
		BackColor = Color.Transparent;
		CaptionStyle = CaptionStyle.Normal;
		ForeColor = SystemColors.ControlText;
		Height = CaptionHeight;
		ShowBorder = true;
		m_customColors = new CustomXPanderPanelColors();
		m_customColors.CustomColorsChanged += OnCustomColorsChanged;
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		base.OnPaintBackground(pevent);
		base.BackColor = Color.Transparent;
		Color xPanderPanelBackColor = PanelColors.XPanderPanelBackColor;
		if (xPanderPanelBackColor != Color.Empty && xPanderPanelBackColor != Color.Transparent)
		{
			Rectangle rect = new Rectangle(0, CaptionHeight, ClientRectangle.Width, checked(ClientRectangle.Height - CaptionHeight));
			using SolidBrush brush = new SolidBrush(xPanderPanelBackColor);
			pevent.Graphics.FillRectangle(brush, rect);
		}
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		if (BasePanel.IsZeroWidthOrHeight(CaptionRectangle))
		{
			return;
		}
		using (new UseAntiAlias(e.Graphics))
		{
			Graphics graphics = e.Graphics;
			using (new UseClearTypeGridFit(graphics))
			{
				bool expand = Expand;
				bool showBorder = ShowBorder;
				_ = PanelColors.BorderColor;
				_ = ClientRectangle;
				PanelStyle panelStyle = PanelStyle;
				if ((uint)panelStyle <= 1u)
				{
					DrawCaptionbar(graphics, expand, showBorder, PanelStyle);
					CalculatePanelHeights();
					DrawBorders(graphics, this);
				}
			}
		}
	}

	protected override void OnPanelExpanding(object sender, XPanderStateChangeEventArgs e)
	{
		bool expand = e.Expand;
		if (expand)
		{
			Expand = expand;
			Invalidate(invalidateChildren: false);
		}
		base.OnPanelExpanding(sender, e);
	}

	protected virtual void OnCaptionStyleChanged(object sender, EventArgs e)
	{
		Invalidate(CaptionRectangle);
		if (CaptionStyleChanged != null)
		{
			CaptionStyleChanged(sender, e);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (CaptionRectangle.Contains(e.X, e.Y))
		{
			if (!ShowCloseIcon && !ShowExpandIcon)
			{
				OnExpandClick(this, EventArgs.Empty);
			}
			else if (ShowCloseIcon && !ShowExpandIcon && !RectangleCloseIcon.Contains(e.X, e.Y))
			{
				OnExpandClick(this, EventArgs.Empty);
			}
			if (ShowExpandIcon && RectangleExpandIcon.Contains(e.X, e.Y))
			{
				OnExpandClick(this, EventArgs.Empty);
			}
			if (ShowCloseIcon && m_bIsClosable && RectangleCloseIcon.Contains(e.X, e.Y))
			{
				OnCloseClick(this, EventArgs.Empty);
			}
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		base.OnVisibleChanged(e);
		if (DesignMode)
		{
			return;
		}
		if (!Visible && Expand)
		{
			Expand = false;
			foreach (Control control in Parent.Controls)
			{
				if (control is XPanderPanel { Visible: not false } xPanderPanel)
				{
					xPanderPanel.Expand = true;
					return;
				}
			}
		}
		CalculatePanelHeights();
	}

	private void DrawCaptionbar(Graphics graphics, bool bExpand, bool bShowBorder, PanelStyle panelStyle)
	{
		Rectangle captionRectangle = CaptionRectangle;
		Color color = PanelColors.XPanderPanelCaptionGradientBegin;
		Color color2 = PanelColors.XPanderPanelCaptionGradientEnd;
		Color color3 = PanelColors.XPanderPanelCaptionGradientMiddle;
		Color color4 = PanelColors.XPanderPanelCaptionText;
		Color foreColorCloseIcon = PanelColors.XPanderPanelCaptionCloseIcon;
		Color foreColorExpandIcon = PanelColors.XPanderPanelCaptionExpandIcon;
		bool flag = HoverStateCaptionBar == HoverState.Hover;
		if (m_imageClosePanel == null)
		{
			m_imageClosePanel = Resources.closePanel;
		}
		if (m_imageChevronUp == null)
		{
			m_imageChevronUp = Resources.ChevronUp;
		}
		if (m_imageChevronDown == null)
		{
			m_imageChevronDown = Resources.ChevronDown;
		}
		m_imageChevron = m_imageChevronDown;
		if (bExpand)
		{
			m_imageChevron = m_imageChevronUp;
		}
		if (m_captionStyle == CaptionStyle.Normal)
		{
			if (flag)
			{
				color = PanelColors.XPanderPanelSelectedCaptionBegin;
				color2 = PanelColors.XPanderPanelSelectedCaptionEnd;
				color3 = PanelColors.XPanderPanelSelectedCaptionMiddle;
				if (bExpand)
				{
					color = PanelColors.XPanderPanelPressedCaptionBegin;
					color2 = PanelColors.XPanderPanelPressedCaptionEnd;
					color3 = PanelColors.XPanderPanelPressedCaptionMiddle;
				}
				color4 = PanelColors.XPanderPanelSelectedCaptionText;
				foreColorCloseIcon = color4;
				foreColorExpandIcon = color4;
			}
			else if (bExpand)
			{
				color = PanelColors.XPanderPanelCheckedCaptionBegin;
				color2 = PanelColors.XPanderPanelCheckedCaptionEnd;
				color3 = PanelColors.XPanderPanelCheckedCaptionMiddle;
				color4 = PanelColors.XPanderPanelSelectedCaptionText;
				foreColorCloseIcon = color4;
				foreColorExpandIcon = color4;
			}
			if (panelStyle != PanelStyle.Office2007)
			{
				BasePanel.RenderDoubleBackgroundGradient(graphics, captionRectangle, color, color3, color2, LinearGradientMode.Vertical, flipHorizontal: false);
			}
			else
			{
				BasePanel.RenderButtonBackground(graphics, captionRectangle, color, color3, color2);
			}
		}
		else
		{
			Color xPanderPanelFlatCaptionGradientBegin = PanelColors.XPanderPanelFlatCaptionGradientBegin;
			Color xPanderPanelFlatCaptionGradientEnd = PanelColors.XPanderPanelFlatCaptionGradientEnd;
			_ = PanelColors.InnerBorderColor;
			color4 = PanelColors.XPanderPanelCaptionText;
			foreColorExpandIcon = color4;
			BasePanel.RenderFlatButtonBackground(graphics, captionRectangle, xPanderPanelFlatCaptionGradientBegin, xPanderPanelFlatCaptionGradientEnd, flag);
			DrawInnerBorders(graphics, this);
		}
		BasePanel.DrawImagesAndText(graphics, captionRectangle, LogicalToDeviceUnits(CaptionSpacing), ImageRectangle, Image, RightToLeft, m_bIsClosable, ShowCloseIcon, m_imageClosePanel, foreColorCloseIcon, ref RectangleCloseIcon, ShowExpandIcon, m_imageChevron, foreColorExpandIcon, ref RectangleExpandIcon, CaptionFont, color4, Text);
	}

	private static void DrawBorders(Graphics graphics, XPanderPanel xpanderPanel)
	{
		if (!xpanderPanel.ShowBorder)
		{
			return;
		}
		checked
		{
			using GraphicsPath graphicsPath = new GraphicsPath();
			using (Pen pen = new Pen(xpanderPanel.PanelColors.BorderColor, 1f))
			{
				Rectangle captionRectangle = xpanderPanel.CaptionRectangle;
				Rectangle rectangle = captionRectangle;
				if (xpanderPanel.Expand)
				{
					rectangle = xpanderPanel.ClientRectangle;
					graphics.DrawLine(pen, captionRectangle.Left, captionRectangle.Top + captionRectangle.Height - 1, captionRectangle.Left + captionRectangle.Width, captionRectangle.Top + captionRectangle.Height - 1);
				}
				if (xpanderPanel.Parent is XPanderPanelList { Dock: DockStyle.Fill } xPanderPanelList)
				{
					Panel panel = xPanderPanelList.Parent as Panel;
					XPanderPanel xPanderPanel = xPanderPanelList.Parent as XPanderPanel;
					if ((panel != null && panel.Padding == new Padding(0)) || (xPanderPanel != null && xPanderPanel.Padding == new Padding(0)))
					{
						if (xpanderPanel.Top != 0)
						{
							graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left + captionRectangle.Width, rectangle.Top);
						}
						graphics.DrawLine(pen, rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Top + rectangle.Height);
						graphics.DrawLine(pen, rectangle.Left + rectangle.Width - 1, rectangle.Top, rectangle.Left + rectangle.Width - 1, rectangle.Top + rectangle.Height);
					}
					else
					{
						if (xpanderPanel.Top == 0)
						{
							graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left + rectangle.Width, rectangle.Top);
						}
						graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Top + rectangle.Height);
						graphicsPath.AddLine(rectangle.Left, rectangle.Top + rectangle.Height - 1, rectangle.Left + rectangle.Width - 1, rectangle.Top + rectangle.Height - 1);
						graphicsPath.AddLine(rectangle.Left + rectangle.Width - 1, rectangle.Top, rectangle.Left + rectangle.Width - 1, rectangle.Top + rectangle.Height);
					}
				}
				else
				{
					if (xpanderPanel.Top == 0)
					{
						graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left + rectangle.Width, rectangle.Top);
					}
					graphicsPath.AddLine(rectangle.Left, rectangle.Top, rectangle.Left, rectangle.Top + rectangle.Height);
					graphicsPath.AddLine(rectangle.Left, rectangle.Top + rectangle.Height - 1, rectangle.Left + rectangle.Width - 1, rectangle.Top + rectangle.Height - 1);
					graphicsPath.AddLine(rectangle.Left + rectangle.Width - 1, rectangle.Top, rectangle.Left + rectangle.Width - 1, rectangle.Top + rectangle.Height);
				}
			}
			using Pen pen2 = new Pen(xpanderPanel.PanelColors.BorderColor, 1f);
			graphics.DrawPath(pen2, graphicsPath);
		}
	}

	private static void DrawInnerBorders(Graphics graphics, XPanderPanel xpanderPanel)
	{
		if (!xpanderPanel.ShowBorder)
		{
			return;
		}
		checked
		{
			using GraphicsPath graphicsPath = new GraphicsPath();
			Rectangle captionRectangle = xpanderPanel.CaptionRectangle;
			if (xpanderPanel.Parent is XPanderPanelList { Dock: DockStyle.Fill } xPanderPanelList)
			{
				Panel panel = xPanderPanelList.Parent as Panel;
				XPanderPanel xPanderPanel = xPanderPanelList.Parent as XPanderPanel;
				if ((panel != null && panel.Padding == new Padding(0)) || (xPanderPanel != null && xPanderPanel.Padding == new Padding(0)))
				{
					graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y + captionRectangle.Height, captionRectangle.X, captionRectangle.Y + 1);
					if (xpanderPanel.Top == 0)
					{
						graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y, captionRectangle.X + captionRectangle.Width, captionRectangle.Y);
					}
					else
					{
						graphicsPath.AddLine(captionRectangle.X, captionRectangle.Y + 1, captionRectangle.X + captionRectangle.Width, captionRectangle.Y + 1);
					}
				}
			}
			else
			{
				graphicsPath.AddLine(captionRectangle.X + 1, captionRectangle.Y + captionRectangle.Height, captionRectangle.X + 1, captionRectangle.Y);
				if (xpanderPanel.Top == 0)
				{
					graphicsPath.AddLine(captionRectangle.X + 1, captionRectangle.Y + 1, captionRectangle.X + captionRectangle.Width - 1, captionRectangle.Y + 1);
				}
				else
				{
					graphicsPath.AddLine(captionRectangle.X + 1, captionRectangle.Y, captionRectangle.X + captionRectangle.Width - 1, captionRectangle.Y);
				}
			}
			using Pen pen = new Pen(xpanderPanel.PanelColors.InnerBorderColor);
			graphics.DrawPath(pen, graphicsPath);
		}
	}

	private void CalculatePanelHeights()
	{
		if (Parent == null)
		{
			return;
		}
		int num = Parent.Padding.Top;
		checked
		{
			foreach (Control control in Parent.Controls)
			{
				if (control is XPanderPanel { Visible: not false } xPanderPanel)
				{
					num += xPanderPanel.CaptionHeight;
				}
			}
			num += Parent.Padding.Bottom;
			foreach (Control control2 in Parent.Controls)
			{
				if (control2 is XPanderPanel xPanderPanel2)
				{
					if (xPanderPanel2.Expand)
					{
						xPanderPanel2.Height = Parent.Height + xPanderPanel2.CaptionHeight - num;
					}
					else
					{
						xPanderPanel2.Height = xPanderPanel2.CaptionHeight;
					}
				}
			}
			int num2 = Parent.Padding.Top;
			foreach (Control control3 in Parent.Controls)
			{
				if (control3 is XPanderPanel { Visible: not false } xPanderPanel3)
				{
					xPanderPanel3.Top = num2;
					num2 += xPanderPanel3.Height;
				}
			}
		}
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
		this.BackColor = System.Drawing.Color.FromArgb(214, 223, 247);
	}
}
