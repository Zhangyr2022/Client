using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;

public class Record : MonoBehaviour
{
    public enum PlayState
    {
        Prepare,
        Play,
        Pause,
        End,
        Jump
    }
    public class RecordInfo
    {
        // 20 frame per second
        public const float FrameTime = 0.05f;
        public PlayState NowPlayState = PlayState.Pause;
        public int NowTick = 0;
        /// <summary>
        /// Now record serial number
        /// </summary>
        public int NowRecordNum = 0;
        /// <summary>
        /// The speed of the record which can be negative
        /// </summary>
        public float RecordSpeed = 1f;
        public const float MinSpeed = -5f;
        public const float MaxSpeed = 5f;

        /// <summary>
        /// Contains all the item in the game
        /// </summary>
        public float NowframeTime
        {
            get
            {
                return FrameTime / RecordSpeed;
            }
        }
        /// <summary>
        /// If NowDeltaTime is larger than NowframeTime, then play the next frame
        /// </summary>
        public float NowDeltaTime = 0;

        /// <summary>
        /// The target tick to jump
        /// </summary>
        public int JumpTargetTick = int.MaxValue;
        /// <summary>
        /// Current max tick
        /// </summary>
        public int MaxTick;
        public void Reset()
        {
            this.RecordSpeed = 1f;
            this.NowTick = 0;
            this.NowRecordNum = 0;
            JumpTargetTick = int.MaxValue;
        }
    }
    private BlockCreator _blockCreator;
    private EntityCreator _entityCreator;
    private RecordInfo _recordInfo;
    private Upload _upload = new() { };
    private Upload.OpenFileName _recordFile = new() { };
    private JArray _recordArray;

    /// <summary>
    /// Stop / Continue button
    /// </summary>
    private Button _stopButton;
    /// <summary>
    /// The stop sprite
    /// </summary>
    private Sprite _stopButtonSprite;
    private Sprite _continueButtonSprite;

    /// <summary>
    /// Replay button
    /// </summary>
    private Button _replayButton;
    /// <summary>
    /// The slider which can change the record playing rate
    /// </summary>
    private Slider _recordSpeedSlider;
    private TMP_Text _recordSpeedText;
    private float _recordSpeedSliderMinValue;
    private float _recordSpeedSliderMaxValue;

    /// <summary>
    /// 
    /// </summary>
    private Slider _processSlider;

    private TMP_Text _jumpTargetTickText; // The target tick text in Unity 
    private TMP_Text _maxTickText; // The text of max tick in Unity


    public RecordInfo RecordInformation
    {
        get
        {
            return this._recordInfo;
        }
    }

