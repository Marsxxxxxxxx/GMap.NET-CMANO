using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.IO;
using System.Net;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.Xml;
using BSE.Windows.Forms;
using Demo.WindowsForms.CustomMarkers;
using Demo.WindowsForms.Forms;
using GMap.NET;
using GMap.NET.MapProviders;
using GMap.NET.ObjectModel;
using GMap.NET.WindowsForms;
using GMap.NET.WindowsForms.Markers;
using GMap.NET.WindowsForms.ToolTips;

namespace Demo.WindowsForms;

public partial class MainForm : Form
{
	private readonly GMapOverlay top = new GMapOverlay();

	internal readonly GMapOverlay objects = new GMapOverlay("objects");

	internal readonly GMapOverlay routes = new GMapOverlay("routes");

	internal readonly GMapOverlay polygons = new GMapOverlay("polygons");

	private GMapMarker currentMarker;

	private GMapPolygon polygon;

	private readonly Random rnd = new Random();

	private readonly DescendingComparer ComparerIpStatus = new DescendingComparer();

	private GMapMarkerRect CurentRectMarker;

	private string mobileGpsLog = string.Empty;

	private bool isMouseDown;

	private PointLatLng start;

	private PointLatLng end;

	private int tt;

	private System.Windows.Forms.Timer timerPerf = new System.Windows.Forms.Timer();

	private BackgroundWorker flightWorker = new BackgroundWorker();

	private readonly List<FlightRadarData> flights = new List<FlightRadarData>();

	private readonly Dictionary<int, GMapMarker> flightMarkers = new Dictionary<int, GMapMarker>();

	private bool firstLoadFlight = true;

	private GMapMarker currentFlight;

	private RectLatLng flightBounds = new RectLatLng(54.4955675218741, -495.0 / 512.0, 28.916015625, 13.3830987326932);

	private BackgroundWorker transportWorker = new BackgroundWorker();

	private readonly List<VehicleData> trolleybus = new List<VehicleData>();

	private readonly Dictionary<int, GMapMarker> trolleybusMarkers = new Dictionary<int, GMapMarker>();

	private readonly List<VehicleData> bus = new List<VehicleData>();

	private readonly Dictionary<int, GMapMarker> busMarkers = new Dictionary<int, GMapMarker>();

	private bool firstLoadTrasport = true;

	private GMapMarker currentTransport;

	private BackgroundWorker connectionsWorker = new BackgroundWorker();

	private BackgroundWorker ipInfoSearchWorker = new BackgroundWorker();

	private BackgroundWorker iptracerWorker = new BackgroundWorker();

	private readonly Dictionary<string, IpInfo> TcpState = new Dictionary<string, IpInfo>();

	private readonly Dictionary<string, IpInfo> TcpTracePoints = new Dictionary<string, IpInfo>();

	private readonly Dictionary<string, List<PingReply>> TraceRoutes = new Dictionary<string, List<PingReply>>();

	private readonly List<string> TcpStateNeedLocationInfo = new List<string>();

	private readonly Queue<string> TcpStateNeedtraceInfo = new Queue<string>();

	private volatile bool TryTraceConnection;

	private GMapMarker lastTcpmarker;

	private readonly SQLiteIpCache IpCache = new SQLiteIpCache();

	private readonly Dictionary<string, GMapMarker> tcpConnections = new Dictionary<string, GMapMarker>();

	private readonly Dictionary<string, GMapRoute> tcpRoutes = new Dictionary<string, GMapRoute>();

	private readonly List<IpStatus> CountryStatusView = new List<IpStatus>();

	private readonly SortedDictionary<string, int> CountryStatus = new SortedDictionary<string, int>();

	private readonly List<string> SelectedCountries = new List<string>();

	private readonly Dictionary<int, Process> ProcessList = new Dictionary<int, Process>();

	private GMapPolygon currentPolygon;

	private GMapRoute currentRoute;

	private PointLatLng lastPosition;

	private int lastZoom;

	private bool UserAcceptedLicenseOnce;

	private IContainer components;

	private TrackBar trackBar1;

	private CheckBox checkBoxUseRouteCache;

	private Button button9;

	private Button button10;

	private Button button11;

	internal Map MainMap;

	private BSE.Windows.Forms.Panel panelMenu;

	private XPanderPanelList xPanderPanelList1;

	private XPanderPanel xPanderPanelMain;

	private XPanderPanel xPanderPanelCache;

	private XPanderPanel xPanderPanelLive;

	private System.Windows.Forms.Panel panel2;

	private System.Windows.Forms.Panel panel4;

	private BSE.Windows.Forms.Splitter splitter1;

	private XPanderPanel xPanderPanelInfo;

	private Label label4;

	private Label label5;

	private TextBox textBoxLngCurrent;

	private TextBox textBoxLatCurrent;

	private Label label9;

	private TextBox textBoxZoomCurrent;

	private Label label10;

	private TextBox textBoxMemory;

	private Label label12;

	private TextBox textBoxrouteCount;

	private Label label11;

	private TextBox textBoxMarkerCount;

	private TableLayoutPanel tableLayoutPanel1;

	private TableLayoutPanel tableLayoutPanel2;

	private TableLayoutPanel tableLayoutPanel3;

	private RadioButton radioButtonNone;

	private CheckBox checkBoxTcpIpSnap;

	private DataGridView GridConnections;

	private DataGridViewTextBoxColumn CountryName;

	private DataGridViewTextBoxColumn ConnectionsCount;

	private RadioButton radioButtonFlight;

	private RadioButton radioButtonPerf;

	private RadioButton radioButtonTcpIp;

	private TableLayoutPanel tableLayoutPanel4;

	private CheckBox checkBoxTraceRoute;

	private Button button2;

	private RadioButton radioButtonVehicle;

	private TableLayoutPanel tableLayoutPanel5;

	private GroupBox groupBox8;

	private Button button7;

	private CheckBox checkBoxPlacemarkInfo;

	private Button button5;

	private Button button4;

	private GroupBox groupBox7;

	private Button buttonExportToGpx;

	private DateTimePicker MobileLogTo;

	private Label label3;

	private Button button15;

	private DateTimePicker MobileLogFrom;

	private Button button14;

	private Button buttonSetEnd;

	private Button buttonSetStart;

	private Button button6;

	private Button button3;

	private GroupBox groupBox5;

	private Button button16;

	private Button button13;

	private CheckBox checkBoxDebug;

	private Button button12;

	private Label label8;

	private ComboBox comboBoxMode;

	private CheckBox checkBoxCanDrag;

	private CheckBox checkBoxCurrentMarker;

	private Label label7;

	private ComboBox comboBoxMapType;

	private GroupBox groupBox3;

	private Label label6;

	private TextBox textBoxGeo;

	private Button button1;

	private Button button8;

	private Label label2;

	private Label label1;

	private TextBox textBoxLng;

	private TextBox textBoxLat;

	private TableLayoutPanel tableLayoutPanel6;

	private Button buttonZoomUp;

	private Button buttonZoomDown;

	private TextBox textBoxCacheStatus;

	private Label label13;

	private Label label14;

	private TextBox textBoxCacheSize;

	private Button button17;

	private CheckBox checkBoxTileHost;

	private TextBox textBoxLngSecond;

	private TextBox textBoxLngMinute;

	private TextBox textBoxLngDegree;

	private TextBox textBoxLatSecond;

	private TextBox textBoxLatMinute;

	private TextBox textBoxLatDegree;

	private Label label16;

	private Label label15;

	private Button buttonDraw;

	private Label label20;

	private Label label21;

	private TextBox textBoxBRLngSecond;

	private TextBox textBoxBRLngMinute;

	private TextBox textBoxBRLngDegree;

	private TextBox textBoxBRLatSecond;

	private TextBox textBoxBRLatMinute;

	private TextBox textBoxBRLatDegree;

	private Label label22;

	private Label label18;

	private Label label19;

	private TextBox textBoxTLLngSecond;

	private TextBox textBoxTLLngMinute;

	private TextBox textBoxTLLngDegree;

	private TextBox textBoxTLLatSecond;

	private TextBox textBoxTLLatMinute;

	private TextBox textBoxTLLatDegree;

	private Label label17;

	public MainForm() : this(GMapProviders.BingSatelliteMap)
	{
	}

	internal MainForm(GMapProvider initialProvider, string cacheLocation = null)
	{
		InitializeComponent();
		checked
		{
			if (!GMapControl.IsDesignerHosted)
			{
				// Resolve from the executable, since shortcuts can use a different working directory.
				string cachePath = Path.GetFullPath(cacheLocation ?? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "cache"));
				Directory.CreateDirectory(cachePath);
				MainMap.CacheLocation = cachePath.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar) + Path.DirectorySeparatorChar;
				// Tile requests determine connectivity. An unrelated ICMP probe can fail
				// even when HTTPS map services work, especially behind a proxy.
				MainMap.MapProvider = initialProvider;
				MainMap.Position = new PointLatLng(54.6961334816182, 25.2985095977783);
				MainMap.MinZoom = 0;
				MainMap.MaxZoom = 24;
				MainMap.Zoom = 9.0;
				MainMap.OnPositionChanged += MainMap_OnPositionChanged;
				MainMap.OnTileLoadStart += MainMap_OnTileLoadStart;
				MainMap.OnTileLoadComplete += MainMap_OnTileLoadComplete;
				MainMap.OnMapZoomChanged += MainMap_OnMapZoomChanged;
				MainMap.OnMapTypeChanged += MainMap_OnMapTypeChanged;
				MainMap.OnMarkerClick += MainMap_OnMarkerClick;
				MainMap.OnMarkerEnter += MainMap_OnMarkerEnter;
				MainMap.OnMarkerLeave += MainMap_OnMarkerLeave;
				MainMap.OnPolygonEnter += MainMap_OnPolygonEnter;
				MainMap.OnPolygonLeave += MainMap_OnPolygonLeave;
				MainMap.OnRouteEnter += MainMap_OnRouteEnter;
				MainMap.OnRouteLeave += MainMap_OnRouteLeave;
				GMaps manager = MainMap.Manager;
				manager.OnTileCacheComplete = (TileCacheComplete)Delegate.Combine(manager.OnTileCacheComplete, new TileCacheComplete(OnTileCacheComplete));
				GMaps manager2 = MainMap.Manager;
				manager2.OnTileCacheStart = (TileCacheStart)Delegate.Combine(manager2.OnTileCacheStart, new TileCacheStart(OnTileCacheStart));
				GMaps manager3 = MainMap.Manager;
				manager3.OnTileCacheProgress = (TileCacheProgress)Delegate.Combine(manager3.OnTileCacheProgress, new TileCacheProgress(OnTileCacheProgress));
				MainMap.MouseMove += MainMap_MouseMove;
				MainMap.MouseDown += MainMap_MouseDown;
				MainMap.MouseUp += MainMap_MouseUp;
				MainMap.MouseDoubleClick += MainMap_MouseDoubleClick;
				comboBoxMapType.ValueMember = "Name";
				var availableProviders = new List<GMapProvider>(GMapProviders.List);
				if (!availableProviders.Contains(initialProvider)) availableProviders.Add(initialProvider);
				comboBoxMapType.DataSource = availableProviders;
				comboBoxMapType.SelectedItem = MainMap.MapProvider;
				comboBoxMode.DataSource = Enum.GetValues(typeof(AccessMode));
				comboBoxMode.SelectedItem = MainMap.Manager.Mode;
				textBoxLat.Text = MainMap.Position.Lat.ToString("F6", CultureInfo.InvariantCulture);
				textBoxLng.Text = MainMap.Position.Lng.ToString("F6", CultureInfo.InvariantCulture);
				SetDmsInputs(MainMap.Position);
				checkBoxUseRouteCache.Checked = MainMap.Manager.UseRouteCache;
				MobileLogFrom.Value = DateTime.Today;
				MobileLogTo.Value = DateTime.Now;
				trackBar1.Minimum = MainMap.MinZoom * 100;
				trackBar1.Maximum = MainMap.MaxZoom * 100;
				trackBar1.TickFrequency = 100;
				ToolStripManager.Renderer = new Office2007Renderer();
				flightWorker.DoWork += flight_DoWork;
				flightWorker.ProgressChanged += flight_ProgressChanged;
				flightWorker.WorkerSupportsCancellation = true;
				flightWorker.WorkerReportsProgress = true;
				transportWorker.DoWork += transport_DoWork;
				transportWorker.ProgressChanged += transport_ProgressChanged;
				transportWorker.WorkerSupportsCancellation = true;
				transportWorker.WorkerReportsProgress = true;
				connectionsWorker.DoWork += connectionsWorker_DoWork;
				connectionsWorker.ProgressChanged += connectionsWorker_ProgressChanged;
				connectionsWorker.WorkerSupportsCancellation = true;
				connectionsWorker.WorkerReportsProgress = true;
				ipInfoSearchWorker.DoWork += ipInfoSearchWorker_DoWork;
				ipInfoSearchWorker.WorkerSupportsCancellation = true;
				iptracerWorker.DoWork += iptracerWorker_DoWork;
				iptracerWorker.WorkerSupportsCancellation = true;
				GridConnections.AutoGenerateColumns = false;
				IpCache.CacheLocation = MainMap.CacheLocation;
				timerPerf.Tick += timer_Tick;
				MainMap.Overlays.Add(routes);
				MainMap.Overlays.Add(polygons);
				MainMap.Overlays.Add(objects);
				MainMap.Overlays.Add(top);
				routes.Routes.CollectionChanged += Routes_CollectionChanged;
				objects.Markers.CollectionChanged += Markers_CollectionChanged;
				currentMarker = new GMarkerGoogle(MainMap.Position, GMarkerGoogleType.arrow);
				currentMarker.IsHitTestVisible = false;
				top.Markers.Add(currentMarker);
				checkBoxCurrentMarker.Checked = true;
				UpdateWorkspaceStatus();
			}
		}
	}

	public T DeepClone<T>(T obj)
	{
		using MemoryStream memoryStream = new MemoryStream();
		BinaryFormatter binaryFormatter = new BinaryFormatter();
		binaryFormatter.Serialize(memoryStream, obj);
		memoryStream.Position = 0L;
		return (T)binaryFormatter.Deserialize(memoryStream);
	}

	private void Markers_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		textBoxMarkerCount.Text = objects.Markers.Count.ToString();
	}

	private void Routes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		textBoxrouteCount.Text = routes.Routes.Count.ToString();
	}

	private double NextDouble(Random rng, double min, double max)
	{
		return min + rng.NextDouble() * (max - min);
	}

	private void timer_Tick(object sender, EventArgs e)
	{
		GMapMarker gMapMarker = new GMarkerGoogle(new PointLatLng(NextDouble(rnd, MainMap.ViewArea.Top, MainMap.ViewArea.Bottom), NextDouble(rnd, MainMap.ViewArea.Left, MainMap.ViewArea.Right)), GMarkerGoogleType.green_pushpin);
		gMapMarker.ToolTipText = checked(tt++).ToString();
		gMapMarker.ToolTipMode = MarkerTooltipMode.Always;
		objects.Markers.Add(gMapMarker);
		if (tt >= 333)
		{
			timerPerf.Stop();
			tt = 0;
		}
	}

	private void flight_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		MainMap.HoldInvalidation = true;
		lock (flights)
		{
			if (flightBounds != MainMap.ViewArea)
			{
				flightBounds = MainMap.ViewArea;
				foreach (GMapMarker marker in objects.Markers)
				{
					if (!flightBounds.Contains(marker.Position))
					{
						marker.IsVisible = false;
					}
					else
					{
						marker.IsVisible = true;
					}
				}
			}
			foreach (FlightRadarData flight in flights)
			{
				if (!flightMarkers.TryGetValue(flight.Id, out var value))
				{
					value = new GMarkerArrow(flight.point);
					value.Tag = flight.Id;
					value.ToolTipMode = MarkerTooltipMode.OnMouseOver;
					(value as GMarkerArrow).Bearing = flight.bearing;
					flightMarkers[flight.Id] = value;
					objects.Markers.Add(value);
				}
				else
				{
					value.Position = flight.point;
					(value as GMarkerArrow).Bearing = flight.bearing;
				}
				value.ToolTipText = flight.name + ", " + flight.altitude + ", " + flight.speed;
				if (currentFlight != null && currentFlight == value)
				{
					MainMap.Position = value.Position;
					MainMap.Bearing = flight.bearing;
				}
			}
		}
		if (firstLoadFlight)
		{
			MainMap.Zoom = 5.0;
			MainMap.SetZoomToFitRect(new RectLatLng(54.4955675218741, -495.0 / 512.0, 28.916015625, 13.3830987326932));
			firstLoadFlight = false;
		}
		MainMap.Refresh();
	}

	private void flight_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!flightWorker.CancellationPending)
		{
			try
			{
				lock (flights)
				{
					Stuff.GetFlightRadarData(flights, flightBounds);
				}
				flightWorker.ReportProgress(100);
			}
			catch (Exception)
			{
			}
			Thread.Sleep(5000);
		}
		flightMarkers.Clear();
	}

	private void transport_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		MainMap.HoldInvalidation = true;
		lock (trolleybus)
		{
			foreach (VehicleData trolleybu in trolleybus)
			{
				if (!trolleybusMarkers.TryGetValue(trolleybu.Id, out var value))
				{
					value = new GMarkerGoogle(new PointLatLng(trolleybu.Lat, trolleybu.Lng), GMarkerGoogleType.red_small);
					value.Tag = trolleybu.Id;
					value.ToolTipMode = MarkerTooltipMode.OnMouseOver;
					trolleybusMarkers[trolleybu.Id] = value;
					objects.Markers.Add(value);
				}
				else
				{
					value.Position = new PointLatLng(trolleybu.Lat, trolleybu.Lng);
				}
				GMapMarker gMapMarker = value;
				string[] array = new string[5] { "Trolley ", trolleybu.Line, null, null, null };
				double? bearing = trolleybu.Bearing;
				string text;
				if (!bearing.HasValue)
				{
					text = string.Empty;
				}
				else
				{
					bearing = trolleybu.Bearing;
					text = ", bearing: " + bearing.Value;
				}
				array[2] = text;
				array[3] = ", ";
				array[4] = trolleybu.Time;
				gMapMarker.ToolTipText = string.Concat(array);
				if (currentTransport != null && currentTransport == value)
				{
					MainMap.Position = value.Position;
					bearing = trolleybu.Bearing;
					if (bearing.HasValue)
					{
						Map mainMap = MainMap;
						bearing = trolleybu.Bearing;
						mainMap.Bearing = (float)bearing.Value;
					}
				}
			}
		}
		lock (bus)
		{
			foreach (VehicleData item in bus)
			{
				if (!busMarkers.TryGetValue(item.Id, out var value2))
				{
					value2 = new GMarkerGoogle(new PointLatLng(item.Lat, item.Lng), GMarkerGoogleType.green_small);
					value2.Tag = item.Id;
					value2.ToolTipMode = MarkerTooltipMode.OnMouseOver;
					busMarkers[item.Id] = value2;
					objects.Markers.Add(value2);
				}
				else
				{
					value2.Position = new PointLatLng(item.Lat, item.Lng);
				}
				GMapMarker gMapMarker2 = value2;
				string[] array2 = new string[5] { "Bus ", item.Line, null, null, null };
				double? bearing = item.Bearing;
				string text2;
				if (!bearing.HasValue)
				{
					text2 = string.Empty;
				}
				else
				{
					bearing = item.Bearing;
					text2 = ", bearing: " + bearing.Value;
				}
				array2[2] = text2;
				array2[3] = ", ";
				array2[4] = item.Time;
				gMapMarker2.ToolTipText = string.Concat(array2);
				if (currentTransport != null && currentTransport == value2)
				{
					MainMap.Position = value2.Position;
					bearing = item.Bearing;
					if (bearing.HasValue)
					{
						Map mainMap2 = MainMap;
						bearing = item.Bearing;
						mainMap2.Bearing = (float)bearing.Value;
					}
				}
			}
		}
		if (firstLoadTrasport)
		{
			MainMap.Zoom = 5.0;
			MainMap.ZoomAndCenterMarkers("objects");
			firstLoadTrasport = false;
		}
		MainMap.Refresh();
	}

	private void transport_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!transportWorker.CancellationPending)
		{
			try
			{
				lock (trolleybus)
				{
					Stuff.GetVilniusTransportData(TransportType.TrolleyBus, string.Empty, trolleybus);
				}
				lock (bus)
				{
					Stuff.GetVilniusTransportData(TransportType.Bus, string.Empty, bus);
				}
				transportWorker.ReportProgress(100);
			}
			catch (Exception)
			{
			}
			Thread.Sleep(2000);
		}
		trolleybusMarkers.Clear();
		busMarkers.Clear();
	}

	private void ipInfoSearchWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		while (!ipInfoSearchWorker.CancellationPending)
		{
			try
			{
				string text = string.Empty;
				lock (TcpStateNeedLocationInfo)
				{
					using List<string>.Enumerator enumerator = TcpStateNeedLocationInfo.GetEnumerator();
					if (enumerator.MoveNext())
					{
						string current = enumerator.Current;
						if (text.Length > 0)
						{
							text += ",";
						}
						text += current;
					}
				}
				if (string.IsNullOrEmpty(text))
				{
					break;
				}
				List<IpInfo> ipHostInfo = GetIpHostInfo(text);
				foreach (IpInfo item in ipHostInfo)
				{
					lock (TcpState)
					{
						if (!TcpState.TryGetValue(item.Ip, out var value))
						{
							continue;
						}
						value.CountryName = item.CountryName;
						value.RegionName = item.RegionName;
						value.City = item.City;
						value.Latitude = item.Latitude;
						value.Longitude = item.Longitude;
						value.TracePoint = false;
						if (value.CountryName != "Reserved")
						{
							value.Ip = item.Ip;
							lock (TcpStateNeedtraceInfo)
							{
								if (!TcpStateNeedtraceInfo.Contains(item.Ip))
								{
									TcpStateNeedtraceInfo.Enqueue(item.Ip);
								}
							}
						}
						lock (TcpStateNeedLocationInfo)
						{
							TcpStateNeedLocationInfo.Remove(item.Ip);
						}
					}
				}
				ipHostInfo.Clear();
			}
			catch (Exception)
			{
			}
			Thread.Sleep(1111);
		}
	}

	private void iptracerWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		checked
		{
			while (!iptracerWorker.CancellationPending)
			{
				try
				{
					string text = string.Empty;
					lock (TcpStateNeedtraceInfo)
					{
						if (TcpStateNeedtraceInfo.Count > 0)
						{
							text = TcpStateNeedtraceInfo.Dequeue();
						}
					}
					if (string.IsNullOrEmpty(text))
					{
						break;
					}
					string text2 = string.Empty;
					bool flag = false;
					List<PingReply> value;
					lock (TraceRoutes)
					{
						flag = TraceRoutes.TryGetValue(text, out value);
					}
					if (!flag)
					{
						value = TraceRoute.GetTraceRoute(text);
						if (value != null)
						{
							if (value[value.Count - 1].Status == IPStatus.Success)
							{
								foreach (PingReply item in value)
								{
									if (!item.ToString().StartsWith("192.168.") && !item.ToString().StartsWith("127.0."))
									{
										if (text2.Length > 0)
										{
											text2 += ",";
										}
										text2 += item.Address.ToString();
									}
								}
								if (!string.IsNullOrEmpty(text2))
								{
									List<IpInfo> ipHostInfo = GetIpHostInfo(text2);
									if (ipHostInfo.Count > 0)
									{
										for (int i = 0; i < ipHostInfo.Count; i++)
										{
											IpInfo ipInfo = ipHostInfo[i];
											ipInfo.TracePoint = true;
											if (ipInfo.CountryName != "Reserved")
											{
												lock (TcpTracePoints)
												{
													TcpTracePoints[ipInfo.Ip] = ipInfo;
												}
											}
										}
										ipHostInfo.Clear();
										lock (TraceRoutes)
										{
											TraceRoutes[text] = value;
										}
									}
								}
							}
							else
							{
								lock (TcpStateNeedtraceInfo)
								{
									TcpStateNeedtraceInfo.Enqueue(text);
								}
							}
						}
					}
				}
				catch (Exception)
				{
				}
				Thread.Sleep(3333);
			}
		}
	}

	private void connectionsWorker_DoWork(object sender, DoWorkEventArgs e)
	{
		IPGlobalProperties.GetIPGlobalProperties();
		checked
		{
			while (!connectionsWorker.CancellationPending)
			{
				try
				{
					lock (TcpState)
					{
						CountryStatus.Clear();
						ManagedIpHelper.UpdateExtendedTcpTable(sorted: false);
						foreach (TcpRow tcpRow in ManagedIpHelper.TcpRows)
						{
							string text = tcpRow.RemoteEndPoint.Address.ToString();
							if (text.StartsWith("192.168.") || text.StartsWith("127.0."))
							{
								continue;
							}
							if (!TcpState.TryGetValue(text, out var value))
							{
								value = new IpInfo();
								TcpState[text] = value;
								lock (TcpStateNeedLocationInfo)
								{
									if (!TcpStateNeedLocationInfo.Contains(text))
									{
										TcpStateNeedLocationInfo.Add(text);
										if (!ipInfoSearchWorker.IsBusy)
										{
											ipInfoSearchWorker.RunWorkerAsync();
										}
									}
								}
							}
							value.State = tcpRow.State;
							value.Port = tcpRow.RemoteEndPoint.Port;
							value.StatusTime = DateTime.Now;
							try
							{
								if (!ProcessList.TryGetValue(tcpRow.ProcessId, out var value2))
								{
									value2 = Process.GetProcessById(tcpRow.ProcessId);
									ProcessList[tcpRow.ProcessId] = value2;
								}
								value.ProcessName = value2.ProcessName;
							}
							catch
							{
							}
							if (!string.IsNullOrEmpty(value.CountryName))
							{
								if (!CountryStatus.ContainsKey(value.CountryName))
								{
									CountryStatus[value.CountryName] = 1;
								}
								else
								{
									CountryStatus[value.CountryName]++;
								}
							}
						}
					}
					if (TryTraceConnection && !iptracerWorker.IsBusy)
					{
						lock (TcpStateNeedtraceInfo)
						{
							if (TcpStateNeedtraceInfo.Count > 0)
							{
								iptracerWorker.RunWorkerAsync();
							}
						}
					}
					connectionsWorker.ReportProgress(100);
				}
				catch (Exception)
				{
				}
				Thread.Sleep(3333);
			}
			tcpConnections.Clear();
		}
	}

	private void connectionsWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
	{
		try
		{
			MainMap.HoldInvalidation = true;
			SelectedCountries.Clear();
			int rowCount = GridConnections.Rows.GetRowCount(DataGridViewElementStates.Selected);
			if (rowCount > 0)
			{
				for (int i = 0; i < rowCount; i = checked(i + 1))
				{
					string item = GridConnections.SelectedRows[i].Cells[0].Value as string;
					SelectedCountries.Add(item);
				}
			}
			ComparerIpStatus.SortOnlyCountryName = rowCount != 0;
			lock (TcpState)
			{
				bool flag = true;
				foreach (KeyValuePair<string, IpInfo> item2 in TcpState)
				{
					if (!tcpConnections.TryGetValue(item2.Key, out var value))
					{
						if (string.IsNullOrEmpty(item2.Value.Ip))
						{
							continue;
						}
						value = new GMarkerGoogle(new PointLatLng(item2.Value.Latitude, item2.Value.Longitude), GMarkerGoogleType.green_small);
						value.ToolTipMode = MarkerTooltipMode.OnMouseOver;
						value.Tag = item2.Value.CountryName;
						tcpConnections[item2.Key] = value;
						if (rowCount > 0 && !SelectedCountries.Contains(item2.Value.CountryName))
						{
							continue;
						}
						objects.Markers.Add(value);
						UpdateMarkerTcpIpToolTip(value, item2.Value, "(" + objects.Markers.Count + ") ");
						if (flag)
						{
							if (checkBoxTcpIpSnap.Checked && !MainMap.IsDragging)
							{
								MainMap.Position = value.Position;
							}
							flag = false;
							if (lastTcpmarker != null)
							{
								value.ToolTipMode = MarkerTooltipMode.Always;
								lastTcpmarker.ToolTipMode = MarkerTooltipMode.OnMouseOver;
							}
							lastTcpmarker = value;
						}
						continue;
					}
					if (DateTime.Now - item2.Value.StatusTime > TimeSpan.FromSeconds(8.0) || (rowCount > 0 && !SelectedCountries.Contains(item2.Value.CountryName)))
					{
						objects.Markers.Remove(value);
						if (tcpRoutes.TryGetValue(item2.Key, out var value2))
						{
							routes.Routes.Remove(value2);
						}
						lock (TcpStateNeedLocationInfo)
						{
							TcpStateNeedLocationInfo.Remove(item2.Key);
						}
						continue;
					}
					value.Position = new PointLatLng(item2.Value.Latitude, item2.Value.Longitude);
					if (!objects.Markers.Contains(value))
					{
						objects.Markers.Add(value);
					}
					UpdateMarkerTcpIpToolTip(value, item2.Value, string.Empty);
					if (!TryTraceConnection)
					{
						continue;
					}
					if (!tcpRoutes.TryGetValue(item2.Key, out var value3))
					{
						lock (TraceRoutes)
						{
							if (!TraceRoutes.TryGetValue(item2.Key, out var value4) || value4 == null)
							{
								continue;
							}
							List<PointLatLng> list = new List<PointLatLng>();
							foreach (PingReply item3 in value4)
							{
								lock (TcpTracePoints)
								{
									if (TcpTracePoints.TryGetValue(item3.Address.ToString(), out var value5) && !string.IsNullOrEmpty(value5.Ip))
									{
										list.Add(new PointLatLng(value5.Latitude, value5.Longitude));
									}
								}
							}
							if (list.Count > 0)
							{
								value3 = new GMapRoute(list, item2.Value.CountryName);
								value3.Stroke = new Pen(GetRandomColor());
								value3.Stroke.Width = 4f;
								value3.Stroke.DashStyle = DashStyle.DashDot;
								value3.Stroke.StartCap = LineCap.NoAnchor;
								value3.Stroke.EndCap = LineCap.ArrowAnchor;
								value3.Stroke.LineJoin = LineJoin.Round;
								routes.Routes.Add(value3);
								tcpRoutes[item2.Key] = value3;
							}
						}
					}
					else if (!routes.Routes.Contains(value3))
					{
						routes.Routes.Add(value3);
					}
				}
				if (panelMenu.Expand && xPanderPanelLive.Expand)
				{
					bool flag2 = CountryStatusView.Count == 0;
					if (!ComparerIpStatus.SortOnlyCountryName)
					{
						CountryStatusView.Clear();
					}
					foreach (KeyValuePair<string, int> c in CountryStatus)
					{
						IpStatus ipStatus = new IpStatus
						{
							CountryName = c.Key,
							ConnectionsCount = c.Value
						};
						if (ComparerIpStatus.SortOnlyCountryName)
						{
							int num = CountryStatusView.FindIndex((IpStatus p) => p.CountryName == c.Key);
							if (num >= 0)
							{
								CountryStatusView[num] = ipStatus;
							}
						}
						else
						{
							CountryStatusView.Add(ipStatus);
						}
					}
					CountryStatusView.Sort(ComparerIpStatus);
					GridConnections.RowCount = CountryStatusView.Count;
					GridConnections.Refresh();
					if (flag2)
					{
						GridConnections.ClearSelection();
					}
				}
			}
			MainMap.Refresh();
		}
		catch (Exception)
		{
		}
	}

	private void GridConnections_CellValueNeeded(object sender, DataGridViewCellValueEventArgs e)
	{
		if (e.RowIndex >= CountryStatusView.Count)
		{
			return;
		}
		IpStatus ipStatus = CountryStatusView[e.RowIndex];
		string name = GridConnections.Columns[e.ColumnIndex].Name;
		if (!(name == "CountryName"))
		{
			if (name == "ConnectionsCount")
			{
				e.Value = ipStatus.ConnectionsCount;
			}
		}
		else
		{
			e.Value = ipStatus.CountryName;
		}
	}

	private Color GetRandomColor()
	{
		byte red = Convert.ToByte(rnd.Next(0, 111));
		byte green = Convert.ToByte(rnd.Next(0, 111));
		byte blue = Convert.ToByte(rnd.Next(0, 111));
		return Color.FromArgb(144, red, green, blue);
	}

	private void UpdateMarkerTcpIpToolTip(GMapMarker marker, IpInfo tcp, string info)
	{
		marker.ToolTipText = tcp.State.ToString();
		if (!string.IsNullOrEmpty(tcp.ProcessName))
		{
			marker.ToolTipText = marker.ToolTipText + " by " + tcp.ProcessName;
		}
		if (!string.IsNullOrEmpty(tcp.CountryName))
		{
			marker.ToolTipText = marker.ToolTipText + "\n" + tcp.CountryName;
		}
		if (!string.IsNullOrEmpty(tcp.City))
		{
			marker.ToolTipText = marker.ToolTipText + ", " + tcp.City;
		}
		else if (!string.IsNullOrEmpty(tcp.RegionName))
		{
			marker.ToolTipText = marker.ToolTipText + ", " + tcp.RegionName;
		}
		marker.ToolTipText = marker.ToolTipText + "\n" + tcp.Ip + ":" + tcp.Port + "\n" + info;
	}

	private List<IpInfo> GetIpHostInfo(string iplist)
	{
		List<IpInfo> list = new List<IpInfo>();
		string apiKey = Environment.GetEnvironmentVariable("GMAP_IPINFODB_API_KEY");
		if (string.IsNullOrWhiteSpace(apiKey))
		{
			return list;
		}
		bool flag = false;
		string text = string.Empty;
		string[] array = iplist.Split(',');
		foreach (string text2 in array)
		{
			IpInfo dataFromCache = IpCache.GetDataFromCache(text2);
			if (dataFromCache != null)
			{
				list.Add(dataFromCache);
				continue;
			}
			if (text.Length > 0)
			{
				text += ",";
			}
			text += text2;
		}
		if (!string.IsNullOrEmpty(text))
		{
			string requestUriString = $"http://api.ipinfodb.com/v2/ip_query.php?key={Uri.EscapeDataString(apiKey)}&ip={text}&timezone=false";
			while (true)
			{
				list.Clear();
				try
				{
					HttpWebRequest obj = WebRequest.Create(requestUriString) as HttpWebRequest;
					string xml = string.Empty;
					using (HttpWebResponse httpWebResponse = obj.GetResponse() as HttpWebResponse)
					{
						using (StreamReader streamReader = new StreamReader(httpWebResponse.GetResponseStream(), Encoding.UTF8))
						{
							xml = streamReader.ReadToEnd();
						}
						httpWebResponse.Close();
					}
					XmlDocument xmlDocument = new XmlDocument();
					xmlDocument.LoadXml(xml);
					foreach (XmlNode item in xmlDocument.SelectNodes("/Response"))
					{
						string innerText = item.SelectSingleNode("Ip").InnerText;
						IpInfo ipInfo = new IpInfo();
						ipInfo.Ip = innerText;
						ipInfo.CountryName = item.SelectSingleNode("CountryName").InnerText;
						ipInfo.RegionName = item.SelectSingleNode("RegionName").InnerText;
						ipInfo.City = item.SelectSingleNode("City").InnerText;
						ipInfo.Latitude = double.Parse(item.SelectSingleNode("Latitude").InnerText, CultureInfo.InvariantCulture);
						ipInfo.Longitude = double.Parse(item.SelectSingleNode("Longitude").InnerText, CultureInfo.InvariantCulture);
						ipInfo.CacheTime = DateTime.Now;
						list.Add(ipInfo);
						IpCache.PutDataToCache(innerText, ipInfo);
					}
				}
				catch (Exception)
				{
					if (flag)
					{
						break;
					}
					flag = true;
					requestUriString = $"http://backup.ipinfodb.com/v2/ip_query.php?key={Uri.EscapeDataString(apiKey)}&ip={iplist}&timezone=false";
					continue;
				}
				break;
			}
		}
		return list;
	}

	private void RegeneratePolygon()
	{
		List<PointLatLng> list = new List<PointLatLng>();
		foreach (GMapMarker marker in objects.Markers)
		{
			if (marker is GMapMarkerRect)
			{
				marker.Tag = list.Count;
				list.Add(marker.Position);
			}
		}
		if (polygon == null)
		{
			polygon = new GMapPolygon(list, "polygon test");
			polygon.IsHitTestVisible = true;
			polygons.Polygons.Add(polygon);
			return;
		}
		polygon.Points.Clear();
		polygon.Points.AddRange(list);
		if (polygons.Polygons.Count == 0)
		{
			polygons.Polygons.Add(polygon);
		}
		else
		{
			MainMap.UpdatePolygonLocalPosition(polygon);
		}
	}

	private void AddGpsMobileLogRoutes(string file)
	{
		try
		{
			MainMap.HoldInvalidation = true;
			DateTime? dateTime = null;
			DateTime? dateTime2 = null;
			if (MobileLogFrom.Checked)
			{
				dateTime = MobileLogFrom.Value.ToUniversalTime();
			}
			if (MobileLogTo.Checked)
			{
				dateTime2 = MobileLogTo.Value.ToUniversalTime();
			}
			IEnumerable<List<GpsLog>> routesFromMobileLog = Stuff.GetRoutesFromMobileLog(file, dateTime, dateTime2, 10.0);
			if (routes != null)
			{
				List<PointLatLng> list = new List<PointLatLng>();
				List<List<GpsLog>> list2 = new List<List<GpsLog>>(routesFromMobileLog);
				PointLatLng item = PointLatLng.Empty;
				foreach (List<GpsLog> item3 in list2)
				{
					if (!item.IsEmpty && item3.Count > 0)
					{
						list.Clear();
						list.Add(item);
						list.Add(item3[0].Position);
						GMapRoute gMapRoute = new GMapRoute(list, "");
						gMapRoute.Stroke = new Pen(GMapRoute.DefaultStroke.Brush);
						gMapRoute.Stroke.Color = Color.Red;
						gMapRoute.Stroke.Width = 2f;
						routes.Routes.Add(gMapRoute);
					}
					list.Clear();
					foreach (GpsLog item4 in item3)
					{
						list.Add(item4.Position);
					}
					if (list.Count > 0)
					{
						item = list[checked(list.Count - 1)];
						GMapRoute item2 = new GMapRoute(list, "");
						routes.Routes.Add(item2);
					}
					else
					{
						item = PointLatLng.Empty;
					}
				}
				list2.Clear();
				list2 = null;
				list.Clear();
				list = null;
			}
			MainMap.Refresh();
		}
		catch (Exception)
		{
		}
	}

	private void AddLocationLithuania(string place)
	{
		GeoCoderStatusCode status = GeoCoderStatusCode.UNKNOWN_ERROR;
		PointLatLng? point = GMapProviders.GoogleMap.GetPoint("Lithuania, " + place, out status);
		if (point.HasValue && status == GeoCoderStatusCode.OK)
		{
			GMarkerGoogle gMarkerGoogle = new GMarkerGoogle(point.Value, GMarkerGoogleType.green);
			gMarkerGoogle.ToolTip = new GMapRoundedToolTip(gMarkerGoogle);
			GMapMarkerRect gMapMarkerRect = new GMapMarkerRect(point.Value);
			gMapMarkerRect.InnerMarker = gMarkerGoogle;
			gMapMarkerRect.ToolTipText = place;
			gMapMarkerRect.ToolTipMode = MarkerTooltipMode.Always;
			objects.Markers.Add(gMarkerGoogle);
			objects.Markers.Add(gMapMarkerRect);
		}
	}

	private bool TryExtractLeafletjs()
	{
		checked
		{
			try
			{
				string text = string.Empty;
				string[] manifestResourceNames = Assembly.GetExecutingAssembly().GetManifestResourceNames();
				foreach (string text2 in manifestResourceNames)
				{
					if (!text2.Contains("leafletjs"))
					{
						continue;
					}
					string text3 = text2.Replace("Demo.WindowsForms.", string.Empty);
					text3 = text3.Replace(".", "\\");
					int num = text3.LastIndexOf("\\");
					string text4 = text3.Substring(0, num) + "." + text3.Substring(num + 1, text3.Length - num - 1);
					using Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(text2);
					string text5 = MainMap.CacheLocation + text4;
					if (text5.Contains("gmap.html"))
					{
						text = text5;
					}
					string directoryName = Path.GetDirectoryName(text5);
					if (!Directory.Exists(directoryName))
					{
						Directory.CreateDirectory(directoryName);
					}
					using FileStream fileStream = File.Create(text5, (int)stream.Length);
					byte[] array = new byte[stream.Length];
					stream.Read(array, 0, array.Length);
					fileStream.Write(array, 0, array.Length);
				}
				if (!string.IsNullOrEmpty(text))
				{
					Process.Start(text);
				}
			}
			catch (Exception)
			{
				return false;
			}
			return true;
		}
	}

	private void OnTileCacheComplete()
	{
		long size = 0L;
		int db = 0;
		checked
		{
			try
			{
				FileInfo[] files = new DirectoryInfo(MainMap.CacheLocation).GetFiles("*.gmdb", SearchOption.AllDirectories);
				foreach (FileInfo fileInfo in files)
				{
					size += fileInfo.Length;
					db++;
				}
			}
			catch
			{
			}
			if (IsDisposed)
			{
				return;
			}
			MethodInvoker method = () =>
			{
				textBoxCacheSize.Text = string.Format(CultureInfo.InvariantCulture, "{0} db in {1:00} MB", new object[2]
				{
					db,
					(double)size / 1048576.0
				});
				textBoxCacheStatus.Text = "all tiles saved!";
			};
			if (!IsDisposed)
			{
				try
				{
					Invoke(method);
				}
				catch (Exception)
				{
				}
			}
		}
	}

	private void OnTileCacheStart()
	{
		if (!IsDisposed)
		{
			MethodInvoker method = () =>
			{
				textBoxCacheStatus.Text = "saving tiles...";
			};
			Invoke(method);
		}
	}

	private void OnTileCacheProgress(int left)
	{
		if (!IsDisposed)
		{
			MethodInvoker method = () =>
			{
				textBoxCacheStatus.Text = left + " tile to save...";
			};
			Invoke(method);
		}
	}

	private void MainMap_OnMarkerLeave(GMapMarker item)
	{
		if (item is GMapMarkerRect)
		{
			CurentRectMarker = null;
			(item as GMapMarkerRect).Pen.Color = Color.Blue;
		}
	}

	private void MainMap_OnMarkerEnter(GMapMarker item)
	{
		if (item is GMapMarkerRect)
		{
			GMapMarkerRect gMapMarkerRect = item as GMapMarkerRect;
			gMapMarkerRect.Pen.Color = Color.Red;
			CurentRectMarker = gMapMarkerRect;
		}
	}

	private void MainMap_OnPolygonLeave(GMapPolygon item)
	{
		currentPolygon = null;
		item.Stroke.Color = Color.MidnightBlue;
	}

	private void MainMap_OnPolygonEnter(GMapPolygon item)
	{
		currentPolygon = item;
		item.Stroke.Color = Color.Red;
	}

	private void MainMap_OnRouteLeave(GMapRoute item)
	{
		currentRoute = null;
		item.Stroke.Color = Color.MidnightBlue;
	}

	private void MainMap_OnRouteEnter(GMapRoute item)
	{
		currentRoute = item;
		item.Stroke.Color = Color.Red;
	}

	private void MainMap_OnMapTypeChanged(GMapProvider type)
	{
		comboBoxMapType.SelectedItem = type;
		checked
		{
			trackBar1.Minimum = MainMap.MinZoom * 100;
			trackBar1.Maximum = MainMap.MaxZoom * 100;
			if (radioButtonFlight.Checked)
			{
				MainMap.ZoomAndCenterMarkers("objects");
			}
		}
	}

	private void MainMap_MouseUp(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			isMouseDown = false;
		}
	}

	private void MainMap_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		GMapMarkerCircle item = new GMapMarkerCircle(MainMap.FromLocalToLatLng(e.X, e.Y));
		objects.Markers.Add(item);
	}

	private void MainMap_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left)
		{
			isMouseDown = true;
			if (currentMarker.IsVisible)
			{
				currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
				GPoint p = MainMap.MapProvider.Projection.FromLatLngToPixel(currentMarker.Position.Lat, currentMarker.Position.Lng, checked((int)MainMap.Zoom));
				MainMap.MapProvider.Projection.FromPixelToTileXY(p);
			}
		}
	}

	private void MainMap_MouseMove(object sender, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left || !isMouseDown)
		{
			return;
		}
		if (CurentRectMarker == null)
		{
			if (currentMarker.IsVisible)
			{
				currentMarker.Position = MainMap.FromLocalToLatLng(e.X, e.Y);
			}
		}
		else
		{
			PointLatLng pointLatLng = MainMap.FromLocalToLatLng(e.X, e.Y);
			int? num = (int?)CurentRectMarker.Tag;
			if (num.HasValue && num < polygon.Points.Count)
			{
				polygon.Points[num.Value] = pointLatLng;
				MainMap.UpdatePolygonLocalPosition(polygon);
			}
			if (currentMarker.IsVisible)
			{
				currentMarker.Position = pointLatLng;
			}
			CurentRectMarker.Position = pointLatLng;
			if (CurentRectMarker.InnerMarker != null)
			{
				CurentRectMarker.InnerMarker.Position = pointLatLng;
			}
		}
		MainMap.Refresh();
	}

	private void MainMap_OnMapZoomChanged()
	{
		trackBar1.Value = checked((int)(MainMap.Zoom * 100.0));
		textBoxZoomCurrent.Text = MainMap.Zoom.ToString();
	}

	private void MainMap_OnMarkerClick(GMapMarker item, MouseEventArgs e)
	{
		if (e.Button != MouseButtons.Left)
		{
			return;
		}
		if (item is GMapMarkerRect)
		{
			Placemark? placemark = GMapProviders.GoogleMap.GetPlacemark(item.Position, out var status);
			if (status == GeoCoderStatusCode.OK && placemark.HasValue)
			{
				(item as GMapMarkerRect).ToolTipText = placemark.Value.Address;
				MainMap.Invalidate(invalidateChildren: false);
			}
		}
		else if (item.Tag != null)
		{
			if (currentTransport != null)
			{
				currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
				currentTransport = null;
			}
			currentTransport = item;
			currentTransport.ToolTipMode = MarkerTooltipMode.Always;
		}
	}

	private void MainMap_OnTileLoadStart()
	{
		MethodInvoker method = () =>
		{
			panelMenu.Text = "Menu: loading tiles...";
		};
		try
		{
			BeginInvoke(method);
		}
		catch
		{
		}
	}

	private void MainMap_OnTileLoadComplete(long ElapsedMilliseconds)
	{
		MainMap.ElapsedMilliseconds = ElapsedMilliseconds;
		MethodInvoker method = () =>
		{
			panelMenu.Text = "Menu, last load in " + MainMap.ElapsedMilliseconds + "ms";
			textBoxMemory.Text = string.Format(CultureInfo.InvariantCulture, "{0:0.00} MB of {1:0.00} MB", new object[2]
			{
				MainMap.Manager.MemoryCache.Size,
				MainMap.Manager.MemoryCache.Capacity
			});
		};
		try
		{
			BeginInvoke(method);
		}
		catch
		{
		}
	}

	private void MainMap_OnPositionChanged(PointLatLng point)
	{
		textBoxLatCurrent.Text = point.Lat.ToString(CultureInfo.InvariantCulture);
		textBoxLngCurrent.Text = point.Lng.ToString(CultureInfo.InvariantCulture);
		lock (flights)
		{
			lastPosition = point;
			lastZoom = checked((int)MainMap.Zoom);
		}
	}

	private void MainForm_Load(object sender, EventArgs e)
	{
		trackBar1.Value = checked((int)MainMap.Zoom * 100);
		var work = Screen.FromControl(this).WorkingArea;
		MinimumSize = new Size(Math.Min(MinimumSize.Width, work.Width), Math.Min(MinimumSize.Height, work.Height));
		Size = new Size(Math.Min(Width, work.Width), Math.Min(Height, work.Height));
		Location = new Point(work.Left + (work.Width - Width) / 2, work.Top + (work.Height - Height) / 2);
		MainMap.Focus();
	}

	private void xPanderPanel1_Click(object sender, EventArgs e)
	{
		xPanderPanelList1.Expand(xPanderPanelMain);
	}

	private void xPanderPanelCache_Click(object sender, EventArgs e)
	{
		xPanderPanelList1.Expand(xPanderPanelCache);
	}

	private void xPanderPanelLive_Click(object sender, EventArgs e)
	{
		xPanderPanelList1.Expand(xPanderPanelLive);
	}

	private void xPanderPanelInfo_Click(object sender, EventArgs e)
	{
		xPanderPanelList1.Expand(xPanderPanelInfo);
	}

	private void comboBoxMapType_DropDownClosed(object sender, EventArgs e)
	{
		if (!UserAcceptedLicenseOnce)
		{
			if (File.Exists(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt"))
			{
				string text = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + Path.DirectorySeparatorChar + "License.txt");
				Demo.WindowsForms.Forms.Message message = new Demo.WindowsForms.Forms.Message();
				message.richTextBox1.Text = text;
				if (DialogResult.Yes == message.ShowDialog())
				{
					UserAcceptedLicenseOnce = true;
					Text = Text + " - license accepted by " + Environment.UserName + " at " + DateTime.Now;
				}
			}
			else
			{
				UserAcceptedLicenseOnce = true;
			}
		}
		if (UserAcceptedLicenseOnce)
		{
			MainMap.MapProvider = comboBoxMapType.SelectedItem as GMapProvider;
			return;
		}
		MainMap.MapProvider = GMapProviders.BingSatelliteMap;
		comboBoxMapType.SelectedItem = MainMap.MapProvider;
	}

	private void comboBoxMode_DropDownClosed(object sender, EventArgs e)
	{
		MainMap.Manager.Mode = (AccessMode)comboBoxMode.SelectedValue;
		MainMap.ReloadMap();
	}

	private void trackBar1_ValueChanged(object sender, EventArgs e)
	{
		MainMap.Zoom = (double)trackBar1.Value / 100.0;
	}

	private void button8_Click(object sender, EventArgs e)
	{
		if (!TryCoordinate(textBoxLat.Text, out double lat) || !TryCoordinate(textBoxLng.Text, out double lng) || lat < -90 || lat > 90 || lng < -180 || lng > 180)
		{
			MessageBox.Show(this, "请输入有效坐标：纬度 −90～90，经度 −180～180。", "定位地图", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		MainMap.Position = new PointLatLng(lat, lng);
	}
	private void textBoxGeo_KeyPress(object sender, KeyPressEventArgs e)
	{
		if (e.KeyChar == '\r')
		{
			GeoCoderStatusCode geoCoderStatusCode = MainMap.SetPositionByKeywords(textBoxGeo.Text);
			if (geoCoderStatusCode != GeoCoderStatusCode.OK)
			{
				MessageBox.Show("Geocoder can't find: '" + textBoxGeo.Text + "', reason: " + geoCoderStatusCode, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
	}

	private void button1_Click(object sender, EventArgs e)
	{
		MainMap.ReloadMap();
	}

	private void checkBoxUseCache_CheckedChanged(object sender, EventArgs e)
	{
		MainMap.Manager.UseRouteCache = checkBoxUseRouteCache.Checked;
		MainMap.Manager.UseGeocoderCache = checkBoxUseRouteCache.Checked;
		MainMap.Manager.UsePlacemarkCache = checkBoxUseRouteCache.Checked;
		MainMap.Manager.UseDirectionsCache = checkBoxUseRouteCache.Checked;
	}

	private void button2_Click(object sender, EventArgs e)
	{
		if (MessageBox.Show("Are You sure?", "Clear GMap.NET cache?", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation) == DialogResult.OK)
		{
			try
			{
				MainMap.Manager.PrimaryCache.DeleteOlderThan(DateTime.Now, null);
				MessageBox.Show("Done. Cache is clear.");
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}
	}

	private void button3_Click(object sender, EventArgs e)
	{
		RoutingProvider routingProvider = MainMap.MapProvider as RoutingProvider;
		if (routingProvider == null)
		{
			routingProvider = GMapProviders.OpenStreetMap;
		}
		MapRoute route = routingProvider.GetRoute(start, end, avoidHighways: false, walkingMode: false, checked((int)MainMap.Zoom));
		if (route != null)
		{
			GMapRoute gMapRoute = new GMapRoute(route.Points, route.Name);
			gMapRoute.IsHitTestVisible = true;
			routes.Routes.Add(gMapRoute);
			GMapMarker gMapMarker = new GMarkerGoogle(start, GMarkerGoogleType.green_big_go);
			gMapMarker.ToolTipText = "Start: " + route.Name;
			gMapMarker.ToolTipMode = MarkerTooltipMode.Always;
			GMapMarker gMapMarker2 = new GMarkerGoogle(end, GMarkerGoogleType.red_big_stop);
			gMapMarker2.ToolTipText = "End: " + end.ToString();
			gMapMarker2.ToolTipMode = MarkerTooltipMode.Always;
			objects.Markers.Add(gMapMarker);
			objects.Markers.Add(gMapMarker2);
			MainMap.ZoomAndCenterRoute(gMapRoute);
		}
	}

	private void button4_Click(object sender, EventArgs e)
	{
		GMarkerGoogle gMarkerGoogle = new GMarkerGoogle(currentMarker.Position, GMarkerGoogleType.green_pushpin);
		GMapMarkerRect gMapMarkerRect = new GMapMarkerRect(currentMarker.Position);
		gMapMarkerRect.InnerMarker = gMarkerGoogle;
		if (polygon != null)
		{
			gMapMarkerRect.Tag = polygon.Points.Count;
		}
		gMapMarkerRect.ToolTipMode = MarkerTooltipMode.Always;
		Placemark? placemark = null;
		if (checkBoxPlacemarkInfo.Checked)
		{
			Placemark? placemark2 = GMapProviders.GoogleMap.GetPlacemark(currentMarker.Position, out var status);
			if (status == GeoCoderStatusCode.OK && placemark2.HasValue)
			{
				placemark = placemark2;
			}
		}
		if (placemark.HasValue)
		{
			gMapMarkerRect.ToolTipText = placemark.Value.Address;
		}
		else
		{
			gMapMarkerRect.ToolTipText = currentMarker.Position.ToString();
		}
		objects.Markers.Add(gMarkerGoogle);
		objects.Markers.Add(gMapMarkerRect);
		RegeneratePolygon();
	}

	private void button6_Click(object sender, EventArgs e)
	{
		routes.Routes.Clear();
	}

	private void button15_Click(object sender, EventArgs e)
	{
		polygons.Polygons.Clear();
	}

	private void button5_Click(object sender, EventArgs e)
	{
		objects.Markers.Clear();
	}

	private void checkBoxCurrentMarker_CheckedChanged(object sender, EventArgs e)
	{
		currentMarker.IsVisible = checkBoxCurrentMarker.Checked;
	}

	private void checkBoxCanDrag_CheckedChanged(object sender, EventArgs e)
	{
		MainMap.CanDragMap = checkBoxCanDrag.Checked;
	}

	private void buttonSetStart_Click(object sender, EventArgs e)
	{
		start = currentMarker.Position;
	}

	private void buttonSetEnd_Click(object sender, EventArgs e)
	{
		end = currentMarker.Position;
	}

	private void button7_Click(object sender, EventArgs e)
	{
		MainMap.ZoomAndCenterMarkers("objects");
	}

	private void button9_Click(object sender, EventArgs e)
	{
		MainMap.ShowExportDialog();
	}

	private void button10_Click(object sender, EventArgs e)
	{
		MainMap.ShowImportDialog();
	}

	private void button11_Click(object sender, EventArgs e)
	{
		RectLatLng selectedArea = MainMap.SelectedArea;
		checked
		{
			if (!selectedArea.IsEmpty)
			{
				for (int i = (int)MainMap.Zoom; i <= MainMap.MaxZoom; i++)
				{
					switch (MessageBox.Show("Ready ripp at Zoom = " + i + " ?", "GMap.NET", MessageBoxButtons.YesNoCancel))
					{
					case DialogResult.Yes:
					{
						using (TilePrefetcher tilePrefetcher = new TilePrefetcher())
						{
							tilePrefetcher.Overlay = objects;
							tilePrefetcher.Shuffle = MainMap.Manager.Mode != AccessMode.CacheOnly;
							tilePrefetcher.Owner = this;
							tilePrefetcher.ShowCompleteMessage = true;
							tilePrefetcher.Start(selectedArea, i, MainMap.MapProvider, (MainMap.Manager.Mode != AccessMode.CacheOnly) ? 100 : 0, (MainMap.Manager.Mode != AccessMode.CacheOnly) ? 1 : 0);
						}
						break;
					}
					case DialogResult.Cancel:
						return;
					}
				}
			}
			else
			{
				MessageBox.Show("Select map area holding ALT", "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}
	}

	private void button12_Click(object sender, EventArgs e)
	{
		try
		{
			using SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "PNG (*.png)|*.png";
			saveFileDialog.FileName = "GMap.NET image";
			Image image = MainMap.ToImage();
			if (image == null)
			{
				return;
			}
			using (image)
			{
				if (saveFileDialog.ShowDialog() == DialogResult.OK)
				{
					image.Save(saveFileDialog.FileName);
					MessageBox.Show("Image saved: " + saveFileDialog.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("Image failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void checkBoxDebug_CheckedChanged(object sender, EventArgs e)
	{
		MainMap.ShowTileGridLines = checkBoxDebug.Checked;
	}

	private void button13_Click(object sender, EventArgs e)
	{
		foreach (Form owned in OwnedForms)
		{
			if (owned is StaticImage && !owned.IsDisposed) { owned.Activate(); return; }
		}
		StaticImage staticImage = new StaticImage(this);
		staticImage.Owner = this;
		staticImage.Show();
	}

	private void button14_Click(object sender, EventArgs e)
	{
		using FileDialog fileDialog = new OpenFileDialog();
		fileDialog.CheckPathExists = true;
		fileDialog.CheckFileExists = false;
		fileDialog.AddExtension = true;
		fileDialog.DefaultExt = "gpsd";
		fileDialog.ValidateNames = true;
		fileDialog.Title = "GMap.NET: open gps log generated in your windows mobile";
		fileDialog.Filter = "GMap.NET gps log DB files (*.gpsd)|*.gpsd";
		fileDialog.FilterIndex = 1;
		fileDialog.RestoreDirectory = true;
		if (fileDialog.ShowDialog() == DialogResult.OK)
		{
			routes.Routes.Clear();
			mobileGpsLog = fileDialog.FileName;
			AddGpsMobileLogRoutes(fileDialog.FileName);
			if (routes.Routes.Count > 0)
			{
				MainMap.ZoomAndCenterRoutes(null);
			}
		}
	}

	private void MainForm_KeyUp(object sender, KeyEventArgs e)
	{
		if (!MainMap.Focused) return;
		int num = -22;
		checked
		{
			if (e.KeyCode == Keys.Left)
			{
				MainMap.Offset(-num, 0);
			}
			else if (e.KeyCode == Keys.Right)
			{
				MainMap.Offset(num, 0);
			}
			else if (e.KeyCode == Keys.Up)
			{
				MainMap.Offset(0, -num);
			}
			else if (e.KeyCode == Keys.Down)
			{
				MainMap.Offset(0, num);
			}
			else if (e.KeyCode == Keys.Delete)
			{
				if (currentPolygon != null)
				{
					polygons.Polygons.Remove(currentPolygon);
					currentPolygon = null;
				}
				if (currentRoute != null)
				{
					routes.Routes.Remove(currentRoute);
					currentRoute = null;
				}
				if (CurentRectMarker != null)
				{
					objects.Markers.Remove(CurentRectMarker);
					if (CurentRectMarker.InnerMarker != null)
					{
						objects.Markers.Remove(CurentRectMarker.InnerMarker);
					}
					CurentRectMarker = null;
					RegeneratePolygon();
				}
			}
			else if (e.KeyCode == Keys.Escape)
			{
				MainMap.Bearing = 0f;
				if (currentTransport != null && !MainMap.IsMouseOverMarker)
				{
					currentTransport.ToolTipMode = MarkerTooltipMode.OnMouseOver;
					currentTransport = null;
				}
			}
		}
	}

	private void MainForm_KeyPress(object sender, KeyPressEventArgs e)
	{
		checked
		{
			if (MainMap.Focused)
			{
				if (e.KeyChar == '+')
				{
					MainMap.Zoom = (int)MainMap.Zoom + 1;
				}
				else if (e.KeyChar == '-')
				{
					MainMap.Zoom = (int)(MainMap.Zoom + 0.99) - 1;
				}
				else if (e.KeyChar == 'a')
				{
					MainMap.Bearing--;
				}
				else if (e.KeyChar == 'z')
				{
					MainMap.Bearing++;
				}
			}
		}
	}

	private void buttonZoomUp_Click(object sender, EventArgs e)
	{
		MainMap.Zoom = checked((int)MainMap.Zoom + 1);
	}

	private void buttonZoomDown_Click(object sender, EventArgs e)
	{
		MainMap.Zoom = checked((int)(MainMap.Zoom + 0.99) - 1);
	}

	private void RealTimeChanged(object sender, EventArgs e)
	{
		objects.Markers.Clear();
		polygons.Polygons.Clear();
		routes.Routes.Clear();
		if (radioButtonPerf.Checked)
		{
			timerPerf.Interval = 44;
			timerPerf.Start();
		}
		else
		{
			timerPerf.Stop();
		}
		if (radioButtonFlight.Checked)
		{
			if (!flightWorker.IsBusy)
			{
				firstLoadFlight = true;
				flightWorker.RunWorkerAsync();
			}
		}
		else if (flightWorker.IsBusy)
		{
			flightWorker.CancelAsync();
		}
		if (radioButtonVehicle.Checked)
		{
			if (!transportWorker.IsBusy)
			{
				firstLoadTrasport = true;
				transportWorker.RunWorkerAsync();
			}
		}
		else if (transportWorker.IsBusy)
		{
			transportWorker.CancelAsync();
		}
		if (radioButtonTcpIp.Checked)
		{
			GridConnections.Visible = true;
			checkBoxTcpIpSnap.Visible = true;
			checkBoxTraceRoute.Visible = true;
			GridConnections.Refresh();
			if (!connectionsWorker.IsBusy)
			{
				MainMap.Zoom = 5.0;
				connectionsWorker.RunWorkerAsync();
			}
			return;
		}
		CountryStatusView.Clear();
		GridConnections.Visible = false;
		checkBoxTcpIpSnap.Visible = false;
		checkBoxTraceRoute.Visible = false;
		if (connectionsWorker.IsBusy)
		{
			connectionsWorker.CancelAsync();
		}
		if (ipInfoSearchWorker.IsBusy)
		{
			ipInfoSearchWorker.CancelAsync();
		}
		if (iptracerWorker.IsBusy)
		{
			iptracerWorker.CancelAsync();
		}
	}

	private void buttonExportToGpx_Click(object sender, EventArgs e)
	{
		try
		{
			using SaveFileDialog saveFileDialog = new SaveFileDialog();
			saveFileDialog.Filter = "GPX (*.gpx)|*.gpx";
			saveFileDialog.FileName = "mobile gps log";
			DateTime? dateTime = null;
			DateTime? dateTime2 = null;
			if (MobileLogFrom.Checked)
			{
				dateTime = MobileLogFrom.Value.ToUniversalTime();
				saveFileDialog.FileName = saveFileDialog.FileName + " from " + MobileLogFrom.Value.ToString("yyyy-MM-dd HH-mm");
			}
			if (MobileLogTo.Checked)
			{
				dateTime2 = MobileLogTo.Value.ToUniversalTime();
				saveFileDialog.FileName = saveFileDialog.FileName + " to " + MobileLogTo.Value.ToString("yyyy-MM-dd HH-mm");
			}
			if (saveFileDialog.ShowDialog() == DialogResult.OK)
			{
				IEnumerable<List<GpsLog>> routesFromMobileLog = Stuff.GetRoutesFromMobileLog(mobileGpsLog, dateTime, dateTime2, 3.3);
				if (routesFromMobileLog != null && MainMap.Manager.ExportGPX(routesFromMobileLog, saveFileDialog.FileName))
				{
					MessageBox.Show("GPX saved: " + saveFileDialog.FileName, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
				}
			}
		}
		catch (Exception ex)
		{
			MessageBox.Show("GPX failed to save: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	private void button16_Click(object sender, EventArgs e)
	{
		using FileDialog fileDialog = new OpenFileDialog();
		fileDialog.CheckPathExists = true;
		fileDialog.CheckFileExists = false;
		fileDialog.AddExtension = true;
		fileDialog.DefaultExt = "gpx";
		fileDialog.ValidateNames = true;
		fileDialog.Title = "GMap.NET: open gpx log";
		fileDialog.Filter = "gpx files (*.gpx)|*.gpx";
		fileDialog.FilterIndex = 1;
		fileDialog.RestoreDirectory = true;
		if (fileDialog.ShowDialog() != DialogResult.OK)
		{
			return;
		}
		try
		{
			string objectXml = File.ReadAllText(fileDialog.FileName);
			gpxType gpxType2 = MainMap.Manager.DeserializeGPX(objectXml);
			if (gpxType2 == null || gpxType2.trk.Length == 0)
			{
				return;
			}
			trkType[] trk = gpxType2.trk;
			foreach (trkType obj in trk)
			{
				List<PointLatLng> list = new List<PointLatLng>();
				trksegType[] trkseg = obj.trkseg;
				for (int j = 0; j < trkseg.Length; j++)
				{
					wptType[] trkpt = trkseg[j].trkpt;
					foreach (wptType wptType2 in trkpt)
					{
						list.Add(new PointLatLng((double)wptType2.lat, (double)wptType2.lon));
					}
				}
				GMapRoute gMapRoute = new GMapRoute(list, string.Empty);
				gMapRoute.Stroke = new Pen(Color.FromArgb(144, Color.Red));
				gMapRoute.Stroke.Width = 5f;
				gMapRoute.Stroke.DashStyle = DashStyle.DashDot;
				routes.Routes.Add(gMapRoute);
			}
			MainMap.ZoomAndCenterRoutes(null);
		}
		catch (Exception ex)
		{
			MessageBox.Show("Error importing gpx: " + ex.Message, "GMap.NET", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
		}
	}

	private void checkBoxTraceRoute_CheckedChanged(object sender, EventArgs e)
	{
		TryTraceConnection = checkBoxTraceRoute.Checked;
		if (!TryTraceConnection)
		{
			if (iptracerWorker.IsBusy)
			{
				iptracerWorker.CancelAsync();
			}
			routes.Routes.Clear();
		}
	}

	private void GridConnections_DoubleClick(object sender, EventArgs e)
	{
		GridConnections.ClearSelection();
	}

	private void button17_Click(object sender, EventArgs e)
	{
		try
		{
			Process.Start(new ProcessStartInfo { FileName = MainMap.CacheLocation, UseShellExecute = true });
		}
		catch (Exception error)
		{
			MessageBox.Show(this, "无法打开缓存文件夹：" + error.Message, "本地缓存", MessageBoxButtons.OK, MessageBoxIcon.Error);
		}
	}
	private void checkBoxTileHost_CheckedChanged(object sender, EventArgs e)
	{
		if (checkBoxTileHost.Checked)
		{
			try
			{
				MainMap.Manager.EnableTileHost(8844);
				TryExtractLeafletjs();
				return;
			}
			catch (Exception ex)
			{
				MessageBox.Show("EnableTileHost: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				return;
			}
		}
		MainMap.Manager.DisableTileHost();
	}

	private void textBoxLat_TextChanged(object sender, EventArgs e)
	{
		if (sender is Control input && !input.Focused) return;
		textBoxLat.TextChanged -= textBoxLat_TextChanged;
		try
		{
			double d = Stuff.ConvertDegreeAngleToDouble(textBoxLatDegree, textBoxLatMinute, textBoxLatSecond, isLatitude: true);
			textBoxLat.Text = (double.IsNaN(d) ? string.Empty : d.ToString(CultureInfo.InvariantCulture));
		}
		finally
		{
			textBoxLat.TextChanged += textBoxLat_TextChanged;
		}
	}

	private void textBoxLng_TextChanged(object sender, EventArgs e)
	{
		if (sender is Control input && !input.Focused) return;
		textBoxLng.TextChanged -= textBoxLng_TextChanged;
		try
		{
			double d = Stuff.ConvertDegreeAngleToDouble(textBoxLngDegree, textBoxLngMinute, textBoxLngSecond, isLatitude: false);
			textBoxLng.Text = (double.IsNaN(d) ? string.Empty : d.ToString(CultureInfo.InvariantCulture));
		}
		finally
		{
			textBoxLng.TextChanged += textBoxLng_TextChanged;
		}
	}

	private void buttonDraw_Click(object sender, EventArgs e)
	{
		double north = Stuff.ConvertDegreeAngleToDouble(textBoxTLLatDegree, textBoxTLLatMinute, textBoxTLLatSecond, true);
		double west = Stuff.ConvertDegreeAngleToDouble(textBoxTLLngDegree, textBoxTLLngMinute, textBoxTLLngSecond, false);
		double south = Stuff.ConvertDegreeAngleToDouble(textBoxBRLatDegree, textBoxBRLatMinute, textBoxBRLatSecond, true);
		double east = Stuff.ConvertDegreeAngleToDouble(textBoxBRLngDegree, textBoxBRLngMinute, textBoxBRLngSecond, false);
		if (!TryCreateSelection(north.ToString(CultureInfo.InvariantCulture), west.ToString(CultureInfo.InvariantCulture), south.ToString(CultureInfo.InvariantCulture), east.ToString(CultureInfo.InvariantCulture), out var area, out string error))
		{
			MessageBox.Show(this, error, "度分秒选区", MessageBoxButtons.OK, MessageBoxIcon.Information);
			return;
		}
		SetWorkspaceSelection(area);
		UpdateWorkspaceStatus();
	}
	protected override void Dispose(bool disposing)
	{
		if (disposing && components != null)
		{
			components.Dispose();
		}
		if (disposing) panel4?.Dispose();
		base.Dispose(disposing);
	}

	private void InitializeComponent()
	{
		System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle = new System.Windows.Forms.DataGridViewCellStyle();
		this.panel2 = new System.Windows.Forms.Panel();
		this.MainMap = new Demo.WindowsForms.Map();
		this.panel4 = new System.Windows.Forms.Panel();
		this.splitter1 = new BSE.Windows.Forms.Splitter();
		this.panelMenu = new BSE.Windows.Forms.Panel();
		this.xPanderPanelList1 = new BSE.Windows.Forms.XPanderPanelList();
		this.xPanderPanelMain = new BSE.Windows.Forms.XPanderPanel();
		this.tableLayoutPanel4 = new System.Windows.Forms.TableLayoutPanel();
		this.tableLayoutPanel5 = new System.Windows.Forms.TableLayoutPanel();
		this.groupBox8 = new System.Windows.Forms.GroupBox();
		this.button7 = new System.Windows.Forms.Button();
		this.checkBoxPlacemarkInfo = new System.Windows.Forms.CheckBox();
		this.button5 = new System.Windows.Forms.Button();
		this.button4 = new System.Windows.Forms.Button();
		this.groupBox7 = new System.Windows.Forms.GroupBox();
		this.buttonExportToGpx = new System.Windows.Forms.Button();
		this.MobileLogTo = new System.Windows.Forms.DateTimePicker();
		this.label3 = new System.Windows.Forms.Label();
		this.button15 = new System.Windows.Forms.Button();
		this.MobileLogFrom = new System.Windows.Forms.DateTimePicker();
		this.button14 = new System.Windows.Forms.Button();
		this.buttonSetEnd = new System.Windows.Forms.Button();
		this.buttonSetStart = new System.Windows.Forms.Button();
		this.button6 = new System.Windows.Forms.Button();
		this.button3 = new System.Windows.Forms.Button();
		this.groupBox5 = new System.Windows.Forms.GroupBox();
		this.button16 = new System.Windows.Forms.Button();
		this.button13 = new System.Windows.Forms.Button();
		this.checkBoxDebug = new System.Windows.Forms.CheckBox();
		this.button12 = new System.Windows.Forms.Button();
		this.label8 = new System.Windows.Forms.Label();
		this.comboBoxMode = new System.Windows.Forms.ComboBox();
		this.checkBoxCanDrag = new System.Windows.Forms.CheckBox();
		this.checkBoxCurrentMarker = new System.Windows.Forms.CheckBox();
		this.label7 = new System.Windows.Forms.Label();
		this.comboBoxMapType = new System.Windows.Forms.ComboBox();
		this.groupBox3 = new System.Windows.Forms.GroupBox();
		this.buttonDraw = new System.Windows.Forms.Button();
		this.label20 = new System.Windows.Forms.Label();
		this.label21 = new System.Windows.Forms.Label();
		this.textBoxBRLngSecond = new System.Windows.Forms.TextBox();
		this.textBoxBRLngMinute = new System.Windows.Forms.TextBox();
		this.textBoxBRLngDegree = new System.Windows.Forms.TextBox();
		this.textBoxBRLatSecond = new System.Windows.Forms.TextBox();
		this.textBoxBRLatMinute = new System.Windows.Forms.TextBox();
		this.textBoxBRLatDegree = new System.Windows.Forms.TextBox();
		this.label22 = new System.Windows.Forms.Label();
		this.label18 = new System.Windows.Forms.Label();
		this.label19 = new System.Windows.Forms.Label();
		this.textBoxTLLngSecond = new System.Windows.Forms.TextBox();
		this.textBoxTLLngMinute = new System.Windows.Forms.TextBox();
		this.textBoxTLLngDegree = new System.Windows.Forms.TextBox();
		this.textBoxTLLatSecond = new System.Windows.Forms.TextBox();
		this.textBoxTLLatMinute = new System.Windows.Forms.TextBox();
		this.textBoxTLLatDegree = new System.Windows.Forms.TextBox();
		this.label17 = new System.Windows.Forms.Label();
		this.label16 = new System.Windows.Forms.Label();
		this.label15 = new System.Windows.Forms.Label();
		this.textBoxLngSecond = new System.Windows.Forms.TextBox();
		this.textBoxLngMinute = new System.Windows.Forms.TextBox();
		this.textBoxLngDegree = new System.Windows.Forms.TextBox();
		this.textBoxLatSecond = new System.Windows.Forms.TextBox();
		this.textBoxLatMinute = new System.Windows.Forms.TextBox();
		this.textBoxLatDegree = new System.Windows.Forms.TextBox();
		this.label6 = new System.Windows.Forms.Label();
		this.textBoxGeo = new System.Windows.Forms.TextBox();
		this.button1 = new System.Windows.Forms.Button();
		this.button8 = new System.Windows.Forms.Button();
		this.label2 = new System.Windows.Forms.Label();
		this.label1 = new System.Windows.Forms.Label();
		this.textBoxLng = new System.Windows.Forms.TextBox();
		this.textBoxLat = new System.Windows.Forms.TextBox();
		this.tableLayoutPanel6 = new System.Windows.Forms.TableLayoutPanel();
		this.buttonZoomUp = new System.Windows.Forms.Button();
		this.trackBar1 = new System.Windows.Forms.TrackBar();
		this.buttonZoomDown = new System.Windows.Forms.Button();
		this.xPanderPanelCache = new BSE.Windows.Forms.XPanderPanel();
		this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
		this.textBoxCacheSize = new System.Windows.Forms.TextBox();
		this.textBoxCacheStatus = new System.Windows.Forms.TextBox();
		this.button10 = new System.Windows.Forms.Button();
		this.textBoxMemory = new System.Windows.Forms.TextBox();
		this.checkBoxUseRouteCache = new System.Windows.Forms.CheckBox();
		this.button9 = new System.Windows.Forms.Button();
		this.button11 = new System.Windows.Forms.Button();
		this.label10 = new System.Windows.Forms.Label();
		this.button2 = new System.Windows.Forms.Button();
		this.label13 = new System.Windows.Forms.Label();
		this.label14 = new System.Windows.Forms.Label();
		this.button17 = new System.Windows.Forms.Button();
		this.checkBoxTileHost = new System.Windows.Forms.CheckBox();
		this.xPanderPanelInfo = new BSE.Windows.Forms.XPanderPanel();
		this.tableLayoutPanel2 = new System.Windows.Forms.TableLayoutPanel();
		this.textBoxZoomCurrent = new System.Windows.Forms.TextBox();
		this.textBoxrouteCount = new System.Windows.Forms.TextBox();
		this.label12 = new System.Windows.Forms.Label();
		this.label9 = new System.Windows.Forms.Label();
		this.textBoxLngCurrent = new System.Windows.Forms.TextBox();
		this.textBoxMarkerCount = new System.Windows.Forms.TextBox();
		this.label11 = new System.Windows.Forms.Label();
		this.label4 = new System.Windows.Forms.Label();
		this.textBoxLatCurrent = new System.Windows.Forms.TextBox();
		this.label5 = new System.Windows.Forms.Label();
		this.xPanderPanelLive = new BSE.Windows.Forms.XPanderPanel();
		this.tableLayoutPanel3 = new System.Windows.Forms.TableLayoutPanel();
		this.radioButtonNone = new System.Windows.Forms.RadioButton();
		this.checkBoxTcpIpSnap = new System.Windows.Forms.CheckBox();
		this.GridConnections = new System.Windows.Forms.DataGridView();
		this.CountryName = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.ConnectionsCount = new System.Windows.Forms.DataGridViewTextBoxColumn();
		this.radioButtonFlight = new System.Windows.Forms.RadioButton();
		this.radioButtonPerf = new System.Windows.Forms.RadioButton();
		this.radioButtonTcpIp = new System.Windows.Forms.RadioButton();
		this.checkBoxTraceRoute = new System.Windows.Forms.CheckBox();
		this.radioButtonVehicle = new System.Windows.Forms.RadioButton();
		this.panel2.SuspendLayout();
		this.panel4.SuspendLayout();
		this.panelMenu.SuspendLayout();
		this.xPanderPanelList1.SuspendLayout();
		this.xPanderPanelMain.SuspendLayout();
		this.tableLayoutPanel4.SuspendLayout();
		this.tableLayoutPanel5.SuspendLayout();
		this.groupBox8.SuspendLayout();
		this.groupBox7.SuspendLayout();
		this.groupBox5.SuspendLayout();
		this.groupBox3.SuspendLayout();
		this.tableLayoutPanel6.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).BeginInit();
		this.xPanderPanelCache.SuspendLayout();
		this.tableLayoutPanel1.SuspendLayout();
		this.xPanderPanelInfo.SuspendLayout();
		this.tableLayoutPanel2.SuspendLayout();
		this.xPanderPanelLive.SuspendLayout();
		this.tableLayoutPanel3.SuspendLayout();
		((System.ComponentModel.ISupportInitialize)this.GridConnections).BeginInit();
		base.SuspendLayout();
		this.panel2.Controls.Add(this.MainMap);
		this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel2.Location = new System.Drawing.Point(0, 0);
		this.panel2.Margin = new System.Windows.Forms.Padding(2);
		this.panel2.Name = "panel2";
		this.panel2.Size = new System.Drawing.Size(661, 821);
		this.panel2.TabIndex = 41;
		this.MainMap.Bearing = 0f;
		this.MainMap.CanDragMap = true;
		this.MainMap.Dock = System.Windows.Forms.DockStyle.Fill;
		this.MainMap.EmptyTileColor = System.Drawing.Color.Navy;
		this.MainMap.GrayScaleMode = false;
		this.MainMap.HelperLineOption = GMap.NET.WindowsForms.HelperLineOptions.DontShow;
		this.MainMap.LevelsKeepInMemory = 5;
		this.MainMap.Location = new System.Drawing.Point(0, 0);
		this.MainMap.MarkersEnabled = true;
		this.MainMap.MaxZoom = 17;
		this.MainMap.MinZoom = 2;
		this.MainMap.MouseWheelZoomEnabled = true;
		this.MainMap.MouseWheelZoomType = GMap.NET.MouseWheelZoomType.MousePositionAndCenter;
		this.MainMap.Name = "MainMap";
		this.MainMap.NegativeMode = false;
		this.MainMap.PolygonsEnabled = true;
		this.MainMap.RetryLoadTile = 0;
		this.MainMap.RoutesEnabled = true;
		this.MainMap.ScaleMode = GMap.NET.WindowsForms.ScaleModes.Integer;
		this.MainMap.SelectedAreaFillColor = System.Drawing.Color.FromArgb(33, 65, 105, 225);
		this.MainMap.ShowTileGridLines = false;
		this.MainMap.Size = new System.Drawing.Size(661, 821);
		this.MainMap.TabIndex = 0;
		this.MainMap.Zoom = 0.0;
		this.panel4.Controls.Add(this.panel2);
		this.panel4.Controls.Add(this.splitter1);
		this.panel4.Controls.Add(this.panelMenu);
		this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
		this.panel4.Location = new System.Drawing.Point(0, 0);
		this.panel4.Margin = new System.Windows.Forms.Padding(2);
		this.panel4.Name = "panel4";
		this.panel4.Size = new System.Drawing.Size(891, 821);
		this.panel4.TabIndex = 44;
		this.splitter1.BackColor = System.Drawing.Color.Transparent;
		this.splitter1.Dock = System.Windows.Forms.DockStyle.Right;
		this.splitter1.Enabled = false;
		this.splitter1.Location = new System.Drawing.Point(661, 0);
		this.splitter1.Margin = new System.Windows.Forms.Padding(2);
		this.splitter1.MinExtra = 390;
		this.splitter1.MinSize = 390;
		this.splitter1.Name = "splitter1";
		this.splitter1.Size = new System.Drawing.Size(2, 821);
		this.splitter1.TabIndex = 42;
		this.splitter1.TabStop = false;
		this.panelMenu.AssociatedSplitter = this.splitter1;
		this.panelMenu.BackColor = System.Drawing.Color.Transparent;
		this.panelMenu.CaptionFont = new System.Drawing.Font("Segoe UI", 9.75f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.panelMenu.CaptionHeight = 27;
		this.panelMenu.Controls.Add(this.xPanderPanelList1);
		this.panelMenu.CustomColors.BorderColor = System.Drawing.Color.FromArgb(184, 184, 184);
		this.panelMenu.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
		this.panelMenu.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
		this.panelMenu.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(252, 252, 252);
		this.panelMenu.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
		this.panelMenu.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(248, 248, 248);
		this.panelMenu.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(179, 255, 255);
		this.panelMenu.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(179, 255, 255);
		this.panelMenu.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
		this.panelMenu.CustomColors.CollapsedCaptionText = System.Drawing.SystemColors.ControlText;
		this.panelMenu.CustomColors.ContentGradientBegin = System.Drawing.SystemColors.ButtonFace;
		this.panelMenu.CustomColors.ContentGradientEnd = System.Drawing.Color.FromArgb(252, 252, 252);
		this.panelMenu.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
		this.panelMenu.Dock = System.Windows.Forms.DockStyle.Right;
		this.panelMenu.ForeColor = System.Drawing.SystemColors.ControlText;
		this.panelMenu.Image = null;
		this.panelMenu.Location = new System.Drawing.Point(663, 0);
		this.panelMenu.Margin = new System.Windows.Forms.Padding(2);
		this.panelMenu.MinimumSize = new System.Drawing.Size(27, 27);
		this.panelMenu.Name = "panelMenu";
		this.panelMenu.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.panelMenu.ShowExpandIcon = true;
		this.panelMenu.Size = new System.Drawing.Size(228, 821);
		this.panelMenu.TabIndex = 40;
		this.panelMenu.Text = "Menu";
		this.panelMenu.ToolTipTextCloseIcon = null;
		this.panelMenu.ToolTipTextExpandIconPanelCollapsed = "maximize";
		this.panelMenu.ToolTipTextExpandIconPanelExpanded = "minimize";
		this.xPanderPanelList1.CaptionStyle = BSE.Windows.Forms.CaptionStyle.Flat;
		this.xPanderPanelList1.Controls.Add(this.xPanderPanelMain);
		this.xPanderPanelList1.Controls.Add(this.xPanderPanelCache);
		this.xPanderPanelList1.Controls.Add(this.xPanderPanelInfo);
		this.xPanderPanelList1.Controls.Add(this.xPanderPanelLive);
		this.xPanderPanelList1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.xPanderPanelList1.GradientBackground = System.Drawing.Color.Empty;
		this.xPanderPanelList1.Location = new System.Drawing.Point(0, 28);
		this.xPanderPanelList1.Margin = new System.Windows.Forms.Padding(2);
		this.xPanderPanelList1.Name = "xPanderPanelList1";
		this.xPanderPanelList1.PanelColors = null;
		this.xPanderPanelList1.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.xPanderPanelList1.ShowExpandIcon = true;
		this.xPanderPanelList1.Size = new System.Drawing.Size(228, 792);
		this.xPanderPanelList1.TabIndex = 0;
		this.xPanderPanelList1.Text = "xPanderPanelList1";
		this.xPanderPanelMain.CaptionFont = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.xPanderPanelMain.Controls.Add(this.tableLayoutPanel4);
		this.xPanderPanelMain.CustomColors.BackColor = System.Drawing.SystemColors.Control;
		this.xPanderPanelMain.CustomColors.BorderColor = System.Drawing.Color.FromArgb(184, 184, 184);
		this.xPanderPanelMain.CustomColors.CaptionCheckedGradientBegin = System.Drawing.Color.Empty;
		this.xPanderPanelMain.CustomColors.CaptionCheckedGradientEnd = System.Drawing.Color.Empty;
		this.xPanderPanelMain.CustomColors.CaptionCheckedGradientMiddle = System.Drawing.Color.Empty;
		this.xPanderPanelMain.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelMain.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelMain.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelMain.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
		this.xPanderPanelMain.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelMain.CustomColors.CaptionPressedGradientBegin = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionPressedGradientEnd = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionPressedGradientMiddle = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionSelectedGradientMiddle = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelMain.CustomColors.CaptionSelectedText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelMain.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelMain.CustomColors.FlatCaptionGradientBegin = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelMain.CustomColors.FlatCaptionGradientEnd = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelMain.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
		this.xPanderPanelMain.Expand = true;
		this.xPanderPanelMain.ForeColor = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelMain.Image = null;
		this.xPanderPanelMain.IsClosable = false;
		this.xPanderPanelMain.Margin = new System.Windows.Forms.Padding(2);
		this.xPanderPanelMain.Name = "xPanderPanelMain";
		this.xPanderPanelMain.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.xPanderPanelMain.Size = new System.Drawing.Size(228, 717);
		this.xPanderPanelMain.TabIndex = 0;
		this.xPanderPanelMain.Text = "map";
		this.xPanderPanelMain.ToolTipTextCloseIcon = null;
		this.xPanderPanelMain.ToolTipTextExpandIconPanelCollapsed = null;
		this.xPanderPanelMain.ToolTipTextExpandIconPanelExpanded = null;
		this.xPanderPanelMain.Click += new System.EventHandler(this.xPanderPanel1_Click);
		this.tableLayoutPanel4.ColumnCount = 2;
		this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 48f));
		this.tableLayoutPanel4.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 49.27536f));
		this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel5, 1, 0);
		this.tableLayoutPanel4.Controls.Add(this.tableLayoutPanel6, 0, 0);
		this.tableLayoutPanel4.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel4.Location = new System.Drawing.Point(1, 25);
		this.tableLayoutPanel4.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel4.Name = "tableLayoutPanel4";
		this.tableLayoutPanel4.RowCount = 1;
		this.tableLayoutPanel4.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel4.Size = new System.Drawing.Size(226, 692);
		this.tableLayoutPanel4.TabIndex = 38;
		this.tableLayoutPanel5.ColumnCount = 1;
		this.tableLayoutPanel5.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel5.Controls.Add(this.groupBox8, 0, 3);
		this.tableLayoutPanel5.Controls.Add(this.groupBox7, 0, 2);
		this.tableLayoutPanel5.Controls.Add(this.groupBox5, 0, 1);
		this.tableLayoutPanel5.Controls.Add(this.groupBox3, 0, 0);
		this.tableLayoutPanel5.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel5.Location = new System.Drawing.Point(48, 0);
		this.tableLayoutPanel5.Margin = new System.Windows.Forms.Padding(0, 0, 2, 2);
		this.tableLayoutPanel5.Name = "tableLayoutPanel5";
		this.tableLayoutPanel5.RowCount = 5;
		this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel5.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 6f));
		this.tableLayoutPanel5.Size = new System.Drawing.Size(176, 690);
		this.tableLayoutPanel5.TabIndex = 30;
		this.groupBox8.Controls.Add(this.button7);
		this.groupBox8.Controls.Add(this.checkBoxPlacemarkInfo);
		this.groupBox8.Controls.Add(this.button5);
		this.groupBox8.Controls.Add(this.button4);
		this.groupBox8.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox8.Location = new System.Drawing.Point(3, 671);
		this.groupBox8.Name = "groupBox8";
		this.groupBox8.Size = new System.Drawing.Size(170, 74);
		this.groupBox8.TabIndex = 37;
		this.groupBox8.TabStop = false;
		this.groupBox8.Text = "markers";
		this.button7.Location = new System.Drawing.Point(6, 45);
		this.button7.Name = "button7";
		this.button7.Size = new System.Drawing.Size(82, 24);
		this.button7.TabIndex = 15;
		this.button7.Text = "Zoom Center";
		this.button7.UseVisualStyleBackColor = true;
		this.button7.Click += new System.EventHandler(this.button7_Click);
		this.checkBoxPlacemarkInfo.AutoSize = true;
		this.checkBoxPlacemarkInfo.CheckAlign = System.Drawing.ContentAlignment.TopLeft;
		this.checkBoxPlacemarkInfo.Checked = true;
		this.checkBoxPlacemarkInfo.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxPlacemarkInfo.Location = new System.Drawing.Point(94, 19);
		this.checkBoxPlacemarkInfo.Name = "checkBoxPlacemarkInfo";
		this.checkBoxPlacemarkInfo.Size = new System.Drawing.Size(72, 17);
		this.checkBoxPlacemarkInfo.TabIndex = 14;
		this.checkBoxPlacemarkInfo.Text = "place info";
		this.checkBoxPlacemarkInfo.UseVisualStyleBackColor = true;
		this.button5.Location = new System.Drawing.Point(94, 45);
		this.button5.Name = "button5";
		this.button5.Size = new System.Drawing.Size(63, 24);
		this.button5.TabIndex = 13;
		this.button5.Text = "Clear All";
		this.button5.UseVisualStyleBackColor = true;
		this.button5.Click += new System.EventHandler(this.button5_Click);
		this.button4.Location = new System.Drawing.Point(6, 15);
		this.button4.Name = "button4";
		this.button4.Size = new System.Drawing.Size(82, 24);
		this.button4.TabIndex = 12;
		this.button4.Text = "Add Marker";
		this.button4.UseVisualStyleBackColor = true;
		this.button4.Click += new System.EventHandler(this.button4_Click);
		this.groupBox7.Controls.Add(this.buttonExportToGpx);
		this.groupBox7.Controls.Add(this.MobileLogTo);
		this.groupBox7.Controls.Add(this.label3);
		this.groupBox7.Controls.Add(this.button15);
		this.groupBox7.Controls.Add(this.MobileLogFrom);
		this.groupBox7.Controls.Add(this.button14);
		this.groupBox7.Controls.Add(this.buttonSetEnd);
		this.groupBox7.Controls.Add(this.buttonSetStart);
		this.groupBox7.Controls.Add(this.button6);
		this.groupBox7.Controls.Add(this.button3);
		this.groupBox7.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox7.Location = new System.Drawing.Point(3, 510);
		this.groupBox7.Name = "groupBox7";
		this.groupBox7.Size = new System.Drawing.Size(170, 155);
		this.groupBox7.TabIndex = 35;
		this.groupBox7.TabStop = false;
		this.groupBox7.Text = "routing";
		this.buttonExportToGpx.Location = new System.Drawing.Point(124, 78);
		this.buttonExportToGpx.Margin = new System.Windows.Forms.Padding(2);
		this.buttonExportToGpx.Name = "buttonExportToGpx";
		this.buttonExportToGpx.Size = new System.Drawing.Size(37, 37);
		this.buttonExportToGpx.TabIndex = 48;
		this.buttonExportToGpx.Text = "to GPX";
		this.buttonExportToGpx.UseVisualStyleBackColor = true;
		this.buttonExportToGpx.Click += new System.EventHandler(this.buttonExportToGpx_Click);
		this.MobileLogTo.CustomFormat = "yyyy'.'MM'.'dd HH':'mm";
		this.MobileLogTo.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
		this.MobileLogTo.Location = new System.Drawing.Point(9, 99);
		this.MobileLogTo.Margin = new System.Windows.Forms.Padding(2);
		this.MobileLogTo.Name = "MobileLogTo";
		this.MobileLogTo.ShowCheckBox = true;
		this.MobileLogTo.Size = new System.Drawing.Size(110, 20);
		this.MobileLogTo.TabIndex = 47;
		this.label3.AutoSize = true;
		this.label3.Location = new System.Drawing.Point(9, 128);
		this.label3.Name = "label3";
		this.label3.Size = new System.Drawing.Size(34, 13);
		this.label3.TabIndex = 46;
		this.label3.Text = "Clear:";
		this.button15.Location = new System.Drawing.Point(104, 124);
		this.button15.Name = "button15";
		this.button15.Size = new System.Drawing.Size(56, 24);
		this.button15.TabIndex = 45;
		this.button15.Text = "Polygons";
		this.button15.UseVisualStyleBackColor = true;
		this.button15.Click += new System.EventHandler(this.button15_Click);
		this.MobileLogFrom.CustomFormat = "yyyy'.'MM'.'dd HH':'mm";
		this.MobileLogFrom.Format = System.Windows.Forms.DateTimePickerFormat.Custom;
		this.MobileLogFrom.Location = new System.Drawing.Point(9, 78);
		this.MobileLogFrom.Margin = new System.Windows.Forms.Padding(2);
		this.MobileLogFrom.Name = "MobileLogFrom";
		this.MobileLogFrom.ShowCheckBox = true;
		this.MobileLogFrom.Size = new System.Drawing.Size(110, 20);
		this.MobileLogFrom.TabIndex = 44;
		this.MobileLogFrom.Value = new System.DateTime(2010, 5, 10, 15, 41, 0, 0);
		this.button14.Location = new System.Drawing.Point(88, 49);
		this.button14.Name = "button14";
		this.button14.Size = new System.Drawing.Size(73, 24);
		this.button14.TabIndex = 43;
		this.button14.Text = "Mobile log...";
		this.button14.UseVisualStyleBackColor = true;
		this.button14.Click += new System.EventHandler(this.button14_Click);
		this.buttonSetEnd.Location = new System.Drawing.Point(88, 19);
		this.buttonSetEnd.Name = "buttonSetEnd";
		this.buttonSetEnd.Size = new System.Drawing.Size(73, 24);
		this.buttonSetEnd.TabIndex = 42;
		this.buttonSetEnd.Text = "set End";
		this.buttonSetEnd.UseVisualStyleBackColor = true;
		this.buttonSetEnd.Click += new System.EventHandler(this.buttonSetEnd_Click);
		this.buttonSetStart.Location = new System.Drawing.Point(9, 19);
		this.buttonSetStart.Name = "buttonSetStart";
		this.buttonSetStart.Size = new System.Drawing.Size(73, 24);
		this.buttonSetStart.TabIndex = 41;
		this.buttonSetStart.Text = "set Start";
		this.buttonSetStart.UseVisualStyleBackColor = true;
		this.buttonSetStart.Click += new System.EventHandler(this.buttonSetStart_Click);
		this.button6.Location = new System.Drawing.Point(45, 124);
		this.button6.Name = "button6";
		this.button6.Size = new System.Drawing.Size(55, 24);
		this.button6.TabIndex = 34;
		this.button6.Text = "Routes";
		this.button6.UseVisualStyleBackColor = true;
		this.button6.Click += new System.EventHandler(this.button6_Click);
		this.button3.Location = new System.Drawing.Point(9, 49);
		this.button3.Name = "button3";
		this.button3.Size = new System.Drawing.Size(73, 24);
		this.button3.TabIndex = 33;
		this.button3.Text = "Add Route";
		this.button3.UseVisualStyleBackColor = true;
		this.button3.Click += new System.EventHandler(this.button3_Click);
		this.groupBox5.Controls.Add(this.button16);
		this.groupBox5.Controls.Add(this.button13);
		this.groupBox5.Controls.Add(this.checkBoxDebug);
		this.groupBox5.Controls.Add(this.button12);
		this.groupBox5.Controls.Add(this.label8);
		this.groupBox5.Controls.Add(this.comboBoxMode);
		this.groupBox5.Controls.Add(this.checkBoxCanDrag);
		this.groupBox5.Controls.Add(this.checkBoxCurrentMarker);
		this.groupBox5.Controls.Add(this.label7);
		this.groupBox5.Controls.Add(this.comboBoxMapType);
		this.groupBox5.Dock = System.Windows.Forms.DockStyle.Fill;
		this.groupBox5.Location = new System.Drawing.Point(3, 358);
		this.groupBox5.Name = "groupBox5";
		this.groupBox5.Size = new System.Drawing.Size(170, 146);
		this.groupBox5.TabIndex = 31;
		this.groupBox5.TabStop = false;
		this.groupBox5.Text = "gmap";
		this.button16.Location = new System.Drawing.Point(92, 92);
		this.button16.Name = "button16";
		this.button16.Size = new System.Drawing.Size(69, 24);
		this.button16.TabIndex = 49;
		this.button16.Text = "GPX...";
		this.button16.UseVisualStyleBackColor = true;
		this.button16.Click += new System.EventHandler(this.button16_Click);
		this.button13.Location = new System.Drawing.Point(9, 117);
		this.button13.Margin = new System.Windows.Forms.Padding(2);
		this.button13.Name = "button13";
		this.button13.Size = new System.Drawing.Size(73, 24);
		this.button13.TabIndex = 41;
		this.button13.Text = "Get Static";
		this.button13.UseVisualStyleBackColor = true;
		this.button13.Click += new System.EventHandler(this.button13_Click);
		this.checkBoxDebug.AutoSize = true;
		this.checkBoxDebug.Location = new System.Drawing.Point(104, 74);
		this.checkBoxDebug.Margin = new System.Windows.Forms.Padding(2);
		this.checkBoxDebug.Name = "checkBoxDebug";
		this.checkBoxDebug.Size = new System.Drawing.Size(45, 17);
		this.checkBoxDebug.TabIndex = 40;
		this.checkBoxDebug.Text = "Grid";
		this.checkBoxDebug.UseVisualStyleBackColor = true;
		this.checkBoxDebug.CheckedChanged += new System.EventHandler(this.checkBoxDebug_CheckedChanged);
		this.button12.Location = new System.Drawing.Point(92, 117);
		this.button12.Margin = new System.Windows.Forms.Padding(2);
		this.button12.Name = "button12";
		this.button12.Size = new System.Drawing.Size(69, 24);
		this.button12.TabIndex = 39;
		this.button12.Text = "Save View";
		this.button12.UseVisualStyleBackColor = true;
		this.button12.Click += new System.EventHandler(this.button12_Click);
		this.label8.AutoSize = true;
		this.label8.Location = new System.Drawing.Point(132, 49);
		this.label8.Name = "label8";
		this.label8.Size = new System.Drawing.Size(33, 13);
		this.label8.TabIndex = 38;
		this.label8.Text = "mode";
		this.comboBoxMode.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxMode.FormattingEnabled = true;
		this.comboBoxMode.Location = new System.Drawing.Point(8, 46);
		this.comboBoxMode.Name = "comboBoxMode";
		this.comboBoxMode.Size = new System.Drawing.Size(123, 21);
		this.comboBoxMode.TabIndex = 37;
		this.comboBoxMode.DropDownClosed += new System.EventHandler(this.comboBoxMode_DropDownClosed);
		this.checkBoxCanDrag.AutoSize = true;
		this.checkBoxCanDrag.Checked = true;
		this.checkBoxCanDrag.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxCanDrag.Location = new System.Drawing.Point(9, 95);
		this.checkBoxCanDrag.Name = "checkBoxCanDrag";
		this.checkBoxCanDrag.Size = new System.Drawing.Size(73, 17);
		this.checkBoxCanDrag.TabIndex = 36;
		this.checkBoxCanDrag.Text = "Drag Map";
		this.checkBoxCanDrag.UseVisualStyleBackColor = true;
		this.checkBoxCanDrag.CheckedChanged += new System.EventHandler(this.checkBoxCanDrag_CheckedChanged);
		this.checkBoxCurrentMarker.AutoSize = true;
		this.checkBoxCurrentMarker.Location = new System.Drawing.Point(9, 73);
		this.checkBoxCurrentMarker.Name = "checkBoxCurrentMarker";
		this.checkBoxCurrentMarker.Size = new System.Drawing.Size(96, 17);
		this.checkBoxCurrentMarker.TabIndex = 35;
		this.checkBoxCurrentMarker.Text = "Current Marker";
		this.checkBoxCurrentMarker.UseVisualStyleBackColor = true;
		this.checkBoxCurrentMarker.CheckedChanged += new System.EventHandler(this.checkBoxCurrentMarker_CheckedChanged);
		this.label7.AutoSize = true;
		this.label7.Location = new System.Drawing.Point(132, 22);
		this.label7.Name = "label7";
		this.label7.Size = new System.Drawing.Size(27, 13);
		this.label7.TabIndex = 31;
		this.label7.Text = "type";
		this.comboBoxMapType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
		this.comboBoxMapType.FormattingEnabled = true;
		this.comboBoxMapType.Location = new System.Drawing.Point(8, 19);
		this.comboBoxMapType.Name = "comboBoxMapType";
		this.comboBoxMapType.Size = new System.Drawing.Size(123, 21);
		this.comboBoxMapType.TabIndex = 9;
		this.comboBoxMapType.DropDownClosed += new System.EventHandler(this.comboBoxMapType_DropDownClosed);
		this.groupBox3.Controls.Add(this.buttonDraw);
		this.groupBox3.Controls.Add(this.label20);
		this.groupBox3.Controls.Add(this.label21);
		this.groupBox3.Controls.Add(this.textBoxBRLngSecond);
		this.groupBox3.Controls.Add(this.textBoxBRLngMinute);
		this.groupBox3.Controls.Add(this.textBoxBRLngDegree);
		this.groupBox3.Controls.Add(this.textBoxBRLatSecond);
		this.groupBox3.Controls.Add(this.textBoxBRLatMinute);
		this.groupBox3.Controls.Add(this.textBoxBRLatDegree);
		this.groupBox3.Controls.Add(this.label22);
		this.groupBox3.Controls.Add(this.label18);
		this.groupBox3.Controls.Add(this.label19);
		this.groupBox3.Controls.Add(this.textBoxTLLngSecond);
		this.groupBox3.Controls.Add(this.textBoxTLLngMinute);
		this.groupBox3.Controls.Add(this.textBoxTLLngDegree);
		this.groupBox3.Controls.Add(this.textBoxTLLatSecond);
		this.groupBox3.Controls.Add(this.textBoxTLLatMinute);
		this.groupBox3.Controls.Add(this.textBoxTLLatDegree);
		this.groupBox3.Controls.Add(this.label17);
		this.groupBox3.Controls.Add(this.label16);
		this.groupBox3.Controls.Add(this.label15);
		this.groupBox3.Controls.Add(this.textBoxLngSecond);
		this.groupBox3.Controls.Add(this.textBoxLngMinute);
		this.groupBox3.Controls.Add(this.textBoxLngDegree);
		this.groupBox3.Controls.Add(this.textBoxLatSecond);
		this.groupBox3.Controls.Add(this.textBoxLatMinute);
		this.groupBox3.Controls.Add(this.textBoxLatDegree);
		this.groupBox3.Controls.Add(this.label6);
		this.groupBox3.Controls.Add(this.textBoxGeo);
		this.groupBox3.Controls.Add(this.button1);
		this.groupBox3.Controls.Add(this.button8);
		this.groupBox3.Controls.Add(this.label2);
		this.groupBox3.Controls.Add(this.label1);
		this.groupBox3.Controls.Add(this.textBoxLng);
		this.groupBox3.Controls.Add(this.textBoxLat);
		this.groupBox3.Dock = System.Windows.Forms.DockStyle.Top;
		this.groupBox3.Location = new System.Drawing.Point(3, 3);
		this.groupBox3.Name = "groupBox3";
		this.groupBox3.Size = new System.Drawing.Size(170, 349);
		this.groupBox3.TabIndex = 28;
		this.groupBox3.TabStop = false;
		this.groupBox3.Text = "coordinates";
		this.buttonDraw.Location = new System.Drawing.Point(6, 319);
		this.buttonDraw.Name = "buttonDraw";
		this.buttonDraw.Size = new System.Drawing.Size(72, 24);
		this.buttonDraw.TabIndex = 39;
		this.buttonDraw.Text = "Draw zone !";
		this.buttonDraw.UseVisualStyleBackColor = true;
		this.buttonDraw.Click += new System.EventHandler(this.buttonDraw_Click);
		this.label20.AutoSize = true;
		this.label20.Location = new System.Drawing.Point(109, 296);
		this.label20.Name = "label20";
		this.label20.Size = new System.Drawing.Size(50, 13);
		this.label20.TabIndex = 38;
		this.label20.Text = "longitude";
		this.label21.AutoSize = true;
		this.label21.Location = new System.Drawing.Point(109, 270);
		this.label21.Name = "label21";
		this.label21.Size = new System.Drawing.Size(41, 13);
		this.label21.TabIndex = 37;
		this.label21.Text = "latitude";
		this.textBoxBRLngSecond.Location = new System.Drawing.Point(78, 293);
		this.textBoxBRLngSecond.MaxLength = 2;
		this.textBoxBRLngSecond.Name = "textBoxBRLngSecond";
		this.textBoxBRLngSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxBRLngSecond.TabIndex = 36;
		this.textBoxBRLngSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxBRLngMinute.Location = new System.Drawing.Point(47, 293);
		this.textBoxBRLngMinute.MaxLength = 2;
		this.textBoxBRLngMinute.Name = "textBoxBRLngMinute";
		this.textBoxBRLngMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxBRLngMinute.TabIndex = 35;
		this.textBoxBRLngMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxBRLngDegree.Location = new System.Drawing.Point(6, 293);
		this.textBoxBRLngDegree.MaxLength = 4;
		this.textBoxBRLngDegree.Name = "textBoxBRLngDegree";
		this.textBoxBRLngDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxBRLngDegree.TabIndex = 34;
		this.textBoxBRLatSecond.Location = new System.Drawing.Point(78, 267);
		this.textBoxBRLatSecond.MaxLength = 2;
		this.textBoxBRLatSecond.Name = "textBoxBRLatSecond";
		this.textBoxBRLatSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxBRLatSecond.TabIndex = 33;
		this.textBoxBRLatSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxBRLatMinute.Location = new System.Drawing.Point(47, 267);
		this.textBoxBRLatMinute.MaxLength = 2;
		this.textBoxBRLatMinute.Name = "textBoxBRLatMinute";
		this.textBoxBRLatMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxBRLatMinute.TabIndex = 32;
		this.textBoxBRLatMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxBRLatDegree.Location = new System.Drawing.Point(6, 267);
		this.textBoxBRLatDegree.MaxLength = 4;
		this.textBoxBRLatDegree.Name = "textBoxBRLatDegree";
		this.textBoxBRLatDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxBRLatDegree.TabIndex = 31;
		this.label22.AutoSize = true;
		this.label22.Location = new System.Drawing.Point(6, 251);
		this.label22.Name = "label22";
		this.label22.Size = new System.Drawing.Size(95, 13);
		this.label22.TabIndex = 30;
		this.label22.Text = "Bottom Right Point";
		this.label18.AutoSize = true;
		this.label18.Location = new System.Drawing.Point(109, 231);
		this.label18.Name = "label18";
		this.label18.Size = new System.Drawing.Size(50, 13);
		this.label18.TabIndex = 29;
		this.label18.Text = "longitude";
		this.label19.AutoSize = true;
		this.label19.Location = new System.Drawing.Point(109, 205);
		this.label19.Name = "label19";
		this.label19.Size = new System.Drawing.Size(41, 13);
		this.label19.TabIndex = 28;
		this.label19.Text = "latitude";
		this.textBoxTLLngSecond.Location = new System.Drawing.Point(78, 228);
		this.textBoxTLLngSecond.MaxLength = 2;
		this.textBoxTLLngSecond.Name = "textBoxTLLngSecond";
		this.textBoxTLLngSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxTLLngSecond.TabIndex = 27;
		this.textBoxTLLngSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxTLLngMinute.Location = new System.Drawing.Point(47, 228);
		this.textBoxTLLngMinute.MaxLength = 2;
		this.textBoxTLLngMinute.Name = "textBoxTLLngMinute";
		this.textBoxTLLngMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxTLLngMinute.TabIndex = 26;
		this.textBoxTLLngMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxTLLngDegree.Location = new System.Drawing.Point(6, 228);
		this.textBoxTLLngDegree.MaxLength = 4;
		this.textBoxTLLngDegree.Name = "textBoxTLLngDegree";
		this.textBoxTLLngDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxTLLngDegree.TabIndex = 25;
		this.textBoxTLLatSecond.Location = new System.Drawing.Point(78, 202);
		this.textBoxTLLatSecond.MaxLength = 2;
		this.textBoxTLLatSecond.Name = "textBoxTLLatSecond";
		this.textBoxTLLatSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxTLLatSecond.TabIndex = 24;
		this.textBoxTLLatSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxTLLatMinute.Location = new System.Drawing.Point(47, 202);
		this.textBoxTLLatMinute.MaxLength = 2;
		this.textBoxTLLatMinute.Name = "textBoxTLLatMinute";
		this.textBoxTLLatMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxTLLatMinute.TabIndex = 23;
		this.textBoxTLLatMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxTLLatDegree.Location = new System.Drawing.Point(6, 202);
		this.textBoxTLLatDegree.MaxLength = 4;
		this.textBoxTLLatDegree.Name = "textBoxTLLatDegree";
		this.textBoxTLLatDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxTLLatDegree.TabIndex = 22;
		this.label17.AutoSize = true;
		this.label17.Location = new System.Drawing.Point(6, 186);
		this.label17.Name = "label17";
		this.label17.Size = new System.Drawing.Size(74, 13);
		this.label17.TabIndex = 21;
		this.label17.Text = "Top Left Point";
		this.label16.AutoSize = true;
		this.label16.Location = new System.Drawing.Point(112, 127);
		this.label16.Name = "label16";
		this.label16.Size = new System.Drawing.Size(50, 13);
		this.label16.TabIndex = 20;
		this.label16.Text = "longitude";
		this.label15.AutoSize = true;
		this.label15.Location = new System.Drawing.Point(112, 101);
		this.label15.Name = "label15";
		this.label15.Size = new System.Drawing.Size(41, 13);
		this.label15.TabIndex = 19;
		this.label15.Text = "latitude";
		this.textBoxLngSecond.Location = new System.Drawing.Point(81, 124);
		this.textBoxLngSecond.MaxLength = 2;
		this.textBoxLngSecond.Name = "textBoxLngSecond";
		this.textBoxLngSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxLngSecond.TabIndex = 17;
		this.textBoxLngSecond.Text = "91";
		this.textBoxLngSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxLngSecond.TextChanged += new System.EventHandler(this.textBoxLng_TextChanged);
		this.textBoxLngMinute.Location = new System.Drawing.Point(50, 124);
		this.textBoxLngMinute.MaxLength = 2;
		this.textBoxLngMinute.Name = "textBoxLngMinute";
		this.textBoxLngMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxLngMinute.TabIndex = 16;
		this.textBoxLngMinute.Text = "17";
		this.textBoxLngMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxLngMinute.TextChanged += new System.EventHandler(this.textBoxLng_TextChanged);
		this.textBoxLngDegree.Location = new System.Drawing.Point(9, 124);
		this.textBoxLngDegree.MaxLength = 4;
		this.textBoxLngDegree.Name = "textBoxLngDegree";
		this.textBoxLngDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxLngDegree.TabIndex = 15;
		this.textBoxLngDegree.Text = "E25";
		this.textBoxLngDegree.TextChanged += new System.EventHandler(this.textBoxLng_TextChanged);
		this.textBoxLatSecond.Location = new System.Drawing.Point(81, 98);
		this.textBoxLatSecond.MaxLength = 2;
		this.textBoxLatSecond.Name = "textBoxLatSecond";
		this.textBoxLatSecond.Size = new System.Drawing.Size(25, 20);
		this.textBoxLatSecond.TabIndex = 14;
		this.textBoxLatSecond.Text = "77";
		this.textBoxLatSecond.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxLatSecond.TextChanged += new System.EventHandler(this.textBoxLat_TextChanged);
		this.textBoxLatMinute.Location = new System.Drawing.Point(50, 98);
		this.textBoxLatMinute.MaxLength = 2;
		this.textBoxLatMinute.Name = "textBoxLatMinute";
		this.textBoxLatMinute.Size = new System.Drawing.Size(25, 20);
		this.textBoxLatMinute.TabIndex = 13;
		this.textBoxLatMinute.Text = "41";
		this.textBoxLatMinute.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
		this.textBoxLatMinute.TextChanged += new System.EventHandler(this.textBoxLat_TextChanged);
		this.textBoxLatDegree.Location = new System.Drawing.Point(9, 98);
		this.textBoxLatDegree.MaxLength = 4;
		this.textBoxLatDegree.Name = "textBoxLatDegree";
		this.textBoxLatDegree.Size = new System.Drawing.Size(35, 20);
		this.textBoxLatDegree.TabIndex = 12;
		this.textBoxLatDegree.Text = "N54";
		this.textBoxLatDegree.TextChanged += new System.EventHandler(this.textBoxLat_TextChanged);
		this.label6.AutoSize = true;
		this.label6.Location = new System.Drawing.Point(132, 74);
		this.label6.Name = "label6";
		this.label6.Size = new System.Drawing.Size(28, 13);
		this.label6.TabIndex = 11;
		this.label6.Text = "goto";
		this.textBoxGeo.Location = new System.Drawing.Point(9, 71);
		this.textBoxGeo.Name = "textBoxGeo";
		this.textBoxGeo.Size = new System.Drawing.Size(122, 20);
		this.textBoxGeo.TabIndex = 10;
		this.textBoxGeo.Text = "lietuva vilnius";
		this.textBoxGeo.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.textBoxGeo_KeyPress);
		this.button1.Location = new System.Drawing.Point(82, 150);
		this.button1.Name = "button1";
		this.button1.Size = new System.Drawing.Size(74, 24);
		this.button1.TabIndex = 9;
		this.button1.Text = "Reload";
		this.button1.UseVisualStyleBackColor = true;
		this.button1.Click += new System.EventHandler(this.button1_Click);
		this.button8.Location = new System.Drawing.Point(6, 150);
		this.button8.Name = "button8";
		this.button8.Size = new System.Drawing.Size(72, 24);
		this.button8.TabIndex = 8;
		this.button8.Text = "GoTo !";
		this.button8.UseVisualStyleBackColor = true;
		this.button8.Click += new System.EventHandler(this.button8_Click);
		this.label2.AutoSize = true;
		this.label2.Location = new System.Drawing.Point(132, 48);
		this.label2.Name = "label2";
		this.label2.Size = new System.Drawing.Size(21, 13);
		this.label2.TabIndex = 3;
		this.label2.Text = "lng";
		this.label1.AutoSize = true;
		this.label1.Location = new System.Drawing.Point(132, 22);
		this.label1.Name = "label1";
		this.label1.Size = new System.Drawing.Size(18, 13);
		this.label1.TabIndex = 2;
		this.label1.Text = "lat";
		this.textBoxLng.Location = new System.Drawing.Point(9, 45);
		this.textBoxLng.Name = "textBoxLng";
		this.textBoxLng.Size = new System.Drawing.Size(122, 20);
		this.textBoxLng.TabIndex = 1;
		this.textBoxLng.Text = "25.2985095977783";
		this.textBoxLng.TextChanged += new System.EventHandler(this.textBoxLng_TextChanged);
		this.textBoxLat.Location = new System.Drawing.Point(9, 19);
		this.textBoxLat.Name = "textBoxLat";
		this.textBoxLat.Size = new System.Drawing.Size(122, 20);
		this.textBoxLat.TabIndex = 0;
		this.textBoxLat.Text = "54.6961334816182";
		this.textBoxLat.TextChanged += new System.EventHandler(this.textBoxLat_TextChanged);
		this.tableLayoutPanel6.ColumnCount = 1;
		this.tableLayoutPanel6.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle());
		this.tableLayoutPanel6.Controls.Add(this.buttonZoomUp, 0, 0);
		this.tableLayoutPanel6.Controls.Add(this.trackBar1, 0, 1);
		this.tableLayoutPanel6.Controls.Add(this.buttonZoomDown, 0, 2);
		this.tableLayoutPanel6.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel6.Location = new System.Drawing.Point(2, 2);
		this.tableLayoutPanel6.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel6.Name = "tableLayoutPanel6";
		this.tableLayoutPanel6.RowCount = 3;
		this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel6.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel6.Size = new System.Drawing.Size(44, 688);
		this.tableLayoutPanel6.TabIndex = 31;
		this.buttonZoomUp.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonZoomUp.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonZoomUp.Location = new System.Drawing.Point(0, 0);
		this.buttonZoomUp.Margin = new System.Windows.Forms.Padding(0);
		this.buttonZoomUp.Name = "buttonZoomUp";
		this.buttonZoomUp.Size = new System.Drawing.Size(45, 27);
		this.buttonZoomUp.TabIndex = 0;
		this.buttonZoomUp.Text = "+";
		this.buttonZoomUp.UseVisualStyleBackColor = true;
		this.buttonZoomUp.Click += new System.EventHandler(this.buttonZoomUp_Click);
		this.trackBar1.BackColor = System.Drawing.Color.AliceBlue;
		this.trackBar1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.trackBar1.LargeChange = 1;
		this.trackBar1.Location = new System.Drawing.Point(0, 27);
		this.trackBar1.Margin = new System.Windows.Forms.Padding(0);
		this.trackBar1.Maximum = 1700;
		this.trackBar1.Minimum = 1;
		this.trackBar1.Name = "trackBar1";
		this.trackBar1.Orientation = System.Windows.Forms.Orientation.Vertical;
		this.trackBar1.Size = new System.Drawing.Size(45, 634);
		this.trackBar1.TabIndex = 29;
		this.trackBar1.TickFrequency = 100;
		this.trackBar1.TickStyle = System.Windows.Forms.TickStyle.TopLeft;
		this.trackBar1.Value = 12;
		this.trackBar1.ValueChanged += new System.EventHandler(this.trackBar1_ValueChanged);
		this.buttonZoomDown.Dock = System.Windows.Forms.DockStyle.Fill;
		this.buttonZoomDown.Font = new System.Drawing.Font("Microsoft Sans Serif", 12f, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, 0);
		this.buttonZoomDown.Location = new System.Drawing.Point(0, 661);
		this.buttonZoomDown.Margin = new System.Windows.Forms.Padding(0);
		this.buttonZoomDown.Name = "buttonZoomDown";
		this.buttonZoomDown.Size = new System.Drawing.Size(45, 27);
		this.buttonZoomDown.TabIndex = 1;
		this.buttonZoomDown.Text = "-";
		this.buttonZoomDown.UseVisualStyleBackColor = true;
		this.buttonZoomDown.Click += new System.EventHandler(this.buttonZoomDown_Click);
		this.xPanderPanelCache.CaptionFont = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.xPanderPanelCache.Controls.Add(this.tableLayoutPanel1);
		this.xPanderPanelCache.CustomColors.BackColor = System.Drawing.SystemColors.Control;
		this.xPanderPanelCache.CustomColors.BorderColor = System.Drawing.Color.FromArgb(184, 184, 184);
		this.xPanderPanelCache.CustomColors.CaptionCheckedGradientBegin = System.Drawing.Color.Empty;
		this.xPanderPanelCache.CustomColors.CaptionCheckedGradientEnd = System.Drawing.Color.Empty;
		this.xPanderPanelCache.CustomColors.CaptionCheckedGradientMiddle = System.Drawing.Color.Empty;
		this.xPanderPanelCache.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelCache.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelCache.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelCache.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
		this.xPanderPanelCache.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelCache.CustomColors.CaptionPressedGradientBegin = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionPressedGradientEnd = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionPressedGradientMiddle = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionSelectedGradientMiddle = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelCache.CustomColors.CaptionSelectedText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelCache.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelCache.CustomColors.FlatCaptionGradientBegin = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelCache.CustomColors.FlatCaptionGradientEnd = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelCache.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
		this.xPanderPanelCache.ForeColor = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelCache.Image = null;
		this.xPanderPanelCache.IsClosable = false;
		this.xPanderPanelCache.Margin = new System.Windows.Forms.Padding(2);
		this.xPanderPanelCache.Name = "xPanderPanelCache";
		this.xPanderPanelCache.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.xPanderPanelCache.Size = new System.Drawing.Size(228, 25);
		this.xPanderPanelCache.TabIndex = 1;
		this.xPanderPanelCache.Text = "cache";
		this.xPanderPanelCache.ToolTipTextCloseIcon = null;
		this.xPanderPanelCache.ToolTipTextExpandIconPanelCollapsed = null;
		this.xPanderPanelCache.ToolTipTextExpandIconPanelExpanded = null;
		this.xPanderPanelCache.Click += new System.EventHandler(this.xPanderPanelCache_Click);
		this.tableLayoutPanel1.ColumnCount = 3;
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel1.Controls.Add(this.textBoxCacheSize, 1, 12);
		this.tableLayoutPanel1.Controls.Add(this.textBoxCacheStatus, 1, 15);
		this.tableLayoutPanel1.Controls.Add(this.button10, 1, 1);
		this.tableLayoutPanel1.Controls.Add(this.textBoxMemory, 1, 9);
		this.tableLayoutPanel1.Controls.Add(this.checkBoxUseRouteCache, 1, 17);
		this.tableLayoutPanel1.Controls.Add(this.button9, 1, 2);
		this.tableLayoutPanel1.Controls.Add(this.button11, 1, 3);
		this.tableLayoutPanel1.Controls.Add(this.label10, 1, 8);
		this.tableLayoutPanel1.Controls.Add(this.button2, 1, 5);
		this.tableLayoutPanel1.Controls.Add(this.label13, 1, 14);
		this.tableLayoutPanel1.Controls.Add(this.label14, 1, 11);
		this.tableLayoutPanel1.Controls.Add(this.button17, 1, 6);
		this.tableLayoutPanel1.Controls.Add(this.checkBoxTileHost, 1, 19);
		this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel1.Location = new System.Drawing.Point(1, 25);
		this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel1.Name = "tableLayoutPanel1";
		this.tableLayoutPanel1.RowCount = 21;
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel1.Size = new System.Drawing.Size(226, 0);
		this.tableLayoutPanel1.TabIndex = 41;
		this.textBoxCacheSize.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBoxCacheSize.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxCacheSize.Location = new System.Drawing.Point(18, 238);
		this.textBoxCacheSize.Name = "textBoxCacheSize";
		this.textBoxCacheSize.ReadOnly = true;
		this.textBoxCacheSize.Size = new System.Drawing.Size(190, 28);
		this.textBoxCacheSize.TabIndex = 49;
		this.textBoxCacheSize.Text = "...";
		this.textBoxCacheStatus.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBoxCacheStatus.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxCacheStatus.Location = new System.Drawing.Point(18, 301);
		this.textBoxCacheStatus.Name = "textBoxCacheStatus";
		this.textBoxCacheStatus.ReadOnly = true;
		this.textBoxCacheStatus.Size = new System.Drawing.Size(190, 28);
		this.textBoxCacheStatus.TabIndex = 46;
		this.textBoxCacheStatus.Text = "...";
		this.button10.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button10.Location = new System.Drawing.Point(18, 19);
		this.button10.Name = "button10";
		this.button10.Size = new System.Drawing.Size(190, 20);
		this.button10.TabIndex = 5;
		this.button10.Text = "Import";
		this.button10.UseVisualStyleBackColor = true;
		this.button10.Click += new System.EventHandler(this.button10_Click);
		this.textBoxMemory.Dock = System.Windows.Forms.DockStyle.Top;
		this.textBoxMemory.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxMemory.Location = new System.Drawing.Point(18, 175);
		this.textBoxMemory.Name = "textBoxMemory";
		this.textBoxMemory.ReadOnly = true;
		this.textBoxMemory.Size = new System.Drawing.Size(190, 28);
		this.textBoxMemory.TabIndex = 39;
		this.textBoxMemory.Text = "...";
		this.checkBoxUseRouteCache.AutoSize = true;
		this.checkBoxUseRouteCache.Checked = true;
		this.checkBoxUseRouteCache.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxUseRouteCache.Location = new System.Drawing.Point(18, 351);
		this.checkBoxUseRouteCache.Name = "checkBoxUseRouteCache";
		this.checkBoxUseRouteCache.Size = new System.Drawing.Size(160, 17);
		this.checkBoxUseRouteCache.TabIndex = 2;
		this.checkBoxUseRouteCache.Text = "cache routing/geocodig/etc";
		this.checkBoxUseRouteCache.UseVisualStyleBackColor = true;
		this.checkBoxUseRouteCache.CheckedChanged += new System.EventHandler(this.checkBoxUseCache_CheckedChanged);
		this.button9.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button9.Location = new System.Drawing.Point(18, 45);
		this.button9.Name = "button9";
		this.button9.Size = new System.Drawing.Size(190, 20);
		this.button9.TabIndex = 4;
		this.button9.Text = "Export";
		this.button9.UseVisualStyleBackColor = true;
		this.button9.Click += new System.EventHandler(this.button9_Click);
		this.button11.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button11.Location = new System.Drawing.Point(18, 71);
		this.button11.Name = "button11";
		this.button11.Size = new System.Drawing.Size(190, 20);
		this.button11.TabIndex = 38;
		this.button11.Text = "Prefetch selected area";
		this.button11.UseVisualStyleBackColor = true;
		this.button11.Click += new System.EventHandler(this.button11_Click);
		this.label10.AutoSize = true;
		this.label10.Location = new System.Drawing.Point(18, 159);
		this.label10.Name = "label10";
		this.label10.Size = new System.Drawing.Size(111, 13);
		this.label10.TabIndex = 40;
		this.label10.Text = "memory cache usage:";
		this.button2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button2.Location = new System.Drawing.Point(18, 97);
		this.button2.Name = "button2";
		this.button2.Size = new System.Drawing.Size(190, 20);
		this.button2.TabIndex = 43;
		this.button2.Text = "Clear tiles in disk cache";
		this.button2.UseVisualStyleBackColor = true;
		this.button2.Click += new System.EventHandler(this.button2_Click);
		this.label13.AutoSize = true;
		this.label13.Location = new System.Drawing.Point(18, 285);
		this.label13.Name = "label13";
		this.label13.Size = new System.Drawing.Size(93, 13);
		this.label13.TabIndex = 44;
		this.label13.Text = "disk cache status:";
		this.label14.AutoSize = true;
		this.label14.Location = new System.Drawing.Point(18, 222);
		this.label14.Name = "label14";
		this.label14.Size = new System.Drawing.Size(83, 13);
		this.label14.TabIndex = 47;
		this.label14.Text = "disk cache size:";
		this.button17.Dock = System.Windows.Forms.DockStyle.Fill;
		this.button17.Location = new System.Drawing.Point(17, 122);
		this.button17.Margin = new System.Windows.Forms.Padding(2);
		this.button17.Name = "button17";
		this.button17.Size = new System.Drawing.Size(192, 19);
		this.button17.TabIndex = 50;
		this.button17.Text = "Open cache location";
		this.button17.UseVisualStyleBackColor = true;
		this.button17.Click += new System.EventHandler(this.button17_Click);
		this.checkBoxTileHost.AutoSize = true;
		this.checkBoxTileHost.Location = new System.Drawing.Point(18, 390);
		this.checkBoxTileHost.Name = "checkBoxTileHost";
		this.checkBoxTileHost.Size = new System.Drawing.Size(170, 17);
		this.checkBoxTileHost.TabIndex = 51;
		this.checkBoxTileHost.Text = "TileHost - LeafletJS web demo";
		this.checkBoxTileHost.UseVisualStyleBackColor = true;
		this.checkBoxTileHost.CheckedChanged += new System.EventHandler(this.checkBoxTileHost_CheckedChanged);
		this.xPanderPanelInfo.CaptionFont = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.xPanderPanelInfo.Controls.Add(this.tableLayoutPanel2);
		this.xPanderPanelInfo.CustomColors.BackColor = System.Drawing.SystemColors.Control;
		this.xPanderPanelInfo.CustomColors.BorderColor = System.Drawing.Color.FromArgb(184, 184, 184);
		this.xPanderPanelInfo.CustomColors.CaptionCheckedGradientBegin = System.Drawing.Color.Empty;
		this.xPanderPanelInfo.CustomColors.CaptionCheckedGradientEnd = System.Drawing.Color.Empty;
		this.xPanderPanelInfo.CustomColors.CaptionCheckedGradientMiddle = System.Drawing.Color.Empty;
		this.xPanderPanelInfo.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelInfo.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelInfo.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelInfo.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
		this.xPanderPanelInfo.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelInfo.CustomColors.CaptionPressedGradientBegin = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionPressedGradientEnd = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionPressedGradientMiddle = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionSelectedGradientMiddle = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelInfo.CustomColors.CaptionSelectedText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelInfo.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelInfo.CustomColors.FlatCaptionGradientBegin = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelInfo.CustomColors.FlatCaptionGradientEnd = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelInfo.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
		this.xPanderPanelInfo.ForeColor = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelInfo.Image = null;
		this.xPanderPanelInfo.Margin = new System.Windows.Forms.Padding(2);
		this.xPanderPanelInfo.Name = "xPanderPanelInfo";
		this.xPanderPanelInfo.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.xPanderPanelInfo.Size = new System.Drawing.Size(228, 25);
		this.xPanderPanelInfo.TabIndex = 3;
		this.xPanderPanelInfo.Text = "info";
		this.xPanderPanelInfo.ToolTipTextCloseIcon = null;
		this.xPanderPanelInfo.ToolTipTextExpandIconPanelCollapsed = null;
		this.xPanderPanelInfo.ToolTipTextExpandIconPanelExpanded = null;
		this.xPanderPanelInfo.Click += new System.EventHandler(this.xPanderPanelInfo_Click);
		this.tableLayoutPanel2.ColumnCount = 3;
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel2.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel2.Controls.Add(this.textBoxZoomCurrent, 1, 7);
		this.tableLayoutPanel2.Controls.Add(this.textBoxrouteCount, 1, 11);
		this.tableLayoutPanel2.Controls.Add(this.label12, 1, 10);
		this.tableLayoutPanel2.Controls.Add(this.label9, 1, 6);
		this.tableLayoutPanel2.Controls.Add(this.textBoxLngCurrent, 1, 4);
		this.tableLayoutPanel2.Controls.Add(this.textBoxMarkerCount, 1, 9);
		this.tableLayoutPanel2.Controls.Add(this.label11, 1, 8);
		this.tableLayoutPanel2.Controls.Add(this.label4, 1, 3);
		this.tableLayoutPanel2.Controls.Add(this.textBoxLatCurrent, 1, 2);
		this.tableLayoutPanel2.Controls.Add(this.label5, 1, 1);
		this.tableLayoutPanel2.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel2.Location = new System.Drawing.Point(1, 25);
		this.tableLayoutPanel2.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel2.Name = "tableLayoutPanel2";
		this.tableLayoutPanel2.RowCount = 13;
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel2.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel2.Size = new System.Drawing.Size(226, 0);
		this.tableLayoutPanel2.TabIndex = 14;
		this.textBoxZoomCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxZoomCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxZoomCurrent.Location = new System.Drawing.Point(18, 142);
		this.textBoxZoomCurrent.Name = "textBoxZoomCurrent";
		this.textBoxZoomCurrent.ReadOnly = true;
		this.textBoxZoomCurrent.Size = new System.Drawing.Size(190, 28);
		this.textBoxZoomCurrent.TabIndex = 8;
		this.textBoxZoomCurrent.Text = "...";
		this.textBoxrouteCount.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxrouteCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxrouteCount.Location = new System.Drawing.Point(18, 236);
		this.textBoxrouteCount.Name = "textBoxrouteCount";
		this.textBoxrouteCount.ReadOnly = true;
		this.textBoxrouteCount.Size = new System.Drawing.Size(190, 28);
		this.textBoxrouteCount.TabIndex = 12;
		this.textBoxrouteCount.Text = "...";
		this.label12.AutoSize = true;
		this.label12.Location = new System.Drawing.Point(18, 220);
		this.label12.Name = "label12";
		this.label12.Size = new System.Drawing.Size(39, 13);
		this.label12.TabIndex = 13;
		this.label12.Text = "routes:";
		this.label9.AutoSize = true;
		this.label9.Location = new System.Drawing.Point(18, 126);
		this.label9.Name = "label9";
		this.label9.Size = new System.Drawing.Size(35, 13);
		this.label9.TabIndex = 9;
		this.label9.Text = "zoom:";
		this.textBoxLngCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxLngCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxLngCurrent.Location = new System.Drawing.Point(18, 79);
		this.textBoxLngCurrent.Name = "textBoxLngCurrent";
		this.textBoxLngCurrent.ReadOnly = true;
		this.textBoxLngCurrent.Size = new System.Drawing.Size(190, 28);
		this.textBoxLngCurrent.TabIndex = 5;
		this.textBoxLngCurrent.Text = "...";
		this.textBoxMarkerCount.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxMarkerCount.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxMarkerCount.Location = new System.Drawing.Point(18, 189);
		this.textBoxMarkerCount.Name = "textBoxMarkerCount";
		this.textBoxMarkerCount.ReadOnly = true;
		this.textBoxMarkerCount.Size = new System.Drawing.Size(190, 28);
		this.textBoxMarkerCount.TabIndex = 10;
		this.textBoxMarkerCount.Text = "...";
		this.label11.AutoSize = true;
		this.label11.Location = new System.Drawing.Point(18, 173);
		this.label11.Name = "label11";
		this.label11.Size = new System.Drawing.Size(47, 13);
		this.label11.TabIndex = 11;
		this.label11.Text = "markers:";
		this.label4.AutoSize = true;
		this.label4.Location = new System.Drawing.Point(18, 63);
		this.label4.Name = "label4";
		this.label4.Size = new System.Drawing.Size(24, 13);
		this.label4.TabIndex = 7;
		this.label4.Text = "lng:";
		this.textBoxLatCurrent.Dock = System.Windows.Forms.DockStyle.Fill;
		this.textBoxLatCurrent.Font = new System.Drawing.Font("Microsoft Sans Serif", 13.8f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		this.textBoxLatCurrent.Location = new System.Drawing.Point(18, 32);
		this.textBoxLatCurrent.Name = "textBoxLatCurrent";
		this.textBoxLatCurrent.ReadOnly = true;
		this.textBoxLatCurrent.Size = new System.Drawing.Size(190, 28);
		this.textBoxLatCurrent.TabIndex = 4;
		this.textBoxLatCurrent.Text = "...";
		this.label5.AutoSize = true;
		this.label5.Location = new System.Drawing.Point(18, 16);
		this.label5.Name = "label5";
		this.label5.Size = new System.Drawing.Size(21, 13);
		this.label5.TabIndex = 6;
		this.label5.Text = "lat:";
		this.xPanderPanelLive.CaptionFont = new System.Drawing.Font("Segoe UI", 8f, System.Drawing.FontStyle.Bold);
		this.xPanderPanelLive.Controls.Add(this.tableLayoutPanel3);
		this.xPanderPanelLive.CustomColors.BackColor = System.Drawing.SystemColors.Control;
		this.xPanderPanelLive.CustomColors.BorderColor = System.Drawing.Color.FromArgb(184, 184, 184);
		this.xPanderPanelLive.CustomColors.CaptionCheckedGradientBegin = System.Drawing.Color.Empty;
		this.xPanderPanelLive.CustomColors.CaptionCheckedGradientEnd = System.Drawing.Color.Empty;
		this.xPanderPanelLive.CustomColors.CaptionCheckedGradientMiddle = System.Drawing.Color.Empty;
		this.xPanderPanelLive.CustomColors.CaptionCloseIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelLive.CustomColors.CaptionExpandIcon = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelLive.CustomColors.CaptionGradientBegin = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelLive.CustomColors.CaptionGradientEnd = System.Drawing.SystemColors.ButtonFace;
		this.xPanderPanelLive.CustomColors.CaptionGradientMiddle = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelLive.CustomColors.CaptionPressedGradientBegin = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionPressedGradientEnd = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionPressedGradientMiddle = System.Drawing.Color.FromArgb(128, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionSelectedGradientBegin = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionSelectedGradientEnd = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionSelectedGradientMiddle = System.Drawing.Color.FromArgb(179, 255, 255);
		this.xPanderPanelLive.CustomColors.CaptionSelectedText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelLive.CustomColors.CaptionText = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelLive.CustomColors.FlatCaptionGradientBegin = System.Drawing.Color.FromArgb(248, 248, 248);
		this.xPanderPanelLive.CustomColors.FlatCaptionGradientEnd = System.Drawing.Color.FromArgb(252, 252, 252);
		this.xPanderPanelLive.CustomColors.InnerBorderColor = System.Drawing.SystemColors.Window;
		this.xPanderPanelLive.ForeColor = System.Drawing.SystemColors.ControlText;
		this.xPanderPanelLive.Image = null;
		this.xPanderPanelLive.IsClosable = false;
		this.xPanderPanelLive.Margin = new System.Windows.Forms.Padding(2);
		this.xPanderPanelLive.Name = "xPanderPanelLive";
		this.xPanderPanelLive.Padding = new System.Windows.Forms.Padding(0, 0, 0, 24);
		this.xPanderPanelLive.PanelStyle = BSE.Windows.Forms.PanelStyle.Office2007;
		this.xPanderPanelLive.Size = new System.Drawing.Size(228, 25);
		this.xPanderPanelLive.TabIndex = 2;
		this.xPanderPanelLive.Text = "live";
		this.xPanderPanelLive.ToolTipTextCloseIcon = null;
		this.xPanderPanelLive.ToolTipTextExpandIconPanelCollapsed = null;
		this.xPanderPanelLive.ToolTipTextExpandIconPanelExpanded = null;
		this.xPanderPanelLive.Click += new System.EventHandler(this.xPanderPanelLive_Click);
		this.tableLayoutPanel3.ColumnCount = 3;
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 29.06977f));
		this.tableLayoutPanel3.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 15f));
		this.tableLayoutPanel3.Controls.Add(this.radioButtonNone, 1, 1);
		this.tableLayoutPanel3.Controls.Add(this.checkBoxTcpIpSnap, 1, 7);
		this.tableLayoutPanel3.Controls.Add(this.GridConnections, 1, 6);
		this.tableLayoutPanel3.Controls.Add(this.radioButtonFlight, 1, 3);
		this.tableLayoutPanel3.Controls.Add(this.radioButtonPerf, 1, 4);
		this.tableLayoutPanel3.Controls.Add(this.radioButtonTcpIp, 1, 5);
		this.tableLayoutPanel3.Controls.Add(this.checkBoxTraceRoute, 1, 8);
		this.tableLayoutPanel3.Controls.Add(this.radioButtonVehicle, 1, 2);
		this.tableLayoutPanel3.Dock = System.Windows.Forms.DockStyle.Fill;
		this.tableLayoutPanel3.Location = new System.Drawing.Point(1, 25);
		this.tableLayoutPanel3.Margin = new System.Windows.Forms.Padding(2);
		this.tableLayoutPanel3.Name = "tableLayoutPanel3";
		this.tableLayoutPanel3.RowCount = 9;
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 16f));
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50f));
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.RowStyles.Add(new System.Windows.Forms.RowStyle());
		this.tableLayoutPanel3.Size = new System.Drawing.Size(226, 0);
		this.tableLayoutPanel3.TabIndex = 6;
		this.radioButtonNone.AutoSize = true;
		this.radioButtonNone.Checked = true;
		this.radioButtonNone.Location = new System.Drawing.Point(18, 19);
		this.radioButtonNone.Name = "radioButtonNone";
		this.radioButtonNone.Size = new System.Drawing.Size(51, 17);
		this.radioButtonNone.TabIndex = 2;
		this.radioButtonNone.TabStop = true;
		this.radioButtonNone.Text = "None";
		this.radioButtonNone.UseVisualStyleBackColor = true;
		this.radioButtonNone.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
		this.checkBoxTcpIpSnap.AutoSize = true;
		this.checkBoxTcpIpSnap.Checked = true;
		this.checkBoxTcpIpSnap.CheckState = System.Windows.Forms.CheckState.Checked;
		this.checkBoxTcpIpSnap.Location = new System.Drawing.Point(17, -39);
		this.checkBoxTcpIpSnap.Margin = new System.Windows.Forms.Padding(2);
		this.checkBoxTcpIpSnap.Name = "checkBoxTcpIpSnap";
		this.checkBoxTcpIpSnap.Size = new System.Drawing.Size(184, 17);
		this.checkBoxTcpIpSnap.TabIndex = 4;
		this.checkBoxTcpIpSnap.Text = "Snap position on new connection";
		this.checkBoxTcpIpSnap.UseVisualStyleBackColor = true;
		this.checkBoxTcpIpSnap.Visible = false;
		this.GridConnections.AllowUserToAddRows = false;
		this.GridConnections.AllowUserToDeleteRows = false;
		this.GridConnections.AllowUserToResizeRows = false;
		this.GridConnections.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
		this.GridConnections.Columns.AddRange(this.CountryName, this.ConnectionsCount);
		dataGridViewCellStyle.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft;
		dataGridViewCellStyle.BackColor = System.Drawing.Color.AliceBlue;
		dataGridViewCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25f, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, 0);
		dataGridViewCellStyle.ForeColor = System.Drawing.SystemColors.ControlText;
		dataGridViewCellStyle.SelectionBackColor = System.Drawing.Color.LightSkyBlue;
		dataGridViewCellStyle.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
		dataGridViewCellStyle.WrapMode = System.Windows.Forms.DataGridViewTriState.False;
		this.GridConnections.DefaultCellStyle = dataGridViewCellStyle;
		this.GridConnections.Dock = System.Windows.Forms.DockStyle.Fill;
		this.GridConnections.EditMode = System.Windows.Forms.DataGridViewEditMode.EditProgrammatically;
		this.GridConnections.Location = new System.Drawing.Point(17, 133);
		this.GridConnections.Margin = new System.Windows.Forms.Padding(2);
		this.GridConnections.Name = "GridConnections";
		this.GridConnections.ReadOnly = true;
		this.GridConnections.RowHeadersVisible = false;
		this.GridConnections.RowTemplate.Height = 24;
		this.GridConnections.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.GridConnections.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
		this.GridConnections.Size = new System.Drawing.Size(192, 1);
		this.GridConnections.TabIndex = 5;
		this.GridConnections.VirtualMode = true;
		this.GridConnections.Visible = false;
		this.GridConnections.CellValueNeeded += new System.Windows.Forms.DataGridViewCellValueEventHandler(this.GridConnections_CellValueNeeded);
		this.GridConnections.DoubleClick += new System.EventHandler(this.GridConnections_DoubleClick);
		this.CountryName.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
		this.CountryName.DataPropertyName = "CountryName";
		this.CountryName.FillWeight = 200f;
		this.CountryName.HeaderText = "Country";
		this.CountryName.Name = "CountryName";
		this.CountryName.ReadOnly = true;
		this.ConnectionsCount.DataPropertyName = "ConnectionsCount";
		this.ConnectionsCount.HeaderText = "Connections";
		this.ConnectionsCount.Name = "ConnectionsCount";
		this.ConnectionsCount.ReadOnly = true;
		this.radioButtonFlight.AutoSize = true;
		this.radioButtonFlight.Location = new System.Drawing.Point(18, 65);
		this.radioButtonFlight.Name = "radioButtonFlight";
		this.radioButtonFlight.Size = new System.Drawing.Size(108, 17);
		this.radioButtonFlight.TabIndex = 0;
		this.radioButtonFlight.Text = "FlightRadar demo";
		this.radioButtonFlight.UseVisualStyleBackColor = true;
		this.radioButtonFlight.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
		this.radioButtonPerf.AutoSize = true;
		this.radioButtonPerf.Location = new System.Drawing.Point(18, 88);
		this.radioButtonPerf.Name = "radioButtonPerf";
		this.radioButtonPerf.Size = new System.Drawing.Size(105, 17);
		this.radioButtonPerf.TabIndex = 1;
		this.radioButtonPerf.Text = "Performance test";
		this.radioButtonPerf.UseVisualStyleBackColor = true;
		this.radioButtonPerf.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
		this.radioButtonTcpIp.AutoSize = true;
		this.radioButtonTcpIp.Location = new System.Drawing.Point(18, 111);
		this.radioButtonTcpIp.Name = "radioButtonTcpIp";
		this.radioButtonTcpIp.Size = new System.Drawing.Size(122, 17);
		this.radioButtonTcpIp.TabIndex = 3;
		this.radioButtonTcpIp.Text = "TCP/IP connections";
		this.radioButtonTcpIp.UseVisualStyleBackColor = true;
		this.radioButtonTcpIp.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
		this.checkBoxTraceRoute.AutoSize = true;
		this.checkBoxTraceRoute.Location = new System.Drawing.Point(17, -18);
		this.checkBoxTraceRoute.Margin = new System.Windows.Forms.Padding(2);
		this.checkBoxTraceRoute.Name = "checkBoxTraceRoute";
		this.checkBoxTraceRoute.Size = new System.Drawing.Size(179, 17);
		this.checkBoxTraceRoute.TabIndex = 6;
		this.checkBoxTraceRoute.Text = "Trace route for each connection";
		this.checkBoxTraceRoute.UseVisualStyleBackColor = true;
		this.checkBoxTraceRoute.Visible = false;
		this.checkBoxTraceRoute.CheckedChanged += new System.EventHandler(this.checkBoxTraceRoute_CheckedChanged);
		this.radioButtonVehicle.AutoSize = true;
		this.radioButtonVehicle.Location = new System.Drawing.Point(18, 42);
		this.radioButtonVehicle.Name = "radioButtonVehicle";
		this.radioButtonVehicle.Size = new System.Drawing.Size(89, 17);
		this.radioButtonVehicle.TabIndex = 7;
		this.radioButtonVehicle.Text = "Vehicle demo";
		this.radioButtonVehicle.UseVisualStyleBackColor = true;
		this.radioButtonVehicle.CheckedChanged += new System.EventHandler(this.RealTimeChanged);
		base.AutoScaleDimensions = new System.Drawing.SizeF(96f, 96f);
		base.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
		this.BackColor = System.Drawing.Color.AliceBlue;
		base.ClientSize = new System.Drawing.Size(891, 821);
		base.Controls.Add(this.panel4);
		base.KeyPreview = true;
		this.MinimumSize = new System.Drawing.Size(554, 106);
		base.Name = "MainForm";
		base.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
		this.Text = "GMap.NET CMANO - 2.1.7 Preview 5";
		base.WindowState = System.Windows.Forms.FormWindowState.Maximized;
		base.Load += new System.EventHandler(this.MainForm_Load);
		base.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.MainForm_KeyPress);
		base.KeyUp += new System.Windows.Forms.KeyEventHandler(this.MainForm_KeyUp);
		this.panel2.ResumeLayout(false);
		this.panel4.ResumeLayout(false);
		this.panelMenu.ResumeLayout(false);
		this.xPanderPanelList1.ResumeLayout(false);
		this.xPanderPanelMain.ResumeLayout(false);
		this.tableLayoutPanel4.ResumeLayout(false);
		this.tableLayoutPanel5.ResumeLayout(false);
		this.groupBox8.ResumeLayout(false);
		this.groupBox8.PerformLayout();
		this.groupBox7.ResumeLayout(false);
		this.groupBox7.PerformLayout();
		this.groupBox5.ResumeLayout(false);
		this.groupBox5.PerformLayout();
		this.groupBox3.ResumeLayout(false);
		this.groupBox3.PerformLayout();
		this.tableLayoutPanel6.ResumeLayout(false);
		this.tableLayoutPanel6.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.trackBar1).EndInit();
		this.xPanderPanelCache.ResumeLayout(false);
		this.tableLayoutPanel1.ResumeLayout(false);
		this.tableLayoutPanel1.PerformLayout();
		this.xPanderPanelInfo.ResumeLayout(false);
		this.tableLayoutPanel2.ResumeLayout(false);
		this.tableLayoutPanel2.PerformLayout();
		this.xPanderPanelLive.ResumeLayout(false);
		this.tableLayoutPanel3.ResumeLayout(false);
		this.tableLayoutPanel3.PerformLayout();
		((System.ComponentModel.ISupportInitialize)this.GridConnections).EndInit();
		InitializeModernWorkspace();
		base.ResumeLayout(false);
	}
}
