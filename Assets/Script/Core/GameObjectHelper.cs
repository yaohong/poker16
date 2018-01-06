using UnityEngine;
using System;
using UnityEngine.SceneManagement;
using System.Reflection;
using System.Text;
using System.Collections.Generic;

public static class GameObjectHelper
{
    // topLevel : find path start from root node.
    public static GameObject Find(string path)
    {
        string[] names = path.Split('/');
        int len = names.Length;
        if (len == 0)
        {
            return null;
        }

        GameObject root = GameObject.Find(names[0]);
        if (root == null)
        {
            return null;
        }

        if (root.transform.parent != null)
        {
            return null;
        }

        for (int i = 1; i < names.Length; ++i)
        {
            Transform t = root.transform.Find(names[i]);
            if (t == null)
            {
                return null;
            }

            root = t.gameObject;
            if (root == null)
            {
                return null;
            }
        }

        return root;
    }

    private static Transform _RecursionFindChild(Transform trans, string childName)
    {
        Transform childTrans = trans.Find(childName);
        if (null != childTrans)
        {
            return childTrans;
        }
        else
        {
            for (int i = 0; i < trans.childCount; ++i)
            {
                Transform child = _RecursionFindChild(trans.GetChild(i), childName);

                if (child != null)
                {
                    return child;
                }
            }
        }

        return null;
    }

    private static void _getChildIndexPath(Transform child, Transform root, List<int> path)
    {
        if (null == child)
        {
            return;
        }

        path.Add(child.GetSiblingIndex());

        Transform p = child.parent;
        if (!ReferenceEquals(p, root) && null != p)
        {
            _getChildIndexPath(p, root, path);
        }
    }

    public static GameObject AddPrefabToUIRoot(string prefabName)
    {
        GameObject uiRoot = GameObject.Find("_ROOT/_UI_ROOT");
        if (uiRoot == null)
        {
            return null;
        }

        return AddChildPrefab(uiRoot, prefabName);
    }

    static public GameObject AddChildPrefab(GameObject parent, string prefabName)
    {
        if (parent == null)
        {
            return null;
        }

		GameObject prefab = Resources.Load<GameObject>(prefabName);
        if (prefab == null)
        {
            return null;
        }

        return NGUITools.AddChild(parent, prefab);
    }

	public static string GetFullPathName(this GameObject go)
	{
		if (go == null)
		{
			return string.Empty;
		}

		if (go.transform.parent == null)
		{
			return go.name;
		}

		return string.Format("{0}\\{1}",
			go.transform.parent.gameObject.GetFullPathName(),
			go.name);
	}

    //在所有子节点的GameObject中查找
    public static GameObject FindChildByName(this GameObject parent, string childName)
    {
        if (null == parent)
        {
            Debug.LogErrorFormat("FindChildByName [{0}] with NULL Parent", childName);
            return null;
        }
        else if (null == childName)
        {
            Debug.LogErrorFormat("FindChildByName [NULL ChildName] of [{0}]", parent.GetFullPathName());
            return null;
        }

        Transform result = _RecursionFindChild(parent.transform, childName);

#if VERBOSE_DEBUG_OUTPUT
        if (null == result)
        {
            Debug.LogErrorFormat(parent,
                "FindChildByName: [{0}] of [{1}] NOT FOUND!", childName, parent.name);
        }
#endif
        return (null != result ? result.gameObject : null);
    }

    //在直接下层的子节点的GameObject中查找
    public static GameObject FindDirectChildByName(this GameObject parent, string str)
    {
        if (null == parent)
        {
            Debug.LogError("FindDirectChildByName NULL Parent: " + str);
            return null;
        }

        Transform result = parent.transform.Find(str);
        return (null != result ? result.gameObject : null);
    }

    public static Transform GetChildByIndexPath(GameObject parent, string indexPath)
    {
        if (parent == null)
        {
            Debug.LogError("GetChildByIndexPath NULL Parent");
            return null;
        }

        string[] path = indexPath.Split('/');

        Transform current = parent.transform;
        Transform target = null;

        for (int level = 0; level < path.Length; ++level)
        {
            int levelIndex;
            if (!int.TryParse(path[level], out levelIndex))
            {
                break;
            }
            if (current.childCount > levelIndex)
            {
                Transform levelTrans = current.GetChild(levelIndex);

                if (level == path.Length - 1)
                {
                    target = levelTrans;
                }
                else
                {
                    current = levelTrans;
                }
            }
            else
            {
                break;
            }
        }

        return target;
    }


    public static GameObject GetChildByIndex(GameObject parent, int index)
    {
        if (parent == null)
        {
            Debug.LogError("GetChildByIndex NULL Parent");
            return null;
        }

        Transform target = parent.transform.GetChild(index);
        return ((null != target) ? target.gameObject : null);
    }


    public static bool IsContain(this GameObject parent, GameObject obj)
    {
        while (parent != null && obj != null)
        {
            if (parent == obj)
            {
                return true;
            }

            if (obj.transform.parent == null)
            {
                return false;
            }

            obj = obj.transform.parent.gameObject;
        }

        return false;
    }

