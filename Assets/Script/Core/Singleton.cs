using System;
using UnityEngine;

public class ZBSingleton<T> where T : new()
{
    private static T instance;
    
    public static T Ins
    {
        get
        {
            if (ZBSingleton<T>.instance == null)
            {
                ZBSingleton<T>.instance = ((default(T) == null) ? Activator.CreateInstance<T>() : default(T));
            }

            return ZBSingleton<T>.instance;
        }
    }
}
