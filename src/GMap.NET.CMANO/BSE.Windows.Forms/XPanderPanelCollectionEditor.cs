using System;
using System.ComponentModel.Design;

namespace BSE.Windows.Forms;

internal class XPanderPanelCollectionEditor : CollectionEditor
{
	private CollectionForm m_collectionForm;

	public XPanderPanelCollectionEditor(Type type)
		: base(type)
	{
	}

	protected override CollectionForm CreateCollectionForm()
	{
		m_collectionForm = base.CreateCollectionForm();
		return m_collectionForm;
	}

	protected override object CreateInstance(Type ItemType)
	{
		XPanderPanel xPanderPanel = (XPanderPanel)base.CreateInstance(ItemType);
		if (Context.Instance != null)
		{
			xPanderPanel.Expand = true;
		}
		return xPanderPanel;
	}
}
