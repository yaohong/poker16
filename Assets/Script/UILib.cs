using System;
using UnityEngine;

public static class UILib
{
	static public void SwitchProcedurePanel(string name)
	{
		GameObject rootPanels = GameObject.Find ("_ROOT_PANELS");
		if (rootPanels == null) 
		{
			return;
		}

		for (int i = 0; i < rootPanels.transform.childCount; ++i) 
		{
			GameObject child = rootPanels.transform.GetChild (i).gameObject;
			child.SetActive (child.name == name);
		}
	}
}
