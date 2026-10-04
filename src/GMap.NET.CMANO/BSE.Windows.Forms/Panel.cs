using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

[Designer(typeof(PanelDesigner))]
[DesignTimeVisible(true)]
[DefaultEvent("CloseClick")]
[ToolboxBitmap(typeof(System.Windows.Forms.Panel))]
public class Panel : BasePanel
{
	private Rectangle m_restoreBounds;

	private bool m_bShowTransparentBackground;

	private bool m_bShowXPanderPanelProfessionalStyle;

	private bool m_bShowCaptionbar;

	private LinearGradientMode m_linearGradientMode;

	private Image m_imageClosePanel;

	private CustomPanelColors m_customColors;

	private Image m_imgHoverBackground;

	private System.Windows.Forms.Splitter m_associatedSplitter;

	private IContainer components;

	[Description("The associated Splitter.")]
	[Category("Behavior")]
	public virtual System.Windows.Forms.Splitter AssociatedSplitter
	{
		get
		{
			return m_associatedSplitter;
		}
		set
		{
			m_associatedSplitter = value;
		}
	}

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
	[Description("The custom colors which are used for the panel.")]
	[Category("Appearance")]
	public CustomPanelColors CustomColors => m_customColors;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override bool Expand
	{
		get
		{
			return base.Expand;
		}
		set
		{
			base.Expand = value;
		}
	}

