using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

[ToolboxBitmap(typeof(System.Windows.Forms.ProgressBar))]
public class ProgressBar : Control
{
	private Color m_backgroundColor;

	private Color m_valueColor;

	private Color m_borderColor;

	private int m_iMinimum;

	private int m_iMaximum;

	private int m_iValue;

	private IContainer components;

	[Browsable(true)]
	[Description("The color used for the background rectangle of this control.")]
	public Color BackgroundColor
	{
		get
		{
			return m_backgroundColor;
		}
		set
		{
			if (m_backgroundColor != value)
			{
				m_backgroundColor = value;
				OnBackgroundColorChanged(this, EventArgs.Empty);
			}
		}
	}

	[Browsable(true)]
	[Description("The color used for the value rectangle of this control.")]
	public Color ValueColor
	{
		get
		{
			return m_valueColor;
		}
		set
		{
			if (m_valueColor != value)
			{
				m_valueColor = value;
				OnValueColorChanged(this, EventArgs.Empty);
			}
		}
	}

	public Color BorderColor
	{
		get
		{
			return m_borderColor;
		}
		set
		{
			if (m_borderColor != value)
			{
				m_borderColor = value;
				OnBorderColorChanged(this, EventArgs.Empty);
			}
		}
	}

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public new Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
		}
	}

	[Browsable(true)]
	[Description("The upper bound of range this ProgressBar is working with.")]
	public int Maximum
	{
		get
		{
			return m_iMaximum;
		}
		set
		{
			if (m_iMaximum != value)
			{
				if (value < 0)
				{
					object[] args = new object[3]
					{
						"Maximum",
						value.ToString(CultureInfo.CurrentCulture),
						"Maximum"
					};
					throw new ArgumentOutOfRangeException("Maximum", string.Format(CultureInfo.InvariantCulture, Resources.IDS_InvalidLowBoundArgument, args));
				}
				if (m_iMinimum > value)
				{
					m_iMinimum = value;
				}
				m_iMaximum = value;
				if (m_iValue > m_iMaximum)
				{
					m_iValue = m_iMaximum;
				}
				UpdatePos();
			}
		}
	}

	[Browsable(true)]
	[Description("The lower bound of range this ProgressBar is working with.")]
	public int Minimum
	{
		get
		{
			return m_iMinimum;
		}
		set
		{
			if (m_iMinimum != value)
			{
				if (value < 0)
				{
					object[] args = new object[3]
					{
						"Minimum",
						value.ToString(CultureInfo.CurrentCulture),
						"Minimum"
					};
					throw new ArgumentOutOfRangeException("Minimum", string.Format(CultureInfo.InvariantCulture, Resources.IDS_InvalidLowBoundArgument, args));
				}
				if (m_iMaximum < value)
				{
					m_iMaximum = value;
				}
				m_iMinimum = value;
				if (m_iValue < m_iMinimum)
				{
					m_iValue = m_iMinimum;
				}
				UpdatePos();
			}
		}
	}

	[Browsable(true)]
	[Description("The current value for the ProgressBar, in the range specified by the minimum and maximum properties.")]
	public int Value
	{
		get
		{
			return m_iValue;
		}
		set
		{
			if (m_iValue != value)
			{
				if (value < m_iMinimum || value > m_iMaximum)
				{
					throw new ArgumentOutOfRangeException("Value", string.Format(CultureInfo.InvariantCulture, Resources.IDS_InvalidBoundArgument, "Value", value.ToString(CultureInfo.CurrentCulture), "'minimum'", "'maximum'"));
				}
				m_iValue = value;
				UpdatePos();
			}
		}
	}

	[Description("Occurs when the value of the BorderColor property is changed on the control.")]
	public event EventHandler<EventArgs> BorderColorChanged;

	[Description("Occurs when the value of the BackgroundColor property is changed on the control.")]
	public event EventHandler<EventArgs> BackgroundColorChanged;

	[Description("Occurs when the value of the ValueColor property is changed on the control.")]
	public event EventHandler<EventArgs> ValueColorChanged;

	public ProgressBar()
	{
		SetStyle(ControlStyles.AllPaintingInWmPaint, value: true);
		SetStyle(ControlStyles.UserPaint, value: true);
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: true);
		SetStyle(ControlStyles.SupportsTransparentBackColor, value: true);
		InitializeComponent();
		m_iMaximum = 100;
		m_backgroundColor = Color.FromArgb(20, 20, 255);
		m_valueColor = Color.FromArgb(255, 0, 255);
		m_borderColor = SystemColors.ActiveBorder;
		BackColor = Color.Transparent;
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		using (new UseAntiAlias(e.Graphics))
		{
			Graphics graphics = e.Graphics;
			DrawProgressBar(graphics, ClientRectangle, m_backgroundColor, m_valueColor, m_borderColor, RightToLeft, Minimum, Maximum, Value);
			if (string.IsNullOrEmpty(Text))
			{
				return;
			}
			using (new UseClearTypeGridFit(graphics))
			{
				using SolidBrush brush = new SolidBrush(ForeColor);
				using StringFormat stringFormat = new StringFormat();
				stringFormat.FormatFlags = StringFormatFlags.NoWrap;
				if (RightToLeft == RightToLeft.Yes)
				{
					stringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
				}
				stringFormat.Trimming = StringTrimming.EllipsisCharacter;
				stringFormat.LineAlignment = StringAlignment.Center;
				stringFormat.Alignment = StringAlignment.Center;
				Rectangle clientRectangle = ClientRectangle;
				graphics.DrawString(Text, Font, brush, clientRectangle, stringFormat);
			}
		}
	}

	protected virtual void OnBorderColorChanged(object sender, EventArgs e)
	{
		Invalidate(invalidateChildren: true);
		if (BorderColorChanged != null)
		{
			BorderColorChanged(sender, e);
		}
	}

	protected virtual void OnBackgroundColorChanged(object sender, EventArgs e)
	{
		Invalidate();
		if (BackgroundColorChanged != null)
		{
			BackgroundColorChanged(sender, e);
		}
	}

	protected virtual void OnValueColorChanged(object sender, EventArgs e)
	{
		Invalidate(invalidateChildren: true);
		if (ValueColorChanged != null)
		{
			ValueColorChanged(sender, e);
		}
	}

	private void UpdatePos()
	{
		Invalidate(invalidateChildren: true);
	}

	private static void DrawProgressBar(Graphics graphics, Rectangle clientRectangle, Color colorBackgroundEnd, Color colorValueEnd, Color borderColor, RightToLeft rightToLeft, int iMinimum, int iMaximum, int iValue)
	{
		Rectangle rectangleBackground = GetRectangleBackground(clientRectangle);
		using GraphicsPath graphicsPath = GetBackgroundPath(rectangleBackground, 4);
		if (graphicsPath == null)
		{
			return;
		}
		using (LinearGradientBrush linearGradientBrush = GetGradientBackBrush(rectangleBackground, colorBackgroundEnd))
		{
			if (linearGradientBrush != null)
			{
				graphics.FillPath(linearGradientBrush, graphicsPath);
			}
		}
		if (iValue > 0)
		{
			Rectangle rectangleValue = GetRectangleValue(rectangleBackground, rightToLeft, iMinimum, iMaximum, iValue);
			using GraphicsPath path = GetValuePath(rectangleValue, rightToLeft, 5);
			using LinearGradientBrush linearGradientBrush2 = GetGradientBackBrush(rectangleValue, colorValueEnd);
			if (linearGradientBrush2 != null)
			{
				graphics.FillPath(linearGradientBrush2, path);
			}
		}
		using Pen pen = new Pen(borderColor);
		graphics.DrawPath(pen, graphicsPath);
	}

	private static Rectangle GetRectangleBackground(Rectangle clientRectangle)
	{
		Rectangle result = clientRectangle;
		result.Inflate(-1, -1);
		return result;
	}

	private static Rectangle GetRectangleValue(Rectangle backgroundRectangle, RightToLeft rightToLeft, int iMinimum, int iMaximum, int iValue)
	{
		Rectangle result = backgroundRectangle;
		checked
		{
			int num = iMaximum - iMinimum;
			int num2 = (int)((float)(iValue - iMinimum) / (float)num * (float)backgroundRectangle.Width);
			result.Width = num2;
			if (rightToLeft == RightToLeft.Yes)
			{
				result.X = backgroundRectangle.Width - result.Width;
			}
			return result;
		}
	}

	private static GraphicsPath GetBackgroundPath(Rectangle bounds, int radius)
	{
		int num = bounds.X;
		int num2 = bounds.Y;
		int num3 = bounds.Width;
		int num4 = bounds.Height;
		GraphicsPath graphicsPath = new GraphicsPath();
		graphicsPath.AddArc(num, num2, radius, radius, 180f, 90f);
		checked
		{
			graphicsPath.AddArc(num + num3 - radius, num2, radius, radius, 270f, 90f);
			graphicsPath.AddArc(num + num3 - radius, num2 + num4 - radius, radius, radius, 0f, 90f);
			graphicsPath.AddArc(num, num2 + num4 - radius, radius, radius, 90f, 90f);
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
	}

	private static GraphicsPath GetValuePath(Rectangle bounds, RightToLeft rightToLeft, int radius)
	{
		int num = bounds.X;
		int num2 = bounds.Y;
		int num3 = bounds.Width;
		int num4 = bounds.Height;
		GraphicsPath graphicsPath = new GraphicsPath();
		checked
		{
			if (rightToLeft == RightToLeft.No)
			{
				graphicsPath.AddArc(num, num2, radius, radius, 180f, 90f);
				graphicsPath.AddLine(num + radius, num2, num + num3, num2);
				graphicsPath.AddLine(num + num3, num2, num + num3, num2 + num4);
				graphicsPath.AddArc(num, num2 + num4 - radius, radius, radius, 90f, 90f);
			}
			else
			{
				graphicsPath.AddLine(num, num2, num3 - radius, num2);
				graphicsPath.AddArc(num + num3 - radius, num2, radius, radius, 270f, 90f);
				graphicsPath.AddLine(num + num3, num2 + radius, num + num3, num2 + radius + num4 - 2 * radius);
				graphicsPath.AddArc(num + num3 - radius, num2 + radius + num4 - 2 * radius, radius, radius, 360f, 90f);
				graphicsPath.AddLine(num + num3 - radius, num2 + num4, num, num2 + num4);
			}
			graphicsPath.CloseFigure();
			return graphicsPath;
		}
	}

	private static LinearGradientBrush GetGradientBackBrush(Rectangle bounds, Color backColor)
	{
		if (IsZeroWidthOrHeight(bounds))
		{
			return null;
		}
		LinearGradientBrush linearGradientBrush = (linearGradientBrush = new LinearGradientBrush(bounds, Color.White, backColor, LinearGradientMode.Vertical));
		if (linearGradientBrush != null)
		{
			Blend blend = new Blend();
			blend.Positions = new float[7] { 0f, 0.2f, 0.3f, 0.5f, 0.6f, 0.8f, 1f };
			blend.Factors = new float[7] { 0.3f, 0.4f, 0.5f, 0.8f, 1f, 1f, 0.9f };
			linearGradientBrush.Blend = blend;
		}
		return linearGradientBrush;
	}

	private static bool IsZeroWidthOrHeight(Rectangle rectangle)
	{
		if (rectangle.Width != 0)
		{
			return rectangle.Height == 0;
		}
		return true;
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
		base.Size = new System.Drawing.Size(100, 23);
		base.Name = "progressBar";
	}
}
