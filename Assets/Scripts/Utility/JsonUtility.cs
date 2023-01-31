using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;
using System.Linq;

public class JsonUtility
{
    /// <summary>
    /// Unzip the level_data.dat
    /// </summary>
    /// <param name="levelDataZipArchive"></param>
    /// <exception cref="Exception"></exception>
    public static JsonTextReader UnzipLevel(string path)
    {
        ZipArchive levelDataZipFile = ZipFile.OpenRead(path);
        ZipArchiveEntry levelDataEntry = levelDataZipFile.GetEntry("level_data.json") ??
         throw new Exception("Level data not found in zip archive.");

        // Read the level data to a JSON string.
        Stream levelDataEntryStream = levelDataEntry.Open();
        StreamReader levelDataEntryStreamReader = new StreamReader(levelDataEntryStream);
        //Debug.Log(levelDataEntryStreamReader.ReadToEnd());
        return new JsonTextReader(levelDataEntryStreamReader);
    }
    /// <summary>
    /// Parse the json file
    /// </summary>
    /// <param name="fileInfo">The file from class Upload.OpenFileName</param>
    /// <returns></returns>
    public static JsonTextReader ReadJsonFile(Upload.OpenFileName fileInfo)
    {
        System.IO.StreamReader file = System.IO.File.OpenText(fileInfo.File);
        return new JsonTextReader(file);
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="jsonPath">which is inner the resource folder</param>
    /// <returns></returns>
    public static Dictionary<string, int> ParseBlockDictJson(string jsonPath)
    {
        // "Json/Dict"
        TextAsset text = Resources.Load(jsonPath) as TextAsset;
        string json = text.text;
        if (string.IsNullOrEmpty(json))
        {
            return null;
        }
        else
        {
            Dictionary<string, int> dict = JsonConvert.DeserializeObject<Dictionary<string, int>>(json);
            // Delete the prefix "minecraft:" in keys
            string prefix = "minecraft:";

            string ReplaceKey(string key, int prefixIndex)
            {
                key = key.Substring(prefixIndex + prefix.Length);
                // Capitalize the name 
                key = key[..1].ToUpper() + key[1..];
                return key;
            };

            dict = dict.ToDictionary(dictItem => dictItem.Key.IndexOf(prefix) == -1 ?
                dictItem.Key : ReplaceKey(dictItem.Key, dictItem.Key.IndexOf(prefix)),
                dictItem => dictItem.Value); 

            return dict;
        }
    }
}
