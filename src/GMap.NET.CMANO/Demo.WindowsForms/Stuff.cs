using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SQLite;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Windows.Forms;
using GMap.NET;
using GMap.NET.MapProviders;

namespace Demo.WindowsForms;

public class Stuff
{
	private static readonly Random r = new Random();

	private static string sessionId = string.Empty;

	public static double ConvertDegreeAngleToDouble(TextBox degreesTxt, TextBox minutesTxt, TextBox secondsTxt, bool isLatitude)
	{
		if (degreesTxt.Text.Length < 2)
		{
			return double.NaN;
		}
		string text = degreesTxt.Text.Substring(0, 1);
		if ((isLatitude && text.ToUpperInvariant() != "N" && text.ToUpperInvariant() != "S") || (!isLatitude && text.ToUpperInvariant() != "W" && text.ToUpperInvariant() != "E"))
		{
			degreesTxt.BackColor = Color.Red;
			return double.NaN;
		}
		degreesTxt.BackColor = Color.White;
		if (!double.TryParse(degreesTxt.Text.Substring(1, checked(degreesTxt.Text.Length - 1)), out var result))
		{
			return double.NaN;
		}
		if ((isLatitude && text.ToUpperInvariant() == "S") || (!isLatitude && text.ToUpperInvariant() == "W"))
		{
			result *= -1.0;
		}
		if (!double.TryParse(minutesTxt.Text, out var result2))
		{
			return double.NaN;
		}
		if (!double.TryParse(secondsTxt.Text, out var result3))
		{
			return double.NaN;
		}
		int num = ((!(result < 0.0)) ? 1 : (-1));
		return (Math.Abs(result) + result2 / 60.0 + result3 / 3600.0) * (double)num;
	}

	public static Image FixedSize(Image imgPhoto, int Width, int Height)
	{
		return ImageUtilities.ResizeImage(imgPhoto, Width, Height);
	}

	public static bool PingNetwork(string hostNameOrAddress)
	{
		bool result = false;
		using (Ping ping = new Ping())
		{
			byte[] bytes = Encoding.ASCII.GetBytes("aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa");
			int timeout = 4444;
			try
			{
				result = ping.Send(hostNameOrAddress, timeout, bytes).Status == IPStatus.Success;
			}
			catch (Exception)
			{
				result = false;
			}
		}
		return result;
	}

	public static IEnumerable<List<GpsLog>> GetRoutesFromMobileLog(string gpsdLogFile, DateTime? start, DateTime? end, double? maxPositionDilutionOfPrecision)
	{
		SQLiteConnection cn = new SQLiteConnection();
		try
		{
			((DbConnection)(object)cn).ConnectionString = $"Data Source=\"{gpsdLogFile}\";FailIfMissing=True;";
			((DbConnection)(object)cn).Open();
			using (DbCommand cmd = (DbCommand)(object)cn.CreateCommand())
			{
				cmd.CommandText = "SELECT * FROM GPS ";
				if (start.HasValue)
				{
					cmd.CommandText += "WHERE TimeUTC >= @t1 ";
					SQLiteParameter value = new SQLiteParameter("@t1", (object)start);
					cmd.Parameters.Add(value);
				}
				if (end.HasValue)
				{
					if (!start.HasValue)
					{
						cmd.CommandText += "WHERE ";
					}
					else
					{
						cmd.CommandText += "AND ";
					}
					cmd.CommandText += "TimeUTC <= @t2 ";
					SQLiteParameter value2 = new SQLiteParameter("@t2", (object)end);
					cmd.Parameters.Add(value2);
				}
				if (maxPositionDilutionOfPrecision.HasValue)
				{
					if (!start.HasValue && !end.HasValue)
					{
						cmd.CommandText += "WHERE ";
					}
					else
					{
						cmd.CommandText += "AND ";
					}
					cmd.CommandText += "(PositionDilutionOfPrecision <= @p3)";
					SQLiteParameter value3 = new SQLiteParameter("@p3", (object)maxPositionDilutionOfPrecision);
					cmd.Parameters.Add(value3);
				}
				using DbDataReader rd = cmd.ExecuteReader();
				List<GpsLog> points = new List<GpsLog>();
				long num = -1L;
				while (rd.Read())
				{
					GpsLog log = new GpsLog
					{
						TimeUTC = (DateTime)rd["TimeUTC"],
						SessionCounter = (long)rd["SessionCounter"],
						Delta = (rd["Delta"] as double?),
						Speed = (rd["Speed"] as double?),
						SeaLevelAltitude = (rd["SeaLevelAltitude"] as double?),
						EllipsoidAltitude = (rd["EllipsoidAltitude"] as double?),
						SatellitesInView = (rd["SatellitesInView"] as byte?),
						SatelliteCount = (rd["SatelliteCount"] as byte?),
						Position = new PointLatLng((double)rd["Lat"], (double)rd["Lng"]),
						PositionDilutionOfPrecision = (rd["PositionDilutionOfPrecision"] as double?),
						HorizontalDilutionOfPrecision = (rd["HorizontalDilutionOfPrecision"] as double?),
						VerticalDilutionOfPrecision = (rd["VerticalDilutionOfPrecision"] as double?),
						FixQuality = (FixQuality)(byte)rd["FixQuality"],
						FixType = (FixType)(byte)rd["FixType"],
						FixSelection = (FixSelection)(byte)rd["FixSelection"]
					};
					if (checked(log.SessionCounter - num) != 1 && points.Count > 0)
					{
						List<GpsLog> list = new List<GpsLog>(points);
						points.Clear();
						yield return list;
					}
					points.Add(log);
					num = log.SessionCounter;
				}
				if (points.Count > 0)
				{
					List<GpsLog> list2 = new List<GpsLog>(points);
					points.Clear();
					yield return list2;
				}
				points.Clear();
				rd.Close();
			}
			((DbConnection)(object)cn).Close();
		}
		finally
		{
			((IDisposable)cn)?.Dispose();
		}
	}

