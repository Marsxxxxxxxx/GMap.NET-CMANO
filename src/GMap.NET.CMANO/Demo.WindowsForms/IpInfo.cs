using System;
using System.Net.NetworkInformation;

namespace Demo.WindowsForms;

internal class IpInfo
{
	public string Ip;

	public int Port;

	public TcpState State;

	public string ProcessName;

	public string CountryName;

	public string RegionName;

	public string City;

	public double Latitude;

	public double Longitude;

	public DateTime CacheTime;

	public DateTime StatusTime;

	public bool TracePoint;
}
