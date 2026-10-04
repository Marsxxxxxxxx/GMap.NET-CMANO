using GMap.NET;

namespace Demo.WindowsForms;

public struct FlightRadarData
{
	public string name;

	public PointLatLng point;

	public int bearing;

	public string altitude;

	public string speed;

	public int Id;
}
