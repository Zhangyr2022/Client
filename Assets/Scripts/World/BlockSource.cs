using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static System.Collections.Specialized.BitVector32;

public class BlockSource
{
    /// <summary>
    /// Key : <Vector3Int section.positionIndex> which is equal to section's position divided by 16
    /// </summary>
    public Dictionary<Vector3Int, Section> SectionDict;
    public BlockSource()
    {
        SectionDict = new Dictionary<Vector3Int, Section>();
    }
    /// <summary>
    /// Add section into the dict
    /// </summary>
    /// <param name="section"></param>
    /// <returns>False if the section in the position already exists</returns>
    public bool AddSection(Section section)
    {
        if (this.SectionDict.ContainsKey(section.PositionIndex))
        {
            return false;
        }
        else
        {
            this.SectionDict.Add(section.PositionIndex, section);
            return true;
        }
    }
    /// <summary>
    /// Get block by using absolute position
    /// </summary>
    /// <param name="position">Block absolute position</param>
    /// <returns></returns>
    public Block GetBlock(Vector3Int position)
    {
        Vector3Int sectionPositionIndex = Vector3Int.FloorToInt(new Vector3(position.x, position.y, position.z) / 16.0f);
        if (this.SectionDict.ContainsKey(sectionPositionIndex))
        {
            // The relative position to now section
            Vector3Int relativePosition = position - sectionPositionIndex * 16;
            return this.SectionDict[sectionPositionIndex].Blocks[relativePosition.x, relativePosition.y, relativePosition.z];
        }
        else
        {
            // Cannot find the block
            return null;
        }
    }
}