	public static void GetVilniusTransportData(TransportType type, string line, List<VehicleData> ret)
	{
		ret.Clear();
		string text = string.Format(CultureInfo.InvariantCulture, "http://www.troleibusai.lt/eismas/get_gps.php?allowed=true&more=1&bus={0}&rand={1}", new object[2]
		{
			(type != TransportType.Bus) ? 1 : 2,
			r.NextDouble()
		});
		if (!string.IsNullOrEmpty(line))
		{
			text = text + "&nr=" + line;
		}
		text += "&app=GMap.NET.Desktop";
		string text2 = string.Empty;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(text);
		httpWebRequest.UserAgent = GMapProvider.UserAgent;
		httpWebRequest.Timeout = GMapProvider.TimeoutMs;
		httpWebRequest.ReadWriteTimeout = checked(GMapProvider.TimeoutMs * 6);
		httpWebRequest.Accept = "*/*";
		httpWebRequest.KeepAlive = true;
		using (HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse)
		{
			using (Stream stream = httpWebResponse.GetResponseStream())
			{
				using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
				text2 = streamReader.ReadToEnd();
			}
			httpWebResponse.Close();
		}
		if (string.IsNullOrEmpty(text2))
		{
			return;
		}
		string[] array = text2.Split('&');
		for (int i = 0; i < array.Length; i++)
		{
			string[] array2 = array[i].Split(';');
			if (array2.Length != 8)
			{
				continue;
			}
			VehicleData item = new VehicleData
			{
				Id = int.Parse(array2[2]),
				Lat = double.Parse(array2[0], CultureInfo.InvariantCulture),
				Lng = double.Parse(array2[1], CultureInfo.InvariantCulture),
				Line = array2[3]
			};
			if (!string.IsNullOrEmpty(array2[4]))
			{
				item.Bearing = double.Parse(array2[4], CultureInfo.InvariantCulture);
			}
			if (!string.IsNullOrEmpty(array2[5]))
			{
				item.Time = array2[5];
				DateTime dateTime = DateTime.Parse(item.Time);
				if (DateTime.Now - dateTime > TimeSpan.FromMinutes(5.0))
				{
					continue;
				}
				item.Time = dateTime.ToLongTimeString();
			}
			item.TrackType = array2[6];
			ret.Add(item);
		}
	}

	public static void GetFlightRadarData(List<FlightRadarData> ret, RectLatLng bounds)
	{
		ret.Clear();
		string[] array = GetFlightRadarContentUsingHttp(string.Format(CultureInfo.InvariantCulture, "http://arn.data.fr24.com/zones/fcgi/feed.js?bounds={0},{1},{2},{3}&faa=1&mlat=1&flarm=1&adsb=1&gnd=1&air=1&vehicles=1&estimated=1&maxage=900&gliders=1&stats=1&", bounds.Top, bounds.Bottom, bounds.Left, bounds.Right)).Split(new string[1] { "\n," }, StringSplitOptions.RemoveEmptyEntries);
		checked
		{
			foreach (string text in array)
			{
				if (text.Length > 11 && !text.Contains("full_count") && !text.Contains("stats"))
				{
					string[] array2 = text.TrimEnd(']').Replace(":[", ",").Replace("\"", string.Empty)
						.Split(',');
					if (array2.Length >= 9)
					{
						int id = Convert.ToInt32(array2[0], 16);
						string name = array2[8] + "|" + array2[9] + "|" + array2[10];
						string s = array2[2];
						string s2 = array2[3];
						string s3 = array2[4];
						string altitude = (int)((double)int.Parse(array2[5]) * 0.3048) + "m";
						string speed = (int)((double)int.Parse(array2[6]) * 1.852) + "km/h";
						ret.Add(new FlightRadarData
						{
							name = name,
							bearing = int.Parse(s3),
							altitude = altitude,
							speed = speed,
							point = new PointLatLng(double.Parse(s, CultureInfo.InvariantCulture), double.Parse(s2, CultureInfo.InvariantCulture)),
							Id = id
						});
					}
				}
			}
		}
	}

	private static string GetFlightRadarContentUsingHttp(string url)
	{
		string result = string.Empty;
		HttpWebRequest httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
		httpWebRequest.UserAgent = GMapProvider.UserAgent;
		httpWebRequest.Timeout = GMapProvider.TimeoutMs;
		httpWebRequest.ReadWriteTimeout = checked(GMapProvider.TimeoutMs * 6);
		httpWebRequest.Accept = "*/*";
		httpWebRequest.Referer = "http://www.flightradar24.com/";
		httpWebRequest.KeepAlive = true;
		using HttpWebResponse httpWebResponse = httpWebRequest.GetResponse() as HttpWebResponse;
		using (Stream stream = httpWebResponse.GetResponseStream())
		{
			using StreamReader streamReader = new StreamReader(stream, Encoding.UTF8);
			result = streamReader.ReadToEnd();
		}
		httpWebResponse.Close();
		return result;
	}
}
