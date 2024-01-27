using System.IO;
using UnityEngine;

public class Utils
{
	#region JSON

	public static T GetJson<T>(string fileName)
	{
		var fileAddress = GetJsonAddress(fileName);

		if(HasJson(fileAddress) == false)
		{
			Debug.LogError($"File is NULL!!\n{fileAddress}");
			return default(T);
		}

		var load = ReadAllText(fileAddress);
		return JsonUtility.FromJson<T>(load);
	}

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
