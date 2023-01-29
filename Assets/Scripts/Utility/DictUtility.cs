using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DictUtility
{
    /// <summary>
    /// Reverse the dict (<string, int> => <int, string>)
    /// </summary>
    /// <param name="blockDict"></param>
    /// <returns></returns>
    public static string[] BlockDictParser(Dictionary<string, int> blockDict)
    {
        string[] blockArray = new string[blockDict.Count];
        foreach (var blockInfo in blockDict)
        {
            blockArray[blockInfo.Value] = blockInfo.Key;
        }
        // Delete the prefix "minecraft:"
        for (int i = 0; i < blockArray.Length; i++)
        {
            string prefix = "minecraft:";
            int prefixIndex = blockArray[i].IndexOf(prefix);

            if (prefixIndex != -1)
            {
                blockArray[i] = blockArray[i].Substring(prefixIndex + prefix.Length);
            }
            // Capitalize the name 
            blockArray[i] = blockArray[i][..1].ToUpper() + blockArray[i][1..];
        }
        Debug.Log(blockArray);
        return blockArray;
    }
}
