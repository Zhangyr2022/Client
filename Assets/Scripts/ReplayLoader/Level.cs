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
    }
    private BlockCreator _blockCreator;
    private LevelInfo _levelInfo;
    private Upload _upload = new() { };
    private Upload.OpenFileName _levelFile = new() { };


    /// <summary>
    /// Get the private _levelInfo
    /// </summary>
    public LevelInfo LevelInformation
    {
        get { return _levelInfo; }
    }
    private void Start()
    {
        // Initialize the _recordInfo
        this._levelInfo = new();
        // Initialize the BlockCreator
        this._blockCreator = GameObject.Find("BlockCreator").GetComponent<BlockCreator>();
        // Get json file
        var fileLoaded = GameObject.Find("FileLoaded").GetComponent<FileLoaded>();
        // Check if the file is Level json
        this._levelFile = fileLoaded.File;

        Run();
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
        // Read the json file and Process the replay
        JObject jsonObject = (JsonUtility.UnzipLevel(this._levelFile.File));
        // Deal with Sections: array
        JArray sections = (JArray)jsonObject["sections"];
        Debug.Log(sections.Count);

        for (int i = 0; i < sections.Count; i++)
        {
            // Get the absolute position of now section
            int sectionX = int.Parse(sections[i]["x"].ToString());
            int sectionY = int.Parse(sections[i]["y"].ToString());
            int sectionZ = int.Parse(sections[i]["z"].ToString());

            // All blocks in one section
            Section section = new(new Vector3Int(sectionX, sectionY, sectionZ) / LevelInfo.SectionLength);

            // jsonSection: array<int blockID>
            JArray jsonSection = (JArray)(sections[i]["blocks"]);
            if (jsonSection.Count != LevelInfo.BlockNumInSections)
            {
                throw new System.Exception($"The length per section is not {LevelInfo.BlockNumInSections}");
            }

            for (int j = 0; j < jsonSection.Count; j++)
            {
                // Compute relative position <The blocks in the section which can be accessed by `blocks[x*256+y*16+z]>
                int x = j / 256;
                int y = j / 16 - x * 16;
                int z = j % 16;
                // Initialize the block
                section.Blocks[x, y, z] = new Block();
                // BlockID
                section.Blocks[x, y, z].Id = short.Parse(jsonSection[j].ToString());
                // Add name according to _blockNameArray
                try
                {
                    section.Blocks[x, y, z].Name = BlockDicts.BlockNameArray[section.Blocks[x, y, z].Id];
                }
                catch
                {
                    Debug.Log(BlockDicts.BlockNameArray);
                    Debug.Log(section.Blocks[x, y, z].Id);
                    section.Blocks[x, y, z].Name = BlockDicts.BlockNameArray[0];
                    section.Blocks[x, y, z].Id = 0;
                }
                // Compute absolute position
                section.Blocks[x, y, z].Position = new Vector3Int(sectionX + x, sectionY + y, sectionZ + z);
            }
            //try
            //{
            BlockSource.AddSection(section);
            //}
            //catch
            //{
            //    Debug.Log(i);
            //    Debug.Log(new Vector3Int(sectionX, sectionY, sectionZ));
            //}
        }
        //Debug.Log($"Section num: {this._levelInfo.AllBlockSource.SectionDict.Count}");
    }
    public void CheckVisibility()
    {
        this.CheckInnerVisibility();
        this.CheckNeighbourVisibility();
    }

    /// <summary>
    /// Check the visibility in all the sections
    /// </summary>
    /// <returns></returns>
    private void CheckInnerVisibility()
    {
        int airId = BlockDicts.BlockDict["Air"];
        foreach (var blockSourceItem in BlockSource.SectionDict)
        {
            // Check visibility in the section
            Section nowSection = blockSourceItem.Value;

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
    /// <summary>
    /// Check the visibility between all the sections
    /// </summary>
    /// <returns></returns>
    private void CheckNeighbourVisibility()
    {
        int airId = BlockDicts.BlockDict["Air"];

        int sectionSmallEdge = 0, sectionLargeEdge = LevelInfo.SectionLength - 1;
        foreach (var blockSourceItem in BlockSource.SectionDict)
        {
            // Check visibility on the surface of each section
            Section nowSection = blockSourceItem.Value;
            Vector3Int nowSectionPosition = blockSourceItem.Key * LevelInfo.SectionLength;

            // bx: block x , by: block y , bz: block z (relative position to nowSection)

            // X small edge in the section
            for (int by = 0; by <= sectionLargeEdge; by++)
            {
                for (int bz = 0; bz <= sectionLargeEdge; bz++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x - 1,
                        nowSectionPosition.y + by,
                        nowSectionPosition.z + bz);

                    Block xLastSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the X Last Section 
                    if (nowSection.Blocks[sectionSmallEdge + 1, by, bz].Id == airId ||  // Xnext is air

                        (xLastSectionBlock != null && xLastSectionBlock.Id == airId) || // Block in the X Last Section is air

                        (by > 0 && nowSection.Blocks[sectionSmallEdge, by - 1, bz].Id == airId) ||
                        (by < sectionLargeEdge && nowSection.Blocks[sectionSmallEdge, by + 1, bz].Id == airId) ||
                        (bz > 0 && nowSection.Blocks[sectionSmallEdge, by, bz - 1].Id == airId) ||
                        (bz < sectionLargeEdge && nowSection.Blocks[sectionSmallEdge, by, bz + 1].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[sectionSmallEdge, by, bz];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
            // X large edge in the section
            for (int by = 0; by <= sectionLargeEdge; by++)
            {
                for (int bz = 0; bz <= sectionLargeEdge; bz++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x + LevelInfo.SectionLength,
                        nowSectionPosition.y + by,
                        nowSectionPosition.z + bz);

                    Block xNextSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the X Next Section 
                    if (nowSection.Blocks[sectionLargeEdge - 1, by, bz].Id == airId ||  // Xlast is air

                        (xNextSectionBlock != null && xNextSectionBlock.Id == airId) || // block in the X Next Section is air

                        (by > 0 && nowSection.Blocks[sectionLargeEdge, by - 1, bz].Id == airId) ||
                        (by < sectionLargeEdge && nowSection.Blocks[sectionLargeEdge, by + 1, bz].Id == airId) ||
                        (bz > 0 && nowSection.Blocks[sectionLargeEdge, by, bz - 1].Id == airId) ||
                        (bz < sectionLargeEdge && nowSection.Blocks[sectionLargeEdge, by, bz + 1].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[sectionLargeEdge, by, bz];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
            //------------------------------------------------//
            // Y small edge in the section
            for (int bx = 0; bx <= sectionLargeEdge; bx++)
            {
                for (int bz = 0; bz <= sectionLargeEdge; bz++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x + bx,
                        nowSectionPosition.y - 1,
                        nowSectionPosition.z + bz);

                    Block yLastSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the Y last Section 
                    if (nowSection.Blocks[bx, sectionSmallEdge + 1, bz].Id == airId ||  // Ynext is air

                        (yLastSectionBlock != null && yLastSectionBlock.Id == airId) || // block in the Y Last Section is air

                        (bx > 0 && nowSection.Blocks[bx - 1, sectionSmallEdge, bz].Id == airId) ||
                        (bx < sectionLargeEdge && nowSection.Blocks[bx + 1, sectionSmallEdge, bz].Id == airId) ||
                        (bz > 0 && nowSection.Blocks[bx, sectionSmallEdge, bz - 1].Id == airId) ||
                        (bz < sectionLargeEdge && nowSection.Blocks[bx, sectionSmallEdge, bz + 1].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[bx, sectionSmallEdge, bz];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
            // Y large edge in the section
            for (int bx = 0; bx <= sectionLargeEdge; bx++)
            {
                for (int bz = 0; bz <= sectionLargeEdge; bz++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x + bx,
                        nowSectionPosition.y + LevelInfo.SectionLength,
                        nowSectionPosition.z + bz);

                    Block yNextSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the Y next Section 
                    if (nowSection.Blocks[bx, sectionLargeEdge - 1, bz].Id == airId ||  // Ylast is air

                        (yNextSectionBlock != null && yNextSectionBlock.Id == airId) || // block in the X Next Section is air

                        (bx > 0 && nowSection.Blocks[bx - 1, sectionLargeEdge, bz].Id == airId) ||
                        (bx < sectionLargeEdge && nowSection.Blocks[bx + 1, sectionLargeEdge, bz].Id == airId) ||
                        (bz > 0 && nowSection.Blocks[bx, sectionLargeEdge, bz - 1].Id == airId) ||
                        (bz < sectionLargeEdge && nowSection.Blocks[bx, sectionLargeEdge, bz + 1].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[bx, sectionLargeEdge, bz];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
            //------------------------------------------------//
            // Z small edge in the section
            for (int bx = 0; bx <= sectionLargeEdge; bx++)
            {
                for (int by = 0; by <= sectionLargeEdge; by++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x + bx,
                        nowSectionPosition.y + by,
                        nowSectionPosition.z - 1);

                    Block zLastSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the Z last Section 
                    if (nowSection.Blocks[bx, by, sectionSmallEdge + 1].Id == airId ||  // Znext is air

                        (zLastSectionBlock != null && zLastSectionBlock.Id == airId) || // Block in the Z Last Section is air

                        (bx > 0 && nowSection.Blocks[bx - 1, by, sectionSmallEdge].Id == airId) ||
                        (bx < sectionLargeEdge && nowSection.Blocks[bx + 1, by, sectionSmallEdge].Id == airId) ||
                        (by > 0 && nowSection.Blocks[bx, by - 1, sectionSmallEdge].Id == airId) ||
                        (by < sectionLargeEdge && nowSection.Blocks[bx, by + 1, sectionSmallEdge].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[bx, by, sectionSmallEdge];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
            // Z large edge in the section
            for (int bx = 0; bx <= sectionLargeEdge; bx++)
            {
                for (int by = 0; by <= sectionLargeEdge; by++)
                {
                    Vector3Int absolutePosition = new Vector3Int(
                        nowSectionPosition.x + bx,
                        nowSectionPosition.y + by,
                        nowSectionPosition.z + LevelInfo.SectionLength);

                    Block zNextSectionBlock = BlockSource.GetBlock(absolutePosition);// Block in the Z next Section 
                    if (nowSection.Blocks[bx, by, sectionLargeEdge - 1].Id == airId ||  // Zlast is air

                        (zNextSectionBlock != null && zNextSectionBlock.Id == airId) || // Block in the Z Next Section is air

                        (bx > 0 && nowSection.Blocks[bx - 1, by, sectionLargeEdge].Id == airId) ||
                        (bx < sectionLargeEdge && nowSection.Blocks[bx + 1, by, sectionLargeEdge].Id == airId) ||
                        (by > 0 && nowSection.Blocks[bx, by - 1, sectionLargeEdge].Id == airId) ||
                        (by < sectionLargeEdge && nowSection.Blocks[bx, by + 1, sectionLargeEdge].Id == airId))
                    {
                        // If the block is visible, create it at once
                        Block nowBlock = nowSection.Blocks[bx, by, sectionLargeEdge];

                        this._blockCreator.CreateBlock(nowBlock);
                    }
                }
            }
        }

    }
}