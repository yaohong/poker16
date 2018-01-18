using UnityEngine;
using System.Collections;
using System;

public class Log
{
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogYY(string str)
    {
		Debug.Log(str);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Logic(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Battle(string format, params object[] args)
    {
        //Warning(format, args);
    }

    public static void Error(string format, params object[] args)
    {
        Debug.LogErrorFormat(format, args);
    }

    public static void Error(UnityEngine.Object context, string format, params object[] args)
    {
        Debug.LogErrorFormat(context, format, args);
    }

    public static void Warning(string format, params object[] args)
    {
        Debug.LogWarningFormat(format, args);
    }

    public static void Warning(UnityEngine.Object context, string format, params object[] args)
    {
        Debug.LogWarningFormat(context, format, args);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Network(string format, params object[] args)
    {
		format = string.Format("[Net:{0}]:{1}", timestamp, format);
		Debug.LogWarningFormat(format, args);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void Load(string format, params object[] args)
    {
		format = string.Format("[Load:{0}]:{1}", timestamp, format);
		Debug.LogWarningFormat(format, args);
    }

    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void ImLog(string format, params object[] args)
    {
        Debug.LogFormat(format, args);
    }
		


    
    [System.Diagnostics.Conditional("UNITY_EDITOR")]
    public static void LogByte(byte[] buff, int len)
    {
        string str = "BUFF, len=" + len.ToString() + ", data=";
        int logSize = Mathf.Min(len, 64);
        for (int i = 0; i < logSize; ++i)
        {
            str += string.Format("{0},", buff[i]);
        }
        Debug.Log(str);
    }

    public static void TraceBack()
    {
        try
        {
            throw new Exception("CustomTraceBack");
        }
        catch (Exception e)
        {
            Debug.LogError(e.Message + e.StackTrace);
        }
    }

    public static string timestamp
    {
        get
        {
            return TimeMgr.Ins.NowServerDate;
        }
    }
}
