using System;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using Demo.WindowsForms.Properties;

namespace Demo.WindowsForms;

internal class SQLiteIpCache
{
	private string cache;

	private string ipCache;

	private string db;

	public string IpCache => ipCache;

	public string CacheLocation
	{
		get
		{
			return cache;
		}
		set
		{
			cache = value;
			ipCache = Path.Combine(cache, "IpGeoCacheDB") + Path.DirectorySeparatorChar;
			db = ipCache + "Data.ipdb";
			if (!File.Exists(db))
			{
				CreateEmptyDB(db);
			}
		}
	}

	public static bool CreateEmptyDB(string file)
	{
		//IL_0018: Unknown result type (might be due to invalid IL or missing references)
		//IL_001e: Expected Obj, but got Unknown
		bool result = true;
		try
		{
			string directoryName = Path.GetDirectoryName(file);
			if (!Directory.Exists(directoryName))
			{
				Directory.CreateDirectory(directoryName);
			}
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{file}\";FailIfMissing=False;";
				((DbConnection)(object)val).Open();
				using (DbTransaction dbTransaction = (DbTransaction)(object)val.BeginTransaction())
				{
					try
					{
						using (DbCommand dbCommand = (DbCommand)(object)val.CreateCommand())
						{
							dbCommand.Transaction = dbTransaction;
							dbCommand.CommandText = Resources.IpCacheCreateDb;
							dbCommand.ExecuteNonQuery();
						}
						dbTransaction.Commit();
					}
					catch (Exception ex)
					{
						Console.WriteLine("CreateEmptyDB: " + ex.ToString());
						dbTransaction.Rollback();
						result = false;
					}
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public bool PutDataToCache(string ip, IpInfo data)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected Obj, but got Unknown
		//IL_0050: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Expected Obj, but got Unknown
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0076: Expected Obj, but got Unknown
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_0092: Expected Obj, but got Unknown
		//IL_00a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ae: Expected Obj, but got Unknown
		//IL_00c5: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cf: Expected Obj, but got Unknown
		//IL_00e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f0: Expected Obj, but got Unknown
		//IL_0107: Unknown result type (might be due to invalid IL or missing references)
		//IL_0111: Expected Obj, but got Unknown
		bool result = true;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{db}\";";
				((DbConnection)(object)val).Open();
				using (DbTransaction dbTransaction = (DbTransaction)(object)val.BeginTransaction())
				{
					try
					{
						using (DbCommand dbCommand = (DbCommand)(object)val.CreateCommand())
						{
							dbCommand.Transaction = dbTransaction;
							dbCommand.CommandText = "INSERT INTO Cache(Ip, CountryName, RegionName, City, Latitude, Longitude, Time) VALUES(@p1, @p2, @p3, @p4, @p5, @p6, @p7)";
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p1", (object)ip));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p2", (object)data.CountryName));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p3", (object)data.RegionName));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p4", (object)data.City));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p5", (object)data.Latitude));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p6", (object)data.Longitude));
							dbCommand.Parameters.Add((object)new SQLiteParameter("@p7", (object)data.CacheTime));
							dbCommand.ExecuteNonQuery();
						}
						dbTransaction.Commit();
					}
					catch (Exception ex)
					{
						Console.WriteLine("PutDataToCache: " + ex.ToString());
						dbTransaction.Rollback();
						result = false;
					}
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = false;
		}
		return result;
	}

	public IpInfo GetDataFromCache(string ip)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		//IL_0008: Expected Obj, but got Unknown
		IpInfo result = null;
		try
		{
			SQLiteConnection val = new SQLiteConnection();
			try
			{
				((DbConnection)(object)val).ConnectionString = $"Data Source=\"{db}\";";
				((DbConnection)(object)val).Open();
				using (DbCommand dbCommand = (DbCommand)(object)val.CreateCommand())
				{
					dbCommand.CommandText = "SELECT * FROM Cache WHERE Ip = '" + ip + "'";
					using DbDataReader dbDataReader = dbCommand.ExecuteReader();
					if (dbDataReader.Read())
					{
						result = new IpInfo
						{
							Ip = ip,
							CountryName = (dbDataReader["CountryName"] as string),
							RegionName = (dbDataReader["RegionName"] as string),
							City = (dbDataReader["City"] as string),
							Latitude = (double)dbDataReader["Latitude"],
							Longitude = (double)dbDataReader["Longitude"],
							CacheTime = (DateTime)dbDataReader["Time"]
						};
					}
					dbDataReader.Close();
				}
				((DbConnection)(object)val).Close();
			}
			finally
			{
				((IDisposable)val)?.Dispose();
			}
		}
		catch (Exception)
		{
			result = null;
		}
		return result;
	}
}
