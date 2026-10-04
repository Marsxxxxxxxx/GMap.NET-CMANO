using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing.Design;
using System.Windows.Forms;

namespace BSE.Windows.Forms;

public class XPanderPanelListDesignerActionList : DesignerActionList
{
	[Editor(typeof(XPanderPanelCollectionEditor), typeof(UITypeEditor))]
	public XPanderPanelCollection XPanderPanels => XPanderPanelList.XPanderPanels;

	public PanelStyle PanelStyle
	{
		get
		{
			return XPanderPanelList.PanelStyle;
		}
		set
		{
			SetProperty("PanelStyle", value);
		}
	}

	public ColorScheme ColorScheme
	{
		get
		{
			return XPanderPanelList.ColorScheme;
		}
		set
		{
			SetProperty("ColorScheme", value);
		}
	}

	public CaptionStyle CaptionStyle
	{
		get
		{
			return XPanderPanelList.CaptionStyle;
		}
		set
		{
			SetProperty("CaptionStyle", value);
		}
	}

	public bool ShowBorder
	{
		get
		{
			return XPanderPanelList.ShowBorder;
		}
		set
		{
			SetProperty("ShowBorder", value);
		}
	}

	public bool ShowExpandIcon
	{
		get
		{
			return XPanderPanelList.ShowExpandIcon;
		}
		set
		{
			SetProperty("ShowExpandIcon", value);
		}
	}

	public bool ShowCloseIcon
	{
		get
		{
			return XPanderPanelList.ShowCloseIcon;
		}
		set
		{
			SetProperty("ShowCloseIcon", value);
		}
	}

	private XPanderPanelList XPanderPanelList => (XPanderPanelList)Component;

	public XPanderPanelListDesignerActionList(IComponent component)
		: base(component)
	{
		base.AutoShow = true;
	}

	public override DesignerActionItemCollection GetSortedActionItems()
	{
		return new DesignerActionItemCollection
		{
			new DesignerActionMethodItem(this, "ToggleDockStyle", GetDockStyleText(), "Design", "Dock or undock this control in it's parent container.", includeAsDesignerVerb: true),
			new DesignerActionPropertyItem("ShowBorder", "Show Border", GetCategory(XPanderPanelList, "ShowBorder")),
			new DesignerActionPropertyItem("ShowExpandIcon", "Show ExpandIcon", GetCategory(XPanderPanelList, "ShowExpandIcon")),
			new DesignerActionPropertyItem("ShowCloseIcon", "Show CloseIcon", GetCategory(XPanderPanelList, "ShowCloseIcon")),
			new DesignerActionPropertyItem("PanelStyle", "Select PanelStyle", GetCategory(XPanderPanelList, "PanelStyle")),
			new DesignerActionPropertyItem("ColorScheme", "Select ColorScheme", GetCategory(XPanderPanelList, "ColorScheme")),
			new DesignerActionPropertyItem("CaptionStyle", "Select CaptionStyle", GetCategory(XPanderPanelList, "CaptionStyle")),
			new DesignerActionPropertyItem("XPanderPanels", "Edit XPanderPanels", GetCategory(XPanderPanelList, "XPanderPanels"))
		};
	}

	public void ToggleDockStyle()
	{
		if (XPanderPanelList.Dock != DockStyle.Fill)
		{
			SetProperty("Dock", DockStyle.Fill);
		}
		else
		{
			SetProperty("Dock", DockStyle.None);
		}
	}

	private string GetDockStyleText()
	{
		if (XPanderPanelList.Dock == DockStyle.Fill)
		{
			return "Undock in parent container";
		}
		return "Dock in parent container";
	}

	private void SetProperty(string propertyName, object value)
	{
		TypeDescriptor.GetProperties(XPanderPanelList)[propertyName].SetValue(XPanderPanelList, value);
	}

	private static string GetCategory(object source, string propertyName)
	{
		return ((CategoryAttribute)source.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(CategoryAttribute), inherit: false)[0])?.Category;
	}
}