    private void Start()
    {
        // Initialize the _recordInfo
        this._recordInfo = new();
        // Initialize the BlockCreator
        this._blockCreator = GameObject.Find("BlockCreator").GetComponent<BlockCreator>();
        // Initialize the ItemCreator
        this._entityCreator = GameObject.Find("EntityCreator").GetComponent<EntityCreator>();
        // Get json file
        var fileLoaded = GameObject.Find("FileLoaded").GetComponent<FileLoaded>();
        // Check if the file is Level json
        this._recordFile = fileLoaded.File;

        // GUI //

        // Get stop button 
        this._stopButton = GameObject.Find("Canvas/StopButton").GetComponent<Button>();
        // Get stop button sprites
        this._stopButtonSprite = Resources.Load<Sprite>("GUI/Button/StopButton");
        this._continueButtonSprite = Resources.Load<Sprite>("GUI/Button/ContinueButton");
        // Pause at beginning
        this._stopButton.GetComponent<Image>().sprite = _continueButtonSprite;
        // Add listener to stop button
        this._stopButton.onClick.AddListener(() =>
        {
            if (this._recordInfo.NowPlayState == PlayState.Play)
            {
                this._stopButton.GetComponent<Image>().sprite = this._continueButtonSprite;
                this._recordInfo.NowPlayState = PlayState.Pause;
            }
            else if (this._recordInfo.NowPlayState == PlayState.Pause)
            {
                this._stopButton.GetComponent<Image>().sprite = this._stopButtonSprite;
                this._recordInfo.NowPlayState = PlayState.Play;
            }
        });

        // Get Replay button
        this._replayButton = GameObject.Find("Canvas/ReplayButton").GetComponent<Button>();
        this._replayButton.onClick.AddListener(() =>
        {
            this._recordInfo.Reset();
            this._entityCreator.DeleteAllEntities();
        });


        // Record playing rate slider
        this._recordSpeedSlider = GameObject.Find("Canvas/RecordSpeedSlider").GetComponent<Slider>();
        this._recordSpeedText = GameObject.Find("Canvas/RecordSpeedSlider/Value").GetComponent<TMP_Text>();

        this._recordSpeedSliderMinValue = this._recordSpeedSlider.minValue;
        this._recordSpeedSliderMaxValue = this._recordSpeedSlider.maxValue;
        // Set the default slider speed to 1;
        // Linear: 0~1
        float speedRate = (1 - RecordInfo.MinSpeed) / (RecordInfo.MaxSpeed - RecordInfo.MinSpeed);
        this._recordSpeedSlider.value = this._recordSpeedSliderMinValue + (this._recordSpeedSliderMaxValue - this._recordSpeedSliderMinValue) * speedRate;
        // Add listener
        this._recordSpeedSlider.onValueChanged.AddListener((float value) =>
        {
            // Linear
            float sliderRate = (value - this._recordSpeedSliderMinValue) / (this._recordSpeedSliderMaxValue - this._recordSpeedSliderMinValue);
            // Compute current speed
            this._recordInfo.RecordSpeed = RecordInfo.MinSpeed + (RecordInfo.MaxSpeed - RecordInfo.MinSpeed) * sliderRate;
            // Update speed text
            _recordSpeedText.text = $"Speed: {Mathf.Round(this._recordInfo.RecordSpeed * 100) / 100f:F2}";
        });


        // Check
        if (this._recordFile == null)
        {
            Debug.Log("Loading file error!");
            return;
        }
        this._recordArray = LoadRecordData();
        this._recordInfo.MaxTick = (int)this._recordArray.Last["tick"];

        // Process slider
        this._processSlider = GameObject.Find("Canvas/ProcessSlider").GetComponent<Slider>();
        this._processSlider.value = 0;
        this._jumpTargetTickText = GameObject.Find("Canvas/ProcessSlider/Handle Slide Area/Handle/Value").GetComponent<TMP_Text>();
        this._maxTickText = GameObject.Find("Canvas/ProcessSlider/Max").GetComponent<TMP_Text>();
        this._recordInfo.MaxTick = this._recordArray.Count;
        this._maxTickText.text = $"{this._recordInfo.MaxTick}";
        // Add listener
        this._processSlider.onValueChanged.AddListener((float value) =>
        {
            int nowTargetTick = (int)(value * this._recordInfo.MaxTick);
            if (PlayState.Play == this._recordInfo.NowPlayState && Mathf.Abs(this._recordInfo.NowTick - nowTargetTick) > 1)
            {
                // Jump //
                // Reset the scene if the jump tick is smaller than now tick
                if (this._recordInfo.NowTick > nowTargetTick)
                {
                    this._recordInfo.Reset();
                    this._entityCreator.DeleteAllEntities();
                }
                // Change current state
                this._recordInfo.NowPlayState = PlayState.Jump;
                // Change target tick
                this._recordInfo.JumpTargetTick = nowTargetTick;
            }
        });
    }
    private JArray LoadRecordData()
    {
        JObject recordJsonObject = JsonUtility.UnzipRecord(this._recordFile.File);
        // Load the record array
        JArray recordArray = (JArray)recordJsonObject["records"];

        if (recordArray == null)
        {
            Debug.Log("Record file is empty!");
            return null;
        }
        Debug.Log(recordArray.ToString());
        return recordArray;
    }
    #region Event Definition

