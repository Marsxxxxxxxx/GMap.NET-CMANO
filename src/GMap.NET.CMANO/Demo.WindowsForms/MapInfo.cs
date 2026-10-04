using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

public struct MapInfo(RectLatLng Area, int Zoom, GMapProvider Type, bool makeWorldFile, bool MakeKmz, string outputFileName = null, long? sizeLimitMegabytes = null)
{
	public RectLatLng Area = Area;

	public int Zoom = Zoom;

	public GMapProvider Type = Type;

	public bool MakeWorldFile = makeWorldFile;

	public bool MakeKmz = MakeKmz;

	public string OutputFileName = outputFileName;

	public long? SizeLimitMegabytes = sizeLimitMegabytes;
}
