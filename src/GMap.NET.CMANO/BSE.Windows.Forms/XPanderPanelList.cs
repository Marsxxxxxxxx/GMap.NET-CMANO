using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

[Designer(typeof(XPanderPanelListDesigner))]
[DesignTimeVisible(true)]
[ToolboxBitmap(typeof(System.Windows.Forms.Panel))]
public class XPanderPanelList : ScrollableControl
{
	private bool m_bShowBorder;

	private bool m_bShowGradientBackground;

	private bool m_bShowExpandIcon;

	private bool m_bShowCloseIcon;

	private int m_iCaptionHeight;

	private LinearGradientMode m_linearGradientMode;

	private Color m_colorGradientBackground;

	private CaptionStyle m_captionStyle;

	private PanelStyle m_ePanelStyle;

	private ColorScheme m_eColorScheme;

	private XPanderPanelCollection m_xpanderPanels;

	private PanelColors m_panelColors;

	private IContainer components;

	[RefreshProperties(RefreshProperties.Repaint)]
	[Category("Collections")]
	[Browsable(true)]
	[Description("Collection containing all the XPanderPanels for the xpanderpanellist.")]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	[Editor(typeof(XPanderPanelCollectionEditor), typeof(UITypeEditor))]
	public XPanderPanelCollection XPanderPanels => m_xpanderPanels;

	[Description("Specifies the style of the xpanderpanels in this xpanderpanellist.")]
	[DefaultValue(PanelStyle.Default)]
	[Category("Appearance")]
	public PanelStyle PanelStyle
	{
		get
		{
			return m_ePanelStyle;
		}
		set
		{
			if (value != m_ePanelStyle)
			{
				m_ePanelStyle = value;
				OnPanelStyleChanged(this, new PanelStyleChangeEventArgs(m_ePanelStyle));
			}
		}
	}

	public PanelColors PanelColors
	{
		get
		{
			return m_panelColors;
		}
		set
		{
			m_panelColors = value;
		}
	}

	[Description("The colorscheme of the xpanderpanels in the xpanderpanellist")]
	[DefaultValue(ColorScheme.Professional)]
	[Category("Appearance")]
	public ColorScheme ColorScheme
	{
		get
		{
			return m_eColorScheme;
		}
		set
		{
			if (value != m_eColorScheme)
			{
				m_eColorScheme = value;
				OnColorSchemeChanged(this, new ColorSchemeChangeEventArgs(m_eColorScheme));
			}
		}
	}

	[Description("The style of the captionbar.")]
	[Category("Appearance")]
	public CaptionStyle CaptionStyle
	{
		get
		{
			return m_captionStyle;
		}
		set
		{
			m_captionStyle = value;
			OnCaptionStyleChanged(this, EventArgs.Empty);
		}
	}

