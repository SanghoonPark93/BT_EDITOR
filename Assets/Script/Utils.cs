using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace BT.Util
{
    public static class Utils
    {
        private static readonly Dictionary<string, Type> BtTypes = new Dictionary<string, Type>();

        public static Type GetBTType(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName)) return null;
            if (!BtTypes.TryGetValue(typeName, out var type))
            {
                type = Type.GetType(typeName) ?? typeof(Node).Assembly.GetType(typeName)
                    ?? typeof(Node).Assembly.GetType($"BT.{typeName}");
                if (type != null) BtTypes.Add(typeName, type);
            }
            return type;
        }

        public static T GetJson<T>(string fileName)
        {
            var path = GetJsonAddress(fileName);
            if (!File.Exists(path))
            {
                Debug.LogError($"File is missing: {path}");
                return default(T);
            }
            return JsonUtility.FromJson<T>(File.ReadAllText(path));
        }

        public static string GetJsonAddress(string fileName) =>
            Path.Combine(Application.dataPath, fileName + ".json");

        public static bool HasJson(string path) => File.Exists(path);
        public static string ReadAllText(string path) => File.ReadAllText(path);
        public static void WriteAllText(string path, string json) => File.WriteAllText(path, json);

        public static void EditorLog(string log)
        {
#if UNITY_EDITOR
            Debug.Log(log);
#endif
        }
    }
}