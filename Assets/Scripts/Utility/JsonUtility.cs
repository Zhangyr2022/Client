using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using UnityEngine;
using System.Linq;
using Newtonsoft.Json.Linq;
public class JsonUtility
{
    /// <summary>
    /// Unzip the level_data.dat
    /// </summary>
    /// <param name="levelDataZipArchive"></param>
    /// <exception cref="Exception"></exception>
    public static JObject UnzipLevel(string path)
    {
        ZipArchive mcLevelDataZipFile = ZipFile.OpenRead(path);
        Stream mcLevelDataEntryStream = mcLevelDataZipFile.GetEntry("level.dat").Open() ??
         throw new Exception("mcLevel data not found in zip archive.");

        ZipArchive levelDataZipFile = new ZipArchive(mcLevelDataEntryStream);
        Stream levelDataEntryStream = levelDataZipFile.GetEntry("level_data.json").Open() ??
         throw new Exception("Level data not found in zip archive.");

        // Read the level data to a JSON string.
        StreamReader levelDataEntryStreamReader = new StreamReader(levelDataEntryStream);

        //Debug.Log(levelDataEntryStreamReader.ReadToEnd());

        return (JObject)JToken.ReadFrom(new JsonTextReader(levelDataEntryStreamReader));
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="path"></param>
    /// <returns></returns>
    /// <exception cref="Exception"></exception>
    public static JObject UnzipRecord(string path)
    {
        //string unityPath = Application.dataPath + "/";
        //ZipFile.ExtractToDirectory(path, $"{path}.unzip");
        ZipArchive mcLevelDataZipFile = ZipFile.OpenRead(path);

        Debug.Log($"entry count: {mcLevelDataZipFile.Entries.Count}");
        //ZipArchiveEntry mcLevelDataEntryStream = mcLevelDataZipFile.GetEntry("records") ??
        // throw new Exception("mcLevel data not found in zip archive.");

        // Load all the record entry
        List<JObject> allRecordJsonObject = new();

        foreach (ZipArchiveEntry recordEntry in mcLevelDataZipFile.Entries)
        {
            // If the recordEntry is not folder and not level
            if (!recordEntry.FullName.Contains("level") && !recordEntry.FullName.EndsWith("/"))
            {
                Stream recordEntryStream = recordEntry.Open();
                // Unzip the record
                ZipArchive recordZipArchive = new(recordEntryStream);

                StreamReader recordStreamReader = new(recordZipArchive.Entries[0].Open());
                allRecordJsonObject.Add((JObject)JToken.ReadFrom(new JsonTextReader(recordStreamReader)));
                Debug.Log(recordStreamReader.ReadToEnd().ToString());
            }
        }

        if (allRecordJsonObject.Count == 0)
            throw new Exception("Record data not found in zip archive.");

        // Compute the first tick
        // pair<int index, int tick>
        (int, int)[] indexAndTicks = new (int, int)[allRecordJsonObject.Count];
        int nowRecordIndex = 0;
        foreach (JObject jsonObject in allRecordJsonObject)
        {
            indexAndTicks[nowRecordIndex].Item1 = nowRecordIndex;
            // If the record file is wrong, then the record will not be added. So let initial tick equal to -1
            indexAndTicks[nowRecordIndex].Item2 = -1;
            //ZipFile.OpenRead(recordDataEntry.FullName);

            // Find the first tick;
            JArray records = (JArray)jsonObject["records"];
            if (records != null && records.Count > 0)
            {
                JValue tick = (JValue)records[0]["ticks"];
                if (tick != null)
                {
                    // The first tick
                    indexAndTicks[nowRecordIndex].Item2 = int.Parse(tick.ToString());
                }
            }
            nowRecordIndex++;
        }
        // Rearrange the order of record file according to their first ticks
        List<(int, int)> indexAndTicksList = indexAndTicks.ToList<(int, int)>();
        indexAndTicksList.Sort((x, y) => x.Item2.CompareTo(y.Item2));

        // Write the json obj according to the order
        JObject recordJsonObject = new()
        {
            {"type","record" },
            { "records", new JArray() }
        };

        foreach ((int, int) indexAndTick in indexAndTicksList)
        {
            if (indexAndTick.Item2 != -1)
            {
                // Serial number in allRecordJsonObject: indexAndTick.Item1
                JObject jsonObject = allRecordJsonObject[indexAndTick.Item1];
                JArray records = (JArray)jsonObject["records"];

                // Append
                ((JArray)recordJsonObject["records"]).Merge(records);
            }
        }
        return recordJsonObject;

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
