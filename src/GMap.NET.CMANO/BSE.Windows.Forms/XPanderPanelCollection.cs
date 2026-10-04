using System;
using System.Collections;
using System.Globalization;
using System.Windows.Forms;
using Demo.WindowsForms.Properties;

namespace BSE.Windows.Forms;

public sealed class XPanderPanelCollection : IList, ICollection, IEnumerable
{
	private XPanderPanelList m_xpanderPanelList;

	private Control.ControlCollection m_controlCollection;

	public XPanderPanel this[int index] => (XPanderPanel)m_controlCollection[index];

	public int Count => m_controlCollection.Count;

	public bool IsReadOnly => m_controlCollection.IsReadOnly;

	int ICollection.Count => Count;

	bool ICollection.IsSynchronized => ((ICollection)m_controlCollection).IsSynchronized;

	object ICollection.SyncRoot => ((ICollection)m_controlCollection).SyncRoot;

	object IList.this[int index]
	{
		get
		{
			return m_controlCollection[index];
		}
		set
		{
		}
	}

	bool IList.IsReadOnly => IsReadOnly;

	bool IList.IsFixedSize => ((IList)m_controlCollection).IsFixedSize;

	internal XPanderPanelCollection(XPanderPanelList xpanderPanelList)
	{
		m_xpanderPanelList = xpanderPanelList;
		m_controlCollection = m_xpanderPanelList.Controls;
	}

	public bool Contains(XPanderPanel xpanderPanel)
	{
		return m_controlCollection.Contains(xpanderPanel);
	}

	public void Add(XPanderPanel xpanderPanel)
	{
		m_controlCollection.Add(xpanderPanel);
		m_xpanderPanelList.Invalidate();
	}

	public void Remove(XPanderPanel xpanderPanel)
	{
		m_controlCollection.Remove(xpanderPanel);
	}

	public void Clear()
	{
		m_controlCollection.Clear();
	}

	public IEnumerator GetEnumerator()
	{
		return m_controlCollection.GetEnumerator();
	}

	public int IndexOf(XPanderPanel xpanderPanel)
	{
		return m_controlCollection.IndexOf(xpanderPanel);
	}

	public void RemoveAt(int index)
	{
		m_controlCollection.RemoveAt(index);
	}

	public void Insert(int index, XPanderPanel xpanderPanel)
	{
		((IList)this).Insert(index, (object)xpanderPanel);
	}

	public void CopyTo(XPanderPanel[] xpanderPanels, int index)
	{
		m_controlCollection.CopyTo(xpanderPanels, index);
	}

	void ICollection.CopyTo(Array array, int index)
	{
		((ICollection)m_controlCollection).CopyTo(array, index);
	}

	int IList.Add(object value)
	{
		if (!(value is XPanderPanel xpanderPanel))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.IDS_ArgumentException, new object[1] { typeof(XPanderPanel).Name }));
		}
		Add(xpanderPanel);
		return IndexOf(xpanderPanel);
	}

	bool IList.Contains(object value)
	{
		return Contains(value as XPanderPanel);
	}

	int IList.IndexOf(object value)
	{
		return IndexOf(value as XPanderPanel);
	}

	void IList.Insert(int index, object value)
	{
		if (!(value is XPanderPanel))
		{
			throw new ArgumentException(string.Format(CultureInfo.CurrentUICulture, Resources.IDS_ArgumentException, new object[1] { typeof(XPanderPanel).Name }));
		}
	}

	void IList.Remove(object value)
	{
		Remove(value as XPanderPanel);
	}

	void IList.RemoveAt(int index)
	{
		RemoveAt(index);
	}
}