    public static GameObject AddChild(this GameObject parent, string name)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent.transform);
        return obj;
    }
	    
    public static GameObject FirstChild(this GameObject parent)
    {
        if (parent.transform.childCount > 0)
        {
            return parent.transform.GetChild(0).gameObject;
        }

        return null;
    }

    public static GameObject LastChild(this GameObject parent)
    {
        int childCount = parent.transform.childCount;
        if (childCount > 0)
        {
            return parent.transform.GetChild(childCount - 1).gameObject;
        }

        return null;
    }

    public static GameObject InstancePrefab(string prefab, int layer)
    {
        GameObject template = Resources.Load<GameObject>(prefab);
        if (null == template)
        {
            return null;
        }

        GameObject instance = GameObject.Instantiate(template);

        /// 如果设置为负值, 则不修改层
        if (layer >= 0)
        {
            instance.ChangeLayer(layer);
        }

        return instance;
    }

    public static GameObject InstanceGameObject(GameObject source, int layer)
    {
        if (source == null)
        {
            return null;
        }

        GameObject instance = GameObject.Instantiate(source);
        if (null == instance)
        {
            return null;
        }

        /// 如果设置为负值, 则不修改层
        if (layer >= 0)
        {
            instance.ChangeLayer(layer);
        }

        return instance;
    }

    public static GameObject InstanceGameObject(GameObject source)
    {
        return InstanceGameObject(source, -1);
    }

    public static void DestroyGameObject(this GameObject obj)
    {
        if (obj == null)
        {
            return;
        }

#if !UNITY_EDITOR
        GameObject.Destroy(obj);
#else
        if (Application.isPlaying)
        {
            GameObject.Destroy(obj);
        }
        else
        {
            Debug.LogWarningFormat("[{0}] Destroyed in edit mode!", obj.name);
            GameObject.DestroyImmediate(obj);
        }
#endif
    }

    public static void SetActiveSelf(GameObject go, bool isActive)
    {
        if (null != go)
        {
            go.SetActive(isActive);
        }
    }

    public static void SetActiveAllChildGameObject(GameObject parent, bool isActive)
    {
        if (parent == null)
        {
            return;
        }

        int count = parent.transform.childCount;
        for (int i = count - 1; i >= 0; --i)
        {
            Transform childTrans = parent.transform.GetChild(i);
            childTrans.gameObject.SetActive(isActive);
        }
    }

    public static void EditorModeDestroyAllChild(this GameObject parent)
    {
        if (parent == null)
        {
            return;
        }

        int count = parent.transform.childCount;
        for (int i = count - 1; i >= 0; --i)
        {
            GameObject.DestroyImmediate(parent.transform.GetChild(i).gameObject);
        }
    }

    public static void ChangeLayerByName(this GameObject parent, string layerName)
    {
        parent.ChangeLayer(LayerMask.NameToLayer(layerName));
    }

    public static void ChangeLayer(this GameObject parent, int layer)
    {
        if (parent == null)
        {
            return;
        }

        parent.layer = layer;
        for (int i = 0; i < parent.transform.childCount; ++i)
        {
            ChangeLayer(parent.transform.GetChild(i).gameObject, layer);
        }
    }

    private static GameObject _FindChildByPath(GameObject obj, string[] paths, int lv)
    {
        if (lv == paths.Length)
        {
            return obj;
        }

        Transform child = obj.transform.Find(paths[lv]);
        if (child != null)
        {
            return _FindChildByPath(child.gameObject, paths, lv + 1);
        }

        return null;
    }

    public static GameObject FindChildByPath(GameObject obj, string path)
    {
        if (obj == null)
        {
            return null;
        }

        string[] paths = path.Split('/');
        if (paths.Length < 2)
        {
            if (paths.Length == 1 && paths[0].ToLower() == obj.name.ToLower())
            {
                return obj;
            }
            return null;
        }

        return _FindChildByPath(obj, paths, 1);
    }

    public static void SetParticleScale(GameObject obj, float scale)
    {
        if (obj == null)
        {
            return;
        }

        ParticleSystem[] psComponents = obj.GetComponentsInChildren<ParticleSystem>(true);
        for (int i = 0; i < psComponents.Length; ++i)
        {
            ParticleSystem part = psComponents[i];
            part.startSize *= scale;
            part.gravityModifier *= scale;
            part.startSpeed *= scale;
        }
    }

    static public GameObject GetObjectBySceneName(string sceneName, string objName)
    {
        Scene scene = SceneManager.GetSceneByName(sceneName);
        if (!scene.IsValid())
        {
            Debug.LogErrorFormat("GetObjectBySceneName: No Valid Scene [{0}]", sceneName);
            return null;
        }

        GameObject[] rootObjects = scene.GetRootGameObjects();
        foreach (GameObject go in rootObjects)
        {
            if (go.name == objName)
            {
                return go;
            }
        }

        Debug.LogErrorFormat("GetObjectBySceneName: [{0}] of Scene[{1}] NOT FOUND", objName, sceneName);
        return null;
    }


    //修改模型灯光
    public static void SetModelLight(GameObject obj, float lightParam)
    {
        if (null != obj)
        {
            Light light = obj.GetComponent<Light>();
            if (null != light)
            {
                light.intensity = lightParam;
            }
        }
    }
}

