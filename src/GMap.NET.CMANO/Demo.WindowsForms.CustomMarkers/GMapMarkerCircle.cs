using System;
using System.Drawing;
using System.Runtime.Serialization;
using GMap.NET;
using GMap.NET.WindowsForms;

namespace Demo.WindowsForms.CustomMarkers;

[Serializable]
public class GMapMarkerCircle : GMapMarker, ISerializable
{
	public int Radius;

	[NonSerialized]
	public Pen Stroke = new Pen(Color.FromArgb(155, Color.MidnightBlue));

	[NonSerialized]
	public Brush Fill = new SolidBrush(Color.FromArgb(155, Color.AliceBlue));

	public bool IsFilled = true;

	public GMapMarkerCircle(PointLatLng p)
		: base(p)
	{
		Radius = 888;
		IsHitTestVisible = false;
	}

	public override void OnRender(Graphics g)
	{
		checked
		{
			int num = (int)((double)Radius / Overlay.Control.MapProvider.Projection.GetGroundResolution((int)Overlay.Control.Zoom, Position.Lat)) * 2;
			if (IsFilled)
			{
				g.FillEllipse(Fill, new Rectangle(LocalPosition.X - unchecked(num / 2), LocalPosition.Y - unchecked(num / 2), num, num));
			}
			g.DrawEllipse(Stroke, new Rectangle(LocalPosition.X - unchecked(num / 2), LocalPosition.Y - unchecked(num / 2), num, num));
		}
	}

	public override void Dispose()
	{
		if (Stroke != null)
		{
			Stroke.Dispose();
			Stroke = null;
		}
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

	protected GMapMarkerCircle(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
