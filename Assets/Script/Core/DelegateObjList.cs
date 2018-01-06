using System;
using System.Collections.Generic;

public delegate void CALLBACK(object[] args);

public class DelegateObjList
{
    public class DynamicDelegate
    {
        public Delegate callback;
        public bool append;
    }
    
    public List<Delegate> events = new List<Delegate>();
    public List<DynamicDelegate> delayProcesList = null;
    public bool accessEvent = false;
    
    private void AddDynamicDelegate(Delegate dele, bool append)
    {
        if (delayProcesList == null)
        {
            delayProcesList = new List<DynamicDelegate>();
        }

        DynamicDelegate dd = new DynamicDelegate
        {
            append = append,
            callback = dele
        };
        delayProcesList.Add(dd);
    }
    
    public void Add(Delegate c)
    {
        if (accessEvent)
        {
            AddDynamicDelegate(c, true);
        }
        else
        {
#if UNITY_EDITOR
            foreach (var d in events)
            {
                if (d == c)
                {
                    Log.Warning("Event ReListen [{0}]", c.ToString());
                }
            }
#endif

            events.Add(c);
        }
    }
    
    public void Remove(Delegate c)
    {
        if (accessEvent)
        {
            AddDynamicDelegate(c, false);
        }
        else
        {
            events.Remove(c);
        }
    }
    
    public void Enter()
    {
        accessEvent = true;
    }
    
    public void Leave()
    {
        accessEvent = false;
        
        if (delayProcesList == null)
        {
            return;
        }
        
        int count = delayProcesList.Count;
        if (count == 0)
        {
            return;
        }
        
        for (int i=0; i<count; ++i)
        {
            var dp = delayProcesList [i];
            if (dp.append)
            {
                events.Add(dp.callback);
            } else
            {
                events.Remove(dp.callback);
            }
        }
        
        delayProcesList.Clear();
    }
}

