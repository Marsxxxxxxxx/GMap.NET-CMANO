namespace Demo.WindowsForms;

internal struct IpStatus
{
	private string countryName;

	private int connectionsCount;

	public string CountryName
	{
		get
		{
			return countryName;
		}
		set
		{
			countryName = value;
		}
	}

	public int ConnectionsCount
	{
		get
		{
			return connectionsCount;
		}
		set
		{
			connectionsCount = value;
		}
	}
}