	[Description("LinearGradientMode of the background in the xpanderpanellist")]
	[DefaultValue(LinearGradientMode.Vertical)]
	[Category("Appearance")]
	public LinearGradientMode LinearGradientMode
	{
		get
		{
			return m_linearGradientMode;
		}
		set
		{
			if (value != m_linearGradientMode)
			{
				m_linearGradientMode = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Description("Gets or sets a value indicating whether a xpanderpanellist's gradient background is shown.")]
	[DefaultValue(false)]
	[Category("Appearance")]
	public bool ShowGradientBackground
	{
		get
		{
			return m_bShowGradientBackground;
		}
		set
		{
			if (value != m_bShowGradientBackground)
			{
				m_bShowGradientBackground = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Description("Gets or sets a value indicating whether a xpanderpanellist's border is shown")]
	[DefaultValue(true)]
	[Category("Appearance")]
	public bool ShowBorder
	{
		get
		{
			return m_bShowBorder;
		}
		set
		{
			if (value == m_bShowBorder)
			{
				return;
			}
			m_bShowBorder = value;
			foreach (XPanderPanel xPanderPanel in XPanderPanels)
			{
				xPanderPanel.ShowBorder = m_bShowBorder;
			}
			Invalidate(invalidateChildren: false);
		}
	}

	[Description("Gets or sets a value indicating whether the expand icon of the xpanderpanels in this xpanderpanellist are visible.")]
	[DefaultValue(false)]
	[Category("Appearance")]
	public bool ShowExpandIcon
	{
		get
		{
			return m_bShowExpandIcon;
		}
		set
		{
			if (value == m_bShowExpandIcon)
			{
				return;
			}
			m_bShowExpandIcon = value;
			foreach (XPanderPanel xPanderPanel in XPanderPanels)
			{
				xPanderPanel.ShowExpandIcon = m_bShowExpandIcon;
			}
		}
	}

	[Description("Gets or sets a value indicating whether the close icon of the xpanderpanels in this xpanderpanellist are visible.")]
	[DefaultValue(false)]
	[Category("Appearance")]
	public bool ShowCloseIcon
	{
		get
		{
			return m_bShowCloseIcon;
		}
		set
		{
			if (value == m_bShowCloseIcon)
			{
				return;
			}
			m_bShowCloseIcon = value;
			foreach (XPanderPanel xPanderPanel in XPanderPanels)
			{
				xPanderPanel.ShowCloseIcon = m_bShowCloseIcon;
			}
		}
	}

	[Description("Gradientcolor background in this xpanderpanellist")]
	[DefaultValue(false)]
	[Category("Appearance")]
	public Color GradientBackground
	{
		get
		{
			return m_colorGradientBackground;
		}
		set
		{
			if (value != m_colorGradientBackground)
			{
				m_colorGradientBackground = value;
				Invalidate(invalidateChildren: false);
			}
		}
	}

	[Description("Gets or sets the height of the XpanderPanels in this XPanderPanelList. ")]
	[DefaultValue(25)]
	[Category("Appearance")]
	public int CaptionHeight
	{
		get
		{
			return m_iCaptionHeight;
		}
		set
		{
			if (value < 18)
			{
				throw new InvalidOperationException(string.Format(CultureInfo.CurrentUICulture, Resources.IDS_InvalidOperationExceptionInteger, new object[3] { value, "CaptionHeight", 18 }));
			}
			m_iCaptionHeight = value;
			OnCaptionHeightChanged(this, EventArgs.Empty);
		}
	}

	[Description("The PanelStyleChanged event occurs when PanelStyle flags have been changed.")]
	public event EventHandler<PanelStyleChangeEventArgs> PanelStyleChanged;

	[Description("The CaptionStyleChanged event occurs when CaptionStyle flags have been changed.")]
	public event EventHandler<EventArgs> CaptionStyleChanged;

	[Description("The ColorSchemeChanged event occurs when ColorScheme flags have been changed.")]
	public event EventHandler<ColorSchemeChangeEventArgs> ColorSchemeChanged;

	[Description("Occurs when the value of the CaptionHeight property changes.")]
	public event EventHandler<EventArgs> CaptionHeightChanged;

	public XPanderPanelList()
	{
		SetStyle(ControlStyles.DoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: false);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		InitializeComponent();
		m_xpanderPanels = new XPanderPanelCollection(this);
		ShowBorder = true;
		PanelStyle = PanelStyle.Default;
		LinearGradientMode = LinearGradientMode.Vertical;
		CaptionHeight = 25;
	}

	public void Expand(BasePanel panel)
	{
		if (panel == null)
		{
			throw new ArgumentNullException("panel", string.Format(CultureInfo.InvariantCulture, Resources.IDS_ArgumentException, new object[1] { "panel" }));
		}
		if (!(panel is XPanderPanel xPanderPanel))
		{
			return;
		}
		foreach (XPanderPanel xpanderPanel in m_xpanderPanels)
		{
			if (!xpanderPanel.Equals(xPanderPanel))
			{
				xpanderPanel.Expand = false;
			}
		}
		TypeDescriptor.GetProperties(xPanderPanel)["Expand"]?.SetValue(xPanderPanel, true);
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
		base.OnPaintBackground(pevent);
		if (m_bShowGradientBackground)
		{
			Rectangle rect = new Rectangle(0, 0, ClientRectangle.Width, ClientRectangle.Height);
			using LinearGradientBrush brush = new LinearGradientBrush(rect, BackColor, GradientBackground, LinearGradientMode);
			pevent.Graphics.FillRectangle(brush, rect);
		}
	}

	protected override void OnControlAdded(ControlEventArgs e)
	{
		base.OnControlAdded(e);
		if (e.Control is XPanderPanel xPanderPanel)
		{
			if (xPanderPanel.Expand)
			{
				foreach (XPanderPanel xPanderPanel2 in XPanderPanels)
				{
					if (xPanderPanel2 != xPanderPanel)
					{
						xPanderPanel2.Expand = false;
						xPanderPanel2.Height = xPanderPanel.CaptionHeight;
					}
				}
			}
			xPanderPanel.Parent = this;
			xPanderPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
			xPanderPanel.Left = Padding.Left;
			xPanderPanel.Width = checked(ClientRectangle.Width - Padding.Left - Padding.Right);
			xPanderPanel.PanelStyle = PanelStyle;
			xPanderPanel.ColorScheme = ColorScheme;
			if (PanelColors != null)
			{
				xPanderPanel.SetPanelProperties(PanelColors);
			}
			xPanderPanel.ShowBorder = ShowBorder;
			xPanderPanel.ShowCloseIcon = m_bShowCloseIcon;
			xPanderPanel.ShowExpandIcon = m_bShowExpandIcon;
			xPanderPanel.CaptionStyle = m_captionStyle;
			xPanderPanel.Top = GetTopPosition();
			xPanderPanel.PanelStyleChanged += XpanderPanelPanelStyleChanged;
			xPanderPanel.ExpandClick += XPanderPanelExpandClick;
			xPanderPanel.CloseClick += XPanderPanelCloseClick;
			return;
		}
		throw new InvalidOperationException("Can only add BSE.Windows.Forms.XPanderPanel");
	}

	protected override void OnControlRemoved(ControlEventArgs e)
	{
		base.OnControlRemoved(e);
		if (e.Control is XPanderPanel xPanderPanel)
		{
			xPanderPanel.PanelStyleChanged -= XpanderPanelPanelStyleChanged;
			xPanderPanel.ExpandClick -= XPanderPanelExpandClick;
			xPanderPanel.CloseClick -= XPanderPanelCloseClick;
		}
	}

	protected override void OnResize(EventArgs e)
	{
		base.OnResize(e);
		int num = 0;
		if (m_xpanderPanels == null)
		{
			return;
		}
		checked
		{
			foreach (XPanderPanel xpanderPanel in m_xpanderPanels)
			{
				xpanderPanel.Width = ClientRectangle.Width - Padding.Left - Padding.Right;
				if (!xpanderPanel.Visible)
				{
					num -= xpanderPanel.CaptionHeight;
				}
				num += xpanderPanel.CaptionHeight;
			}
			foreach (XPanderPanel xpanderPanel2 in m_xpanderPanels)
			{
				if (xpanderPanel2.Expand)
				{
					xpanderPanel2.Height = Height - num - Padding.Top - Padding.Bottom + xpanderPanel2.CaptionHeight;
					break;
				}
			}
		}
	}

	protected virtual void OnPanelStyleChanged(object sender, PanelStyleChangeEventArgs e)
	{
		PanelStyle panelStyle = e.PanelStyle;
		Padding = new Padding(0);
		foreach (XPanderPanel xPanderPanel in XPanderPanels)
		{
			PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(xPanderPanel);
			if (properties.Count > 0)
			{
				properties["PanelStyle"]?.SetValue(xPanderPanel, panelStyle);
				properties["Left"]?.SetValue(xPanderPanel, Padding.Left);
				properties["Width"]?.SetValue(xPanderPanel, checked(ClientRectangle.Width - Padding.Left - Padding.Right));
			}
		}
		if (PanelStyleChanged != null)
		{
			PanelStyleChanged(sender, e);
		}
	}

	protected virtual void OnColorSchemeChanged(object sender, ColorSchemeChangeEventArgs e)
	{
		ColorScheme colorSchema = e.ColorSchema;
		foreach (XPanderPanel xPanderPanel in XPanderPanels)
		{
			TypeDescriptor.GetProperties(xPanderPanel)["ColorScheme"]?.SetValue(xPanderPanel, colorSchema);
		}
		if (ColorSchemeChanged != null)
		{
			ColorSchemeChanged(sender, e);
		}
	}

	protected virtual void OnCaptionHeightChanged(object sender, EventArgs e)
	{
		foreach (XPanderPanel xPanderPanel in XPanderPanels)
		{
			TypeDescriptor.GetProperties(xPanderPanel)["CaptionHeight"]?.SetValue(xPanderPanel, m_iCaptionHeight);
		}
		if (CaptionHeightChanged != null)
		{
			CaptionHeightChanged(sender, e);
		}
	}

	protected virtual void OnCaptionStyleChanged(object sender, EventArgs e)
	{
		foreach (XPanderPanel xPanderPanel in XPanderPanels)
		{
			TypeDescriptor.GetProperties(xPanderPanel)["CaptionStyle"]?.SetValue(xPanderPanel, m_captionStyle);
		}
		if (CaptionStyleChanged != null)
		{
			CaptionStyleChanged(sender, e);
		}
	}

	private void XPanderPanelExpandClick(object sender, EventArgs e)
	{
		if (sender is XPanderPanel panel)
		{
			Expand(panel);
		}
	}

	private void XPanderPanelCloseClick(object sender, EventArgs e)
	{
		if (sender is XPanderPanel value)
		{
			Controls.Remove(value);
		}
	}

	private void XpanderPanelPanelStyleChanged(object sender, PanelStyleChangeEventArgs e)
	{
		PanelStyle panelStyle = e.PanelStyle;
		if (panelStyle != m_ePanelStyle)
		{
			PanelStyle = panelStyle;
		}
	}

	private int GetTopPosition()
	{
		int num = Padding.Top;
		int num2 = 0;
		IEnumerator enumerator = XPanderPanels.GetEnumerator();
		while (enumerator.MoveNext())
		{
			XPanderPanel xPanderPanel = (XPanderPanel)enumerator.Current;
			if (xPanderPanel.Visible)
			{
				num = ((num2 != Padding.Top) ? num2 : Padding.Top);
				num2 = checked(num + xPanderPanel.Height);
			}
		}
		return num;
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
		base.Name = "xPanderPanelList";
	}
}
