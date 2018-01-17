using System;
using System.IO;
using ProtoBuf;
using UnityEngine;

public class CmdBase
{
    protected static object[] eventParams = new object[10];
	public static byte[] ProtoBufSerialize<T> (T data)
	{
		MemoryStream stream = new MemoryStream (1280);
		try {
			Serializer.Serialize<T> (stream, data);
		} catch (Exception e) {
			Debug.LogError ("ProtoBuf Pack Error:" + e.ToString ());
			return null;
		}

		byte[] result = stream.ToArray ();

		return result;
	}

	public static T ProtoBufDeserialize<T> (byte[] data)
	{
        return ProtoBufDeserializeEx<T>(data, data.Length) ;
	}

	public static T ProtoBufDeserializeEx<T> (byte[] data, int len)
	{
		T result;
		MemoryStream stream = new MemoryStream (data, 0, len);
		try {
			result = Serializer.Deserialize<T> (stream);
		} catch (Exception e) {
			Debug.LogError ("ProtoBuf UnPack Error:" + e.ToString () + ", data len=" + len);
			return default(T);
		}

		return result;
	}

	public static bool UnpackProto<T> (byte[] data, out T t)
	{
		MemoryStream stream = new MemoryStream (data, 0, data.Length);
		bool succ = false;
		try {
			t = Serializer.Deserialize<T> (stream);
			succ = true;
		} catch (Exception e) {
			t = default(T);
			Debug.LogError ("ProtoBuf UnPack Error:" + e.ToString () + ", data len=" + data.Length);
		}
		return succ;
	}
}
