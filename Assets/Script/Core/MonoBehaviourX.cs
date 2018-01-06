using UnityEngine;
using System.Collections;

public class MonoBehaviourX : MonoBehaviour
{
    protected EventScope scope = Eventer.Create();
    
    protected void DestroyScope()
    {
        scope.Destroy();
        scope = null;
    }

    private void OnDestroy()
    {
        DestroyScope();
    }

    protected void _checkOwner(string ownerKeywords)
    {
        if (!gameObject.name.Contains(ownerKeywords))
        {
            Destroy(this);

            Log.Warning(
                "[{0}]组件只属于[{1}],不能出现在[{2}]对象上!", 
                GetType().Name, ownerKeywords, gameObject.name);
        }
    }
}

