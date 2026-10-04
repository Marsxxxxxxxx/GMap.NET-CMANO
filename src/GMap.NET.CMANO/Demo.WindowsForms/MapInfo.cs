using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

public struct MapInfo(RectLatLng Area, int Zoom, GMapProvider Type, bool makeWorldFile, bool MakeKmz)
{
	public RectLatLng Area = Area;

	public int Zoom = Zoom;

	public GMapProvider Type = Type;

	public bool MakeWorldFile = makeWorldFile;

	public bool MakeKmz = MakeKmz;
}
