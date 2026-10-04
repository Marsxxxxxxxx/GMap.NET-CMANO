using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace Demo.WindowsForms.CustomMarkers;

[Serializable]
public class GMarkerArrow : GMapMarker, ISerializable
{
	[NonSerialized]
	public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.Blue));

	private static readonly Point[] Arrow = new Point[4]
	{
		new Point(-7, 7),
		new Point(0, -22),
		new Point(7, 7),
		new Point(0, 2)
	};

	public float Bearing;

	private float scale = 1f;

	public float Scale
	{
		get
		{
			return scale;
		}
		set
		{
			scale = value;
			Size = checked(new Size((int)(14f * scale), (int)(14f * scale)));
			Offset = new Point(checked(-Size.Width) / 2, checked((int)((double)(-Size.Height) / 1.4)));
		}
	}

	public GMarkerArrow(PointLatLng p)
		: base(p)
	{
		Scale = 1f;
	}

	public override void OnRender(Graphics g)
	{
		g.TranslateTransform(ToolTipPosition.X, ToolTipPosition.Y);
		GraphicsContainer container = g.BeginContainer();
		g.RotateTransform(Bearing - Overlay.Control.Bearing);
		g.ScaleTransform(Scale, Scale);
		g.FillPolygon(Fill, Arrow);
		g.EndContainer(container);
		checked
		{
			g.TranslateTransform(-ToolTipPosition.X, -ToolTipPosition.Y);
		}
	}

	public override void Dispose()
	{
		if (Fill != null)
		{
			Fill.Dispose();
			Fill = null;
		}
		base.Dispose();
	}

	void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context)
	{
		GetObjectData(info, context);
	}

	protected GMarkerArrow(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
