using System.Collections.Generic;

namespace Demo.WindowsForms;

internal class DescendingComparer : IComparer<IpStatus>
{
	public bool SortOnlyCountryName;

	public int Compare(IpStatus x, IpStatus y)
	{
		int num = 0;
		if (!SortOnlyCountryName)
		{
			num = y.ConnectionsCount.CompareTo(x.ConnectionsCount);
		}
		if (num == 0)
		{
			return x.CountryName.CompareTo(y.CountryName);
		}
		return num;
	}
}
