using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

public class FileDataHandler
{
    private readonly string dataDirPath = string.Empty;
    private readonly string dataFileName = string.Empty;

    public FileDataHandler(string dataDirPath, string dataFileName)
    {
        this.dataDirPath = dataDirPath;
        this.dataFileName = dataFileName;
    }
    public GameData Load()
    {
        string fullpath = Path.Combine(dataDirPath, dataFileName);
        GameData loadedData = null;
        if (File.Exists(fullpath))
        {
            try
            {
                string dataToLoad = "";
                using FileStream stream = new(fullpath, FileMode.Open);
                using StreamReader reader = new(stream);
                dataToLoad = reader.ReadToEnd();
                loadedData = JsonUtility.FromJson<GameData>(dataToLoad);
            } catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
        return loadedData;
    }

    public void Save(GameData data)
    {
        string fullpath = Path.Combine(dataDirPath, dataFileName);
        try
        {
            Directory.CreateDirectory(Path.GetDirectoryName(fullpath));
            string dataToStore = JsonUtility.ToJson(data, true);
            using FileStream stream = new(fullpath, FileMode.Create);
            using StreamWriter writer = new(stream);
            writer.Write(dataToStore);
        } catch (Exception e)
        {
            Debug.LogException(e);
        }
    }

}
