using System;
using System.IO;
using UnityEngine;

namespace Framework.Storage
{
    public static class AccountManager
    {
        public static string GetDefaultFilePath<T>(string customName = null)
        {
            var fileName = string.IsNullOrEmpty(customName) ? typeof(T).Name + ".json" : customName;
            return Path.Combine(Application.persistentDataPath, fileName);
        }

        public static void Save<T>(T data, string customFileName = null)
        {
            try
            {
                var path = GetDefaultFilePath<T>(customFileName);
                var json = JsonUtility.ToJson(data, true);
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, json);
                Debug.Log($"[AccountManager] Saved {typeof(T).Name} -> {path}");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AccountManager] Save error: {ex}");
            }
        }

        public static bool TryLoad<T>(out T data, string customFileName = null) where T : class
        {
            data = null;
            try
            {
                var path = GetDefaultFilePath<T>(customFileName);
                if (!File.Exists(path)) return false;
                var json = File.ReadAllText(path);
                data = JsonUtility.FromJson<T>(json);
                Debug.Log($"[AccountManager] Loaded {typeof(T).Name} <- {path}");
                return data != null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"[AccountManager] Load error: {ex}");
                return false;
            }
        }

        public static T LoadOrCreate<T>(string customFileName = null) where T : class, new()
        {
            if (TryLoad<T>(out var data, customFileName)) return data;
            return new T();
        }
    }
}
