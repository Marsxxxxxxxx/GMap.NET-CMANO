using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Resources;
using System.Runtime.CompilerServices;

namespace Demo.WindowsForms.Properties;

[GeneratedCode("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
[DebuggerNonUserCode]
[CompilerGenerated]
internal class Resources
{
	private static ResourceManager resourceMan;

	private static CultureInfo resourceCulture;

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static ResourceManager ResourceManager
	{
		get
		{
			if (resourceMan == null)
			{
				resourceMan = new ResourceManager("Demo.WindowsForms.Properties.Resources", typeof(Resources).Assembly);
			}
			return resourceMan;
		}
	}

	[EditorBrowsable(EditorBrowsableState.Advanced)]
	internal static CultureInfo Culture
	{
		get
		{
			return resourceCulture;
		}
		set
		{
			resourceCulture = value;
		}
	}

	internal static Bitmap ChevronDown => (Bitmap)ResourceManager.GetObject("ChevronDown", resourceCulture);

	internal static Bitmap ChevronLeft => (Bitmap)ResourceManager.GetObject("ChevronLeft", resourceCulture);

	internal static Bitmap ChevronRight => (Bitmap)ResourceManager.GetObject("ChevronRight", resourceCulture);

	internal static Bitmap ChevronUp => (Bitmap)ResourceManager.GetObject("ChevronUp", resourceCulture);

	internal static Bitmap closePanel => (Bitmap)ResourceManager.GetObject("closePanel", resourceCulture);

	internal static Bitmap Collapse => (Bitmap)ResourceManager.GetObject("Collapse", resourceCulture);

	internal static Bitmap Collapse_h => (Bitmap)ResourceManager.GetObject("Collapse_h", resourceCulture);

	internal static Bitmap Expand => (Bitmap)ResourceManager.GetObject("Expand", resourceCulture);

	internal static Bitmap Expand_h => (Bitmap)ResourceManager.GetObject("Expand_h", resourceCulture);

	internal static string IDS_ArgumentException => ResourceManager.GetString("IDS_ArgumentException", resourceCulture);

	internal static string IDS_InvalidBoundArgument => ResourceManager.GetString("IDS_InvalidBoundArgument", resourceCulture);

	internal static string IDS_InvalidLowBoundArgument => ResourceManager.GetString("IDS_InvalidLowBoundArgument", resourceCulture);

	internal static string IDS_InvalidOperationExceptionInteger => ResourceManager.GetString("IDS_InvalidOperationExceptionInteger", resourceCulture);

	internal static string IpCacheCreateDb => ResourceManager.GetString("IpCacheCreateDb", resourceCulture);

	internal Resources()
	{
	}
}
