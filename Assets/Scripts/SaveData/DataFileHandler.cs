using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class DataFileHandler
{
    private string filePath;
    private string fileName;

    public DataFileHandler(string _filePath, string _fileName)
    {
        this.filePath = _filePath;
        this.fileName = _fileName;
    }
    
    public T LoadData<T>()
    {
        string fullPath = Path.Combine(filePath, fileName);
        if (File.Exists(fullPath))
        {
            string jsonData = File.ReadAllText(fullPath);
            T data = JsonUtility.FromJson<T>(jsonData);
            return data;
        }
        else
        {
            Debug.LogWarning($"File not found at path: {fullPath}");
            return default;
        }
    }

    public void SaveData<T>(T _data)
    {
        string fullPath = Path.Combine(filePath, fileName);
        string jsonData = JsonUtility.ToJson(_data, true);
        File.WriteAllText(fullPath, jsonData);
    }

    public void DeleteSavedData()
    {
        string fullPath = Path.Combine(filePath, fileName);
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
            Debug.Log($"File deleted at path: {fullPath}");
        }
        else
        {
            Debug.LogWarning($"File not found at path: {fullPath}");
        }
    }
}
