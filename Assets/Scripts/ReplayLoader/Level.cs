using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using static System.Collections.Specialized.BitVector32;
using System.IO.Compression;
using Unity.VisualScripting.FullSerializer;
/// <summary>
/// This script must be bound in a button
/// </summary>
public class Level : MonoBehaviour
{
    public class LevelInfo
    {
        public const int SectionLength = 16;
        public const int BlockNumInSections = SectionLength * SectionLength * SectionLength;
        public const int XSectionNum = 16;
        public const int YSectionNum = 24;
        public const int ZSectionNum = 16;
        /// <summary>
        /// "Sections" contains all the sections (3D array)
        /// The length of Section.Blocks array must be equal to 'BlockNumInSections'
        /// </summary>
        public Section[,,] Sections;
        public LevelInfo(Section[,,] sections)
        {
            this.Sections = sections;
        }
    }
    private BlockCreator _blockCreator;
    private LevelInfo _levelInfo;
    private Upload _upload = new() { };
    private Upload.OpenFileName _levelFile = new() { };
    /// <summary>
    /// Get the block name using block id
    /// </summary>
    public string[] BlockNameArray;

    /// <summary>
    /// The block dict <string name, int id>
    /// </summary>
    public Dictionary<string, int> BlockDict;
    private void Start()
    {
        // Initialize the Sections
        this._levelInfo = new LevelInfo(new Section[LevelInfo.XSectionNum, LevelInfo.YSectionNum, LevelInfo.ZSectionNum]);
        // Initialize the BlockCreator
        _blockCreator = GameObject.Find("BlockCreator").GetComponent<BlockCreator>();
        // Initialize the Dict and BlockNameArray
        this.BlockDict = JsonUtility.ParseBlockDictJson("Json/Dict");
        this.BlockNameArray = DictUtility.BlockDictParser(this.BlockDict);
        // Get json file
        var fileLoaded = GameObject.Find("FileLoaded").GetComponent<FileLoaded>();
        // Check if the file is Level json
        this._levelFile = fileLoaded.File;
        if (fileLoaded.Type == FileLoaded.FileType.Level)
        {
            Run();
        }
    }
    private void Run()
    {
        // Check
        if (this._levelFile == null)
        {
            Debug.Log("Loading file error!");
            return;
        }
        LoadBlockData();
        CheckVisibility();
    }
    public void LoadBlockData()
    {
        // Read the json file
        JsonTextReader reader = JsonUtility.UnzipLevel(this._levelFile.File);
        // Process the replay
        JObject jsonObject = (JObject)JToken.ReadFrom(reader);
        // Deal with Sections: array
        JArray sections = (JArray)jsonObject["sections"];

        Debug.Log(sections.ToString());

        for (int i = 0; i < sections.Count; i++)
        {
            // Compute the absolute position of now section / 16
            int sectionX = i / (LevelInfo.ZSectionNum * LevelInfo.YSectionNum);
            int sectionZ = i % LevelInfo.ZSectionNum;
            int sectionY = i % LevelInfo.YSectionNum - sectionZ;

            // All blocks in one section
            Section section = new(new Vector3Int(sectionX, sectionY, sectionZ));

            // jsonSection: array<int blockID>
            JArray jsonSection = (JArray)(sections[i]["blocks"]);
            if (jsonSection.Count != LevelInfo.BlockNumInSections)
            {
                throw new System.Exception($"The length per section is not {LevelInfo.BlockNumInSections}");
            }

            for (int j = 0; j < jsonSection.Count; j++)
            {
                // Compute relative position <The blocks in the section which can be accessed by `blocks[x*256+y*16+z]>
                int x = j / 256, y = j / 16 - x * 16, z = j % 16;
                // BlockID
                section.Blocks[x, y, z].Id = int.Parse(jsonSection[j].ToString());
                // Add name according to _blockNameArray
                section.Blocks[x, y, z].Name = BlockNameArray[section.Blocks[x, y, z].Id];
                // Compute absolute position
                section.Blocks[x, y, z].Position = new Vector3Int(sectionX + x, sectionY + y, sectionZ + z);
            }
            this._levelInfo.Sections[sectionX, sectionY, sectionZ] = section;
        }
    }
    public void CheckVisibility()
    {
        this.CheckInnerVisibility();
        this.CheckNeighbourVisibility();
    }
    /// <summary>
    /// Get the section index by using x,y,z index
    /// </summary>
    /// <param name="xIndex"> The x index of section </param>
    /// <param name="yIndex"> The y index of section </param>
    /// <param name="zIndex"> The z index of section </param>
    /// <returns></returns>
    private int GetSectionIndex(int xIndex, int yIndex, int zIndex)
    {
        return xIndex * LevelInfo.YSectionNum * LevelInfo.ZSectionNum + yIndex * LevelInfo.ZSectionNum + zIndex;
    }
    /// <summary>
    /// Check the visibility in all the sections
    /// </summary>
    /// <returns></returns>
    private void CheckInnerVisibility()
    {
        int airId = this.BlockDict["Air"];
        // sx: section x / 16 , sy: section y / 16 , sz: section z / 16
        for (int sx = 0; sx < this._levelInfo.Sections.GetLength(0); sx++)
        {
            for (int sy = 0; sy < this._levelInfo.Sections.GetLength(1); sy++)
            {
                for (int sz = 0; sz < this._levelInfo.Sections.GetLength(2); sz++)
                {
                    // Check visibility in the section
                    Section nowSection = this._levelInfo.Sections[sx, sy, sz];
                    // bx: block x , by: block y , bz: block z (relative position to nowSection)
                    // Regardless of the edge which will be computed in function "CheckNeighbourVisibility"
                    for (int bx = 1; bx < nowSection.Blocks.GetLength(0) - 1; bx++)
                    {
                        for (int by = 1; by < nowSection.Blocks.GetLength(1) - 1; by++)
                        {
                            for (int bz = 1; bz < nowSection.Blocks.GetLength(2) - 1; bz++)
                            {
                                // If the block is visible, create it at once
                                Block nowBlock = nowSection.Blocks[bx, by, bz];

                                if (nowSection.Blocks[bx - 1, by, bz].Id == airId ||
                                    nowSection.Blocks[bx + 1, by, bz].Id == airId ||
                                    nowSection.Blocks[bx, by - 1, bz].Id == airId ||
                                    nowSection.Blocks[bx, by + 1, bz].Id == airId ||
                                    nowSection.Blocks[bx, by, bz - 1].Id == airId ||
                                    nowSection.Blocks[bx, by, bz + 1].Id == airId)
                                {
                                    this._blockCreator.CreateBlock(nowBlock);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    /// <summary>
    /// Check the visibility between all the sections
    /// </summary>
    /// <returns></returns>
    private void CheckNeighbourVisibility()
    {
        for (int i = 0; i < this._levelInfo.Sections.GetLength(0); i++)
        {
            for (int j = 0; j < this._levelInfo.Sections.GetLength(1); j++)
            {
                for (int k = 0; k < this._levelInfo.Sections.GetLength(2); k++)
                {
                    // Check visibility between the section
                }
            }
        }
    }
}
