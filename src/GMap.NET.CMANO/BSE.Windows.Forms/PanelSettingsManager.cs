using System;
using System.Collections;
using System.Globalization;
using System.Threading;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

public static class PanelSettingsManager
{
	public static void SetPanelProperties(Control.ControlCollection controls, PanelColors panelColors)
	{
		if (panelColors == null)
		{
			throw new ArgumentNullException("panelColors", string.Format(CultureInfo.InvariantCulture, Resources.IDS_ArgumentException, new object[1] { "panelColors" }));
		}
		PanelStyle panelStyle = panelColors.PanelStyle;
		SetPanelProperties(controls, panelStyle, panelColors);
	}

	public static void SetPanelProperties(Control.ControlCollection controls, PanelStyle panelStyle, PanelColors panelColors)
	{
		if (panelColors == null)
		{
			throw new ArgumentNullException("panelColors", string.Format(CultureInfo.InvariantCulture, Resources.IDS_ArgumentException, new object[1] { "panelColors" }));
		}
		foreach (BasePanel item in FindPanels(searchAllChildren: true, controls))
		{
			item.PanelStyle = panelStyle;
			panelColors.Panel = item;
			item.SetPanelProperties(panelColors);
		}
		foreach (XPanderPanelList item2 in FindPanelLists(searchAllChildren: true, controls))
		{
			item2.PanelStyle = panelStyle;
			item2.PanelColors = panelColors;
		}
	}

	public static void SetPanelProperties(Control.ControlCollection controls, PanelStyle panelStyle)
	{
		ArrayList arrayList = FindPanels(searchAllChildren: true, controls);
		if (arrayList == null)
		{
			return;
		}
		foreach (BasePanel item in arrayList)
		{
			item.PanelStyle = panelStyle;
		}
	}

	public static ArrayList FindPanels(bool searchAllChildren, Control.ControlCollection controlsToLookIn)
	{
		return FindControls(typeof(BasePanel), searchAllChildren, controlsToLookIn, new ArrayList());
	}

	public static ArrayList FindPanelLists(bool searchAllChildren, Control.ControlCollection controlsToLookIn)
	{
		return FindControls(typeof(XPanderPanelList), searchAllChildren, controlsToLookIn, new ArrayList());
	}

	private static ArrayList FindControls(Type baseType, bool searchAllChildren, Control.ControlCollection controlsToLookIn, ArrayList foundControls)
	{
		if (controlsToLookIn == null || foundControls == null)
		{
			return null;
		}
		checked
		{
			try
			{
				for (int i = 0; i < controlsToLookIn.Count; i++)
				{
					if (controlsToLookIn[i] != null && baseType.IsAssignableFrom(controlsToLookIn[i].GetType()))
					{
						foundControls.Add(controlsToLookIn[i]);
					}
				}
				if (!searchAllChildren)
				{
					return foundControls;
				}
				for (int j = 0; j < controlsToLookIn.Count; j++)
				{
					if (controlsToLookIn[j] != null && !(controlsToLookIn[j] is Form) && controlsToLookIn[j].Controls != null && controlsToLookIn[j].Controls.Count > 0)
					{
						foundControls = FindControls(baseType, searchAllChildren, controlsToLookIn[j].Controls, foundControls);
					}
				}
			}
			catch (Exception exception)
			{
				if (IsCriticalException(exception))
				{
					throw;
				}
			}
			return foundControls;
		}
	}

	private static bool IsCriticalException(Exception exception)
	{
		if (!(exception is NullReferenceException) && !(exception is StackOverflowException) && !(exception is OutOfMemoryException) && !(exception is ThreadAbortException) && !(exception is ExecutionEngineException) && !(exception is IndexOutOfRangeException))
		{
			return exception is AccessViolationException;
		}
		return true;
	}
}
