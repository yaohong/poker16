using System;
using System.Collections.Generic;

public class Eventer
{
    static public EventScope globe = new EventScope(null);

    static public EventScope Create()
    {
        return globe.CreateChild();
    }

    public static void Fire(string name, object[] args)
    {
        DelegateObjList dol;
        if (globe.eventTable.TryGetValue(name, out dol))
        {
            dol.Enter();
            int count = dol.events.Count;
            for (int i = 0; i < count; ++i)
            {
                CALLBACK callback = dol.events[i] as CALLBACK;
                callback(args);
            }
            dol.Leave();
        }
    }

    public static void Fire(string name)
    {
        DelegateObjList dol;
        object[] empty = new object[0];
        if (globe.eventTable.TryGetValue(name, out dol))
        {
            dol.Enter();
            int count = dol.events.Count;
            for (int i = 0; i < count; ++i)
            {
                CALLBACK callback = dol.events[i] as CALLBACK;
                callback(empty);
            }
            dol.Leave();
        }
    }

    //  ---------------- 以下接口提供给Lua调用 ----------------------
    public static void _Fire0(string name)
    {
        object[] args = new object[0] { };
        Fire(name, args);
    }

    public static void _Fire1(string name, object arg1)
    {
        object[] args = new object[1] { arg1 };
        Fire(name, args);
    }

    public static void _Fire2(string name, object arg1, object arg2)
    {
        object[] args = new object[2] { arg1, arg2 };
        Fire(name, args);
    }

    public static void _Fire3(string name, object arg1, object arg2, object arg3)
    {
        object[] args = new object[3] { arg1, arg2, arg3 };
        Fire(name, args);
    }

    public static void _Fire4(string name, object arg1, object arg2, object arg3, object arg4)
    {
        object[] args = new object[4] { arg1, arg2, arg3, arg4 };
        Fire(name, args);
    }

    public static void _Fire5(string name, object arg1, object arg2, object arg3, object arg4, object arg5)
    {
        object[] args = new object[5] { arg1, arg2, arg3, arg4, arg5 };
        Fire(name, args);
    }

    public static void _Fire6(string name, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6)
    {
        object[] args = new object[6] { arg1, arg2, arg3, arg4, arg5, arg6 };
        Fire(name, args);
    }

    public static void _Fire7(string name, object arg1, object arg2, object arg3, object arg4, object arg5, object arg6, object arg7)
    {
        object[] args = new object[7] { arg1, arg2, arg3, arg4, arg5, arg6, arg7 };
        Fire(name, args);
    }
}
