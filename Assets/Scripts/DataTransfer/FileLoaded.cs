using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Transfer the file between Scene "Menu" and "Record"
/// </summary>
public class FileLoaded : MonoBehaviour
{
    public enum FileType
    {
        None,
        Level,
        Record
    };

    public Upload.OpenFileName File { get; set; }
    public FileType Type = FileLoaded.FileType.Level;
    /// <summary>
    /// The obj would not be destroyed to transfer the data
    /// </summary>
    void Awake()
    {
        DontDestroyOnLoad(this.transform.gameObject);
    }
}