    /// <summary>
    /// Change the position of entity
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntityCreateEvent(JObject eventDataJson)
    {
        JArray creationList = (JArray)eventDataJson["creation_list"];
        foreach (JObject entityJson in creationList)
        {
            int entityId = (int)entityJson["entity_type_id"];
            int uniqueId = (int)entityJson["unique_id"];
            Vector3 position = new(
                (float)entityJson["position"]["x"],
                (float)entityJson["position"]["y"],
                (float)entityJson["position"]["z"]
            );
            float yaw = (float)entityJson["orientation"]["yaw"];
            float pitch = (float)entityJson["orientation"]["pitch"];

            if (entityId == 0)
            {
                // Player
                if (this._entityCreator.CreatePlayer(new Player(uniqueId, position, yaw, pitch)) == true)
                {
                    Debug.Log($"Create Player (id: 0, unique_id: {uniqueId}, yaw: {yaw}, pitch: {pitch}) successfully!");
                }
                else
                {
                    Debug.Log($"Create Player (id: 0, unique_id: {uniqueId}) error!");
                }
            }
            else if (entityId == 1)
            {
                // Item
                int itemTypeId = (int)(entityJson["item_type_id"] ?? 12);

                if (this._entityCreator.CreateItem(new Item(uniqueId, position, itemTypeId)) == true)
                {
                    Debug.Log($"Create item (id: {itemTypeId}, unique_id: {uniqueId}) successfully!");
                }
                else
                {
                    Debug.Log($"Create item (id: {itemTypeId}, unique_id: {uniqueId}) error!");
                }
            }
        }
    }

    /// <summary>
    /// Create an entity
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntityPositionChangeEvent(JObject eventDataJson)
    {
        JArray changeList = (JArray)eventDataJson["change_list"];

        foreach (JObject entityJson in changeList)
        {
            int uniqueId = (int)entityJson["unique_id"];

            Entity entity = EntitySource.GetEntity(uniqueId, out int? entityTypeId);
            if (entityTypeId == null) continue;

            if (entityTypeId == 0)
            {
                // Update the position
                Vector3 newPosition = new Vector3(
                    (float)entityJson["position"]["x"],
                    (float)entityJson["position"]["y"],
                    (float)entityJson["position"]["z"]
                );
                ((Player)entity).UpdatePosition(newPosition);
            }
            else if (entityTypeId == 1)
            {
                // Update the position
                Vector3 newPosition = new Vector3(
                    (float)entityJson["position"]["x"],
                    (float)entityJson["position"]["y"],
                    (float)entityJson["position"]["z"]
                );
                ((Item)entity).UpdatePosition(newPosition);
            }
        }
    }