	[Description("LinearGradientMode of the Panels background")]
	[DefaultValue(1)]
	[Category("Appearance")]
	public LinearGradientMode LinearGradientMode
	{
		get
		{
			return m_linearGradientMode;
		}
		set
		{
			if (!value.Equals(m_linearGradientMode))
			{
				m_linearGradientMode = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Description("A value indicating whether the panels captionbar is displayed.")]
	[DefaultValue(true)]
	[Category("Behavior")]
	public bool ShowCaptionbar
	{
		get
		{
			return m_bShowCaptionbar;
		}
		set
		{
			if (!value.Equals(m_bShowCaptionbar))
			{
				m_bShowCaptionbar = value;
				Invalidate(invalidateChildren: true);
			}
		}
	}

	[Description("Gets or sets a value indicating whether the controls background is transparent")]
	[DefaultValue(true)]
	[Category("Behavior")]
	public bool ShowTransparentBackground
	{
		get
		{
			return m_bShowTransparentBackground;
		}
		set
		{
			if (!value.Equals(m_bShowTransparentBackground))
			{
				m_bShowTransparentBackground = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Description("Gets or sets a value indicating whether the controls caption professional colorscheme is the same then the XPanderPanels")]
	[DefaultValue(false)]
	[Category("Behavior")]
	public bool ShowXPanderPanelProfessionalStyle
	{
		get
		{
			return m_bShowXPanderPanelProfessionalStyle;
		}
		set
		{
			if (!value.Equals(m_bShowXPanderPanelProfessionalStyle))
			{
				m_bShowXPanderPanelProfessionalStyle = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Browsable(false)]
	public Rectangle RestoreBounds => m_restoreBounds;

	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override Rectangle DisplayRectangle
	{
		get
		{
			Padding padding = Padding;
			checked
			{
				Rectangle result = new Rectangle(ClientRectangle.Left + padding.Left, ClientRectangle.Top + padding.Top, ClientRectangle.Width - padding.Left - padding.Right, ClientRectangle.Height - padding.Top - padding.Bottom);
				if (m_bShowCaptionbar && Controls.Count > 0)
				{
					result = ((!(Controls[0] is XPanderPanelList { Dock: DockStyle.Fill })) ? new Rectangle(padding.Left + 1, CaptionHeight + padding.Top + 1, ClientRectangle.Width - padding.Left - padding.Right - 2, ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom - 2) : new Rectangle(padding.Left, CaptionHeight + padding.Top + 1, ClientRectangle.Width - padding.Left - padding.Right, ClientRectangle.Height - CaptionHeight - padding.Top - padding.Bottom - 2));
				}
				return result;
			}
		}
	}

	public Panel()
	{
		InitializeComponent();
		CaptionFont = new Font(SystemFonts.CaptionFont.FontFamily, SystemFonts.CaptionFont.SizeInPoints + 2.75f, FontStyle.Bold);
		BackColor = Color.Transparent;
		ForeColor = SystemColors.ControlText;
		ShowTransparentBackground = true;
		ShowXPanderPanelProfessionalStyle = false;
		ColorScheme = ColorScheme.Professional;
		LinearGradientMode = LinearGradientMode.Vertical;
		Expand = true;
		CaptionHeight = 27;
		ImageSize = new Size(18, 18);
		m_bShowCaptionbar = true;
		m_customColors = new CustomPanelColors();
		m_customColors.CustomColorsChanged += OnCustomColorsChanged;
	}

	public override void SetPanelProperties(PanelColors panelColors)
	{
		m_imgHoverBackground = null;
		base.SetPanelProperties(panelColors);
	}

	protected override void OnExpandClick(object sender, EventArgs e)
	{
		Expand = !Expand;
		base.OnExpandClick(sender, e);
	}

	protected override void OnExpandIconHoverStateChanged(object sender, HoverStateChangeEventArgs e)
	{
		Invalidate(RectangleExpandIcon);
		base.OnExpandIconHoverStateChanged(sender, e);
	}

	protected override void OnCloseIconHoverStateChanged(object sender, HoverStateChangeEventArgs e)
	{
		Invalidate(RectangleCloseIcon);
		base.OnCloseIconHoverStateChanged(sender, e);
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		if (ShowTransparentBackground)
		{
			base.OnPaintBackground(pevent);
			BackColor = Color.Transparent;
			return;
		}
		Rectangle bounds = ClientRectangle;
		if (m_bShowCaptionbar)
		{
			BackColor = Color.Transparent;
			bounds = checked(new Rectangle(ClientRectangle.Left, ClientRectangle.Top + CaptionHeight, ClientRectangle.Width, ClientRectangle.Height - CaptionHeight));
		}
		BasePanel.RenderBackgroundGradient(pevent.Graphics, bounds, PanelColors.PanelContentGradientBegin, PanelColors.PanelContentGradientEnd, LinearGradientMode);
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		PanelStyle panelStyle = PanelStyle;
		if (!m_bShowCaptionbar)
		{
			return;
		}
		using (new UseAntiAlias(e.Graphics))
		{
			Graphics graphics = e.Graphics;
			using (new UseClearTypeGridFit(graphics))
			{
				Rectangle captionRectangle = CaptionRectangle;
				Color colorGradientBegin = PanelColors.PanelCaptionGradientBegin;
				Color colorGradientEnd = PanelColors.PanelCaptionGradientEnd;
				Color colorGradientMiddle = PanelColors.PanelCaptionGradientMiddle;
				Color color = PanelColors.PanelCaptionText;
				bool showXPanderPanelProfessionalStyle = ShowXPanderPanelProfessionalStyle;
				ColorScheme colorScheme = ColorScheme;
				if (showXPanderPanelProfessionalStyle && colorScheme == ColorScheme.Professional && panelStyle != PanelStyle.Office2007)
				{
					colorGradientBegin = PanelColors.XPanderPanelCaptionGradientBegin;
					colorGradientEnd = PanelColors.XPanderPanelCaptionGradientEnd;
					colorGradientMiddle = PanelColors.XPanderPanelCaptionGradientMiddle;
					color = PanelColors.XPanderPanelCaptionText;
				}
				Image image = Image;
				RightToLeft rightToLeft = RightToLeft;
				Font captionFont = CaptionFont;
				Rectangle clientRectangle = ClientRectangle;
				string strCaptionText = Text;
				DockStyle dock = Dock;
				bool expand = Expand;
				if (m_imageClosePanel == null)
				{
					m_imageClosePanel = Resources.closePanel;
				}
				Color color2 = PanelColors.PanelCaptionCloseIcon;
				if (color2 == Color.Empty)
				{
					color2 = color;
				}
				bool showExpandIcon = ShowExpandIcon;
				bool showCloseIcon = ShowCloseIcon;
				if ((uint)panelStyle <= 1u)
				{
					DrawStyleDefault(graphics, captionRectangle, colorGradientBegin, colorGradientEnd, colorGradientMiddle);
				}
				DrawBorder(graphics, clientRectangle, captionRectangle, PanelColors.BorderColor, PanelColors.InnerBorderColor);
				if (dock == DockStyle.Fill || dock == DockStyle.None || (!showExpandIcon && !showCloseIcon))
				{
					BasePanel.DrawImagesAndText(graphics, captionRectangle, LogicalToDeviceUnits(CaptionSpacing), ImageRectangle, image, rightToLeft, captionFont, color, strCaptionText);
				}
				else
				{
					if (!(showExpandIcon | showCloseIcon))
					{
						return;
					}
					Image expandImage = GetExpandImage(dock, expand);
					BasePanel.DrawImagesAndText(graphics, dock, LogicalToDeviceUnits(CaptionSpacing), captionRectangle, clientRectangle, ImageRectangle, image, rightToLeft, showCloseIcon, m_imageClosePanel, color2, ref RectangleCloseIcon, showExpandIcon, expand, expandImage, color, ref RectangleExpandIcon, captionFont, color, PanelColors.PanelCollapsedCaptionText, strCaptionText);
					if (m_imgHoverBackground == null)
					{
						m_imgHoverBackground = GetPanelIconBackground(graphics, ImageRectangle, PanelColors.PanelCaptionSelectedGradientBegin, PanelColors.PanelCaptionSelectedGradientEnd);
					}
					if (m_imgHoverBackground != null)
					{
						Rectangle rectangleCloseIcon = RectangleCloseIcon;
						if (rectangleCloseIcon != Rectangle.Empty && HoverStateCloseIcon == HoverState.Hover)
						{
							graphics.DrawImage(m_imgHoverBackground, rectangleCloseIcon);
							BasePanel.DrawIcon(graphics, m_imageClosePanel, rectangleCloseIcon, color2, rectangleCloseIcon.Y);
						}
						Rectangle rectangleExpandIcon = RectangleExpandIcon;
						if (rectangleExpandIcon != Rectangle.Empty && HoverStateExpandIcon == HoverState.Hover)
						{
							graphics.DrawImage(m_imgHoverBackground, rectangleExpandIcon);
							BasePanel.DrawIcon(graphics, expandImage, rectangleExpandIcon, color, rectangleExpandIcon.Y);
						}
					}
				}
			}
		}
	}

	protected override void OnPanelCollapsing(object sender, XPanderStateChangeEventArgs e)
	{
		if (Dock == DockStyle.Left || Dock == DockStyle.Right)
		{
			foreach (Control control in Controls)
			{
				control.Hide();
			}
		}
		if (Dock == DockStyle.Left || Dock == DockStyle.Right)
		{
			if (ClientRectangle.Width > CaptionHeight)
			{
				m_restoreBounds = ClientRectangle;
			}
			Width = CaptionHeight;
		}
		if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
		{
			if (ClientRectangle.Height > CaptionHeight)
			{
				m_restoreBounds = ClientRectangle;
			}
			Height = CaptionHeight;
		}
		base.OnPanelCollapsing(sender, e);
	}

	protected override void OnPanelExpanding(object sender, XPanderStateChangeEventArgs e)
	{
		if (Dock == DockStyle.Left || Dock == DockStyle.Right)
		{
			foreach (Control control in Controls)
			{
				control.Show();
			}
			if (ClientRectangle.Width == CaptionHeight)
			{
				Width = m_restoreBounds.Width;
			}
		}
		if (Dock == DockStyle.Top || Dock == DockStyle.Bottom)
		{
			Height = m_restoreBounds.Height;
		}
		base.OnPanelExpanding(sender, e);
	}

	protected override void OnPanelStyleChanged(object sender, PanelStyleChangeEventArgs e)
	{
		OnLayout(new LayoutEventArgs(this, null));
		base.OnPanelStyleChanged(sender, e);
	}

	protected override void OnCreateControl()
	{
		m_restoreBounds = ClientRectangle;
		MinimumSize = new Size(CaptionHeight, CaptionHeight);
		base.OnCreateControl();
	}

	protected override void OnResize(EventArgs e)
	{
		if (ShowExpandIcon)
		{
			if (!Expand)
			{
				if ((Dock == DockStyle.Left || Dock == DockStyle.Right) && Width > CaptionHeight)
				{
					Expand = true;
				}
				if ((Dock == DockStyle.Top || Dock == DockStyle.Bottom) && Height > CaptionHeight)
				{
					Expand = true;
				}
			}
			else
			{
				if ((Dock == DockStyle.Left || Dock == DockStyle.Right) && Width == CaptionHeight)
				{
					Expand = false;
				}
				if ((Dock == DockStyle.Top || Dock == DockStyle.Bottom) && Height == CaptionHeight)
				{
					Expand = false;
				}
			}
		}
		base.OnResize(e);
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		System.Windows.Forms.Splitter associatedSplitter = AssociatedSplitter;
		if (associatedSplitter != null)
		{
			associatedSplitter.Visible = Visible;
		}
		base.OnVisibleChanged(e);
	}

	private static Image GetPanelIconBackground(Graphics graphics, Rectangle rectanglePanelIcon, Color backgroundColorBegin, Color backgroundColorEnd)
	{
		Rectangle bounds = rectanglePanelIcon;
		bounds.X = 0;
		bounds.Y = 0;
		Image image = new Bitmap(rectanglePanelIcon.Width, rectanglePanelIcon.Height, graphics);
		using Graphics graphics2 = Graphics.FromImage(image);
		BasePanel.RenderBackgroundGradient(graphics2, bounds, backgroundColorBegin, backgroundColorEnd, LinearGradientMode.Vertical);
		return image;
	}

	private static void DrawStyleDefault(Graphics graphics, Rectangle captionRectangle, Color colorGradientBegin, Color colorGradientEnd, Color colorGradientMiddle)
	{
		BasePanel.RenderDoubleBackgroundGradient(graphics, captionRectangle, colorGradientBegin, colorGradientMiddle, colorGradientEnd, LinearGradientMode.Vertical, flipHorizontal: true);
	}

	private static void DrawBorder(Graphics graphics, Rectangle panelRectangle, Rectangle captionRectangle, Color borderColor, Color innerBorderColor)
	{
		checked
		{
			using Pen pen = new Pen(borderColor);
			Rectangle bounds = captionRectangle;
			bounds.Width--;
			bounds.Offset(1, 1);
			ControlPaint.DrawBorder(graphics, bounds, innerBorderColor, ButtonBorderStyle.Solid);
			ControlPaint.DrawBorder(graphics, panelRectangle, borderColor, ButtonBorderStyle.Solid);
			graphics.DrawLine(pen, captionRectangle.X, captionRectangle.Y + captionRectangle.Height, captionRectangle.Width, captionRectangle.Y + captionRectangle.Height);
			if (panelRectangle.Height != captionRectangle.Height)
			{
				Rectangle rectangle = panelRectangle;
				rectangle.Y = captionRectangle.Height;
				rectangle.Height -= captionRectangle.Height + (int)pen.Width;
				rectangle.Width -= (int)pen.Width;
				Point[] points = new Point[4]
				{
					new Point(rectangle.X, rectangle.Y),
					new Point(rectangle.X, rectangle.Y + rectangle.Height),
					new Point(rectangle.X + rectangle.Width, rectangle.Y + rectangle.Height),
					new Point(rectangle.X + rectangle.Width, rectangle.Y)
				};
				graphics.DrawLines(pen, points);
			}
		}
	}

	private static Image GetExpandImage(DockStyle dockStyle, bool bIsExpanded)
	{
		Image result = null;
		if ((dockStyle == DockStyle.Left) & bIsExpanded)
		{
			result = Resources.ChevronLeft;
		}
		else if (dockStyle == DockStyle.Left && !bIsExpanded)
		{
			result = Resources.ChevronRight;
		}
		else if ((dockStyle == DockStyle.Right) & bIsExpanded)
		{
			result = Resources.ChevronRight;
		}
		else if (dockStyle == DockStyle.Right && !bIsExpanded)
		{
			result = Resources.ChevronLeft;
		}
		else if ((dockStyle == DockStyle.Top) & bIsExpanded)
		{
			result = Resources.ChevronUp;
		}
		else if (dockStyle == DockStyle.Top && !bIsExpanded)
		{
			result = Resources.ChevronDown;
		}
		else if ((dockStyle == DockStyle.Bottom) & bIsExpanded)
		{
			result = Resources.ChevronDown;
		}
		else if (dockStyle == DockStyle.Bottom && !bIsExpanded)
		{
			result = Resources.ChevronUp;
		}
		return result;
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
		base.Size = new System.Drawing.Size(200, 100);
	}
}
