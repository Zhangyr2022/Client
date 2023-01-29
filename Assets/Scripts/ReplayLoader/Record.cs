using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
/// <summary>
/// This script must be bound in a button
/// </summary>
public class Record : MonoBehaviour
{
    public class RecordInfo
    {

    }

    private Upload _upload = new() { };
    private Upload.OpenFileName _replayFile = new() { };
    private void Start()
    {
        this.GetComponent<Button>().onClick.AddListener(Clicked);
    }
    private void Clicked()
    {
        // Get json file
        this._replayFile = this._upload.UploadDat();
        // Check
        if (this._replayFile == null)
        {
            return;
        }

        // Read the json file
        JsonTextReader reader = ReadJsonFile(this._replayFile);
        // Process the replay
        JObject jsonObject = (JObject)JToken.ReadFrom(reader);
        // Deal with Sections: array
        Debug.Log(jsonObject["section"].ToString());
    }
    /// <summary>
    /// Parse the json file
    /// </summary>
    /// <param name="fileInfo">The file from class Upload.OpenFileName</param>
    /// <returns></returns>
    public JsonTextReader ReadJsonFile(Upload.OpenFileName fileInfo)
    {
        System.IO.StreamReader file = System.IO.File.OpenText(this._replayFile.File);
        return new JsonTextReader(file);
    }

}
