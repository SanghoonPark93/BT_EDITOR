using System.IO;
using UnityEngine;

public class Utils
{
	#region JsonMethod
	public static string GetJsonAddress(string fileName)
	{
		return $"{Application.dataPath}/{fileName}.json";
	}

	public static bool HasJson(string fileAddress)
	{
		return new FileInfo(fileAddress).Exists;
	}

	public static string ReadAllText(string fileAddress)
	{
		return File.ReadAllText(fileAddress);
	}

	public static void WriteAllText(string fileAddress, string json)
	{
		File.WriteAllText(fileAddress, json);
	}
	#endregion
}
