using System;
using System.Collections.Generic;

public class EventScope
{
    public Dictionary<string, DelegateObjList> eventTable = new Dictionary<string, DelegateObjList>();
    private EventScope parent = null;
    private List<EventScope> children = new List<EventScope>();
    
    public EventScope(EventScope _parent)
    {
        parent = _parent;
    }
    
    public EventScope CreateChild()
    {
        EventScope scope = new EventScope(this);
        children.Add(scope);

        return scope;
    }
    
    private void RemoveParentEvents(string name, Delegate deleObject)
    {
        if (parent == null)
        {
            return;
        }
        
        DelegateObjList list;
        if (parent.eventTable.TryGetValue(name, out list))
        {
            list.Remove(deleObject);
        }
        
        parent.RemoveParentEvents(name, deleObject);
    }
    
    public void ClearEvent()
    {
        foreach (var et in eventTable)
        {
            foreach (Delegate _delegate in et.Value.events)
            {
                RemoveParentEvents(et.Key, _delegate);
            }
        }
        
        eventTable.Clear();
    }
    
    public void Destroy()
    {
        ClearEvent();
        
        if (parent != null)
        {
            parent.children.Remove(this);
        }
    }
    
    public void Listen(string name, CALLBACK handler)
    {
        if (handler == null)
        {
            return;
        }

        DelegateObjList dol;
        if (!eventTable.TryGetValue(name, out dol))
        {
            dol = new DelegateObjList();
            eventTable.Add(name, dol);
        }
        
        dol.Add(handler);
        
        if (parent != null)
        {
            parent.Listen(name, handler);
        }
    }
}