    /// <summary>
    /// Create an entity
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntityOrientationChangeEvent(JObject eventDataJson)
    {
        JArray changeList = (JArray)eventDataJson["change_list"];

        foreach (JObject entityJson in changeList)
        {
            int uniqueId = (int)entityJson["unique_id"];

            Entity entity = EntitySource.GetEntity(uniqueId, out int? entityTypeId);
            if (entityTypeId == null) continue;

            float pitch = (float)entityJson["orientation"]["pitch"];
            float yaw = (float)entityJson["orientation"]["yaw"];

            if (entityTypeId == 0)
            {
                // Update the orientation
                ((Player)entity).UpdateOrientation(pitch, yaw);
            }
            else if (entityTypeId == 1)
            {
                // Update the orientation
                ((Item)entity).UpdateOrientation(pitch, yaw);
            }
        }
    }
    /// <summary>
    /// Create an entity
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntityRemoveEvent(JObject eventDataJson)
    {
        JArray removalList = (JArray)eventDataJson["removal_list"];

        foreach (var entityJson in removalList)
        {
            int uniqueId = (int)entityJson["unique_id"];

            Entity entity = EntitySource.GetEntity(uniqueId, out int? entityTypeId);
            if (entityTypeId == null) continue;

            if (entityTypeId == 0)
            {
                this._entityCreator.DeletePlayer((Player)entity);
            }
            else if (entityTypeId == 1)
            {
                this._entityCreator.DeleteItem((Item)entity);
            }
        }
    }
    /// <summary>
    /// Create an entity
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterBlockChange(JObject eventDataJson)
    {
        JArray changeList = (JArray)eventDataJson["change_list"];

        foreach (var blockJson in changeList)
        {
            int x = (int)blockJson["position"]["x"];
            int y = (int)blockJson["position"]["y"];
            int z = (int)blockJson["position"]["z"];

            short newId = (short)blockJson["block_type_id"];
            Block block = this._blockCreator.UpdateBlock(new Vector3Int(x, y, z), newId, BlockDicts.BlockNameArray[newId], out short? originalTypeId);
            if (block != null)
            {
                // Check visibility of other blocks which are around this block;
                if ((originalTypeId == 0 && block.Id != 0) || (originalTypeId != 0 && block.Id == 0))
                {
                    CheckVisibility.CheckSingleBlockNeighbourVisibility(this._blockCreator, block);
                }
                Debug.Log($"Change block ({x},{y},{z}) from {originalTypeId} to {newId}!");
            }
            else
            {
                Debug.Log($"Cannot get block ({x},{y},{z})!");
            }
        }
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntitySpawn(JObject eventDataJson)
    {
        JArray spawnList = (JArray)eventDataJson["change_list"];
        foreach (JToken entityJson in spawnList)
        {
            int uniqueId = (int)entityJson["unique_id"];

            Entity entity = EntitySource.GetEntity(uniqueId, out int? entityTypeId);
            if (entityTypeId == null) continue;

            this._entityCreator.SpawnEntity(entity);
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="eventDataJson"></param>
    private void AfterEntityDespawn(JObject eventDataJson)
    {
        JArray spawnList = (JArray)eventDataJson["change_list"];
        foreach (JToken entityJson in spawnList)
        {
            int uniqueId = (int)entityJson["unique_id"];

            Entity entity = EntitySource.GetEntity(uniqueId, out int? entityTypeId);
            if (entityTypeId == null) continue;

            this._entityCreator.DespawnEntity(entity, (int)entityTypeId);
        }
    }
    #endregion

    #region Record Update
    #endregion
    /// <summary>
    /// 
    /// </summary>
    private void UpdateTick()
    {
        // Play
        if (this._recordInfo.RecordSpeed > 0)
        {
            List<JObject> nowEventsJson = new();

            // Find all the events at now tick
            for (; this._recordInfo.NowRecordNum < this._recordArray.Count; this._recordInfo.NowRecordNum++)
            {
                JObject nowEvent = (JObject)this._recordArray[this._recordInfo.NowRecordNum];
                if (this._recordInfo.NowTick == (int)nowEvent["tick"])
                {
                    nowEventsJson.Add(nowEvent);
                }
                else
                {
                    break;
                }
            }
            foreach (var nowEventJson in nowEventsJson)
            {

                if (nowEventJson["type"].ToString() == "event")
                {
                    JObject nowEventDataJson = (JObject)nowEventJson["data"];
                    switch (nowEventJson["identifier"].ToString())
                    {
                        case "after_entity_create":
                            this.AfterEntityCreateEvent(nowEventDataJson);
                            break;
                        case "after_entity_position_change":
                            this.AfterEntityPositionChangeEvent(nowEventDataJson);
                            break;
                        case "after_entity_remove":
                            this.AfterEntityRemoveEvent(nowEventDataJson);
                            break;
                        case "after_block_change":
                            this.AfterBlockChange(nowEventDataJson);
                            break;
                        case "after_entity_orientation_change":
                            this.AfterEntityOrientationChangeEvent(nowEventDataJson);
                            break;
                        case "after_entity_spawn":
                            this.AfterEntitySpawn(nowEventDataJson);
                            break;
                        case "after_entity_despawn":
                            this.AfterEntityDespawn(nowEventDataJson);
                            break;
                        default:
                            break;
                    }
                }
            }
            // Ticks
            this._recordInfo.NowTick++;
            this._jumpTargetTickText.text = $"Tick\n{this._recordInfo.NowTick}"; // Update process slider text
            // move the process slider
            this._processSlider.value = (this._recordInfo.NowTick / (float)this._recordInfo.MaxTick);
            // Jump end if now tick reaches JumpTargetTick
            if (this._recordInfo.NowTick >= this._recordInfo.JumpTargetTick &&
                this._recordInfo.NowPlayState == PlayState.Jump)
            { this._recordInfo.NowPlayState = PlayState.Play; }
        }
        // Upend
        else
        {

        }

    }
    private void Update()
    {
        if ((this._recordInfo.NowPlayState == PlayState.Play && this._recordInfo.NowRecordNum < this._recordArray.Count) ||
            this._recordInfo.NowPlayState == PlayState.Jump)
        {
            if (this._recordInfo.NowDeltaTime > this._recordInfo.NowframeTime || this._recordInfo.NowPlayState == PlayState.Jump)
            {
                UpdateTick();
                this._recordInfo.NowDeltaTime = 0;
            }
            this._recordInfo.NowDeltaTime += Time.deltaTime;
        }
    }
}

