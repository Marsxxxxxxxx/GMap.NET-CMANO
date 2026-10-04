using System.ComponentModel;
using System.ComponentModel.Design;
using System.Windows.Forms;

namespace BSE.Windows.Forms;

public class PanelDesignerActionList : DesignerActionList
{
	public bool ShowCaptionbar
	{
		get
		{
			return Panel.ShowCaptionbar;
		}
		set
		{
			SetProperty("ShowCaptionbar", value);
		}
	}

	public bool ShowTransparentBackground
	{
		get
		{
			return Panel.ShowTransparentBackground;
		}
		set
		{
			SetProperty("ShowTransparentBackground", value);
		}
	}

	public bool ShowXPanderPanelProfessionalStyle
	{
		get
		{
			return Panel.ShowXPanderPanelProfessionalStyle;
		}
		set
		{
			SetProperty("ShowXPanderPanelProfessionalStyle", value);
		}
	}

	public bool ShowExpandIcon
	{
		get
		{
			return Panel.ShowExpandIcon;
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
			return Panel.ShowCloseIcon;
		}
		set
		{
			SetProperty("ShowCloseIcon", value);
		}
	}

	public PanelStyle PanelStyle
	{
		get
		{
			return Panel.PanelStyle;
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
			return Panel.ColorScheme;
		}
		set
		{
			SetProperty("ColorScheme", value);
		}
	}

	private Panel Panel => (Panel)Component;

	public PanelDesignerActionList(IComponent component)
		: base(component)
	{
		base.AutoShow = true;
	}

	public override DesignerActionItemCollection GetSortedActionItems()
	{
		return new DesignerActionItemCollection
		{
			new DesignerActionMethodItem(this, "ToggleDockStyle", GetDockStyleText(), "Design", "Dock or undock this control in it's parent container.", includeAsDesignerVerb: true),
			new DesignerActionPropertyItem("ShowTransparentBackground", "Show transparent backcolor", GetCategory(Panel, "ShowTransparentBackground")),
			new DesignerActionPropertyItem("ShowXPanderPanelProfessionalStyle", "Show the XPanderPanels professional colorscheme", GetCategory(Panel, "ShowXPanderPanelProfessionalStyle")),
			new DesignerActionPropertyItem("ShowCaptionbar", "Show the captionbar on top of the panel", GetCategory(Panel, "ShowCaptionbar")),
			new DesignerActionPropertyItem("ShowExpandIcon", "Show the expand panel icon (not at DockStyle.None or DockStyle.Fill)", GetCategory(Panel, "ShowExpandIcon")),
			new DesignerActionPropertyItem("ShowCloseIcon", "Show the close panel icon (not at DockStyle.None or DockStyle.Fill)", GetCategory(Panel, "ShowCloseIcon")),
			new DesignerActionPropertyItem("PanelStyle", "Select PanelStyle", GetCategory(Panel, "PanelStyle")),
			new DesignerActionPropertyItem("ColorScheme", "Select ColorScheme", GetCategory(Panel, "ColorScheme"))
		};
	}

	public void ToggleDockStyle()
	{
		if (Panel.Dock != DockStyle.Fill)
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
		if (Panel.Dock == DockStyle.Fill)
		{
			return "Undock in parent container";
		}
		return "Dock in parent container";
	}

	private void SetProperty(string propertyName, object value)
	{
		TypeDescriptor.GetProperties(Panel)[propertyName].SetValue(Panel, value);
	}

	private static string GetCategory(object source, string propertyName)
	{
		return ((CategoryAttribute)source.GetType().GetProperty(propertyName).GetCustomAttributes(typeof(CategoryAttribute), inherit: false)[0])?.Category;
	}
}
