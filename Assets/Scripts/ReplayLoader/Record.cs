using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TMPro;
using System.Xml;
using UnityEngine.Events;
using static UnityEditor.Progress;

public class Record : MonoBehaviour
{
    public enum PlayState
    {
        Prepare,
        Play,
        Pause,
        End
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
    /// The slider which can change the record playing rate
    /// </summary>
    private Slider _recordSpeedSlider;
    private TMP_Text _recordSpeedText;
    private float _recordSpeedSliderMinValue;
    private float _recordSpeedSliderMaxValue;

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
        Debug.Log(this._stopButtonSprite);
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


        // Record playing rate slider
        this._recordSpeedSlider = GameObject.Find("Canvas/RecordSpeedSlider").GetComponent<Slider>();
        this._recordSpeedText = GameObject.Find("Canvas/RecordSpeedSlider/Value").GetComponent<TMP_Text>();

        this._recordSpeedSliderMinValue = this._recordSpeedSlider.minValue;
        this._recordSpeedSliderMaxValue = this._recordSpeedSlider.maxValue;
        // Set the default slider speed to 1;
        // Linear: 0~1
        float speedRate = (1 - RecordInfo.MinSpeed) / (RecordInfo.MaxSpeed - RecordInfo.MinSpeed);
        this._recordSpeedSlider.value = this._recordSpeedSliderMinValue + (this._recordSpeedSliderMaxValue - this._recordSpeedSliderMinValue) * speedRate;
        // Add listenr
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
        JArray creationList = (JArray)eventDataJson["creationList"];
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
                    Debug.Log($"Create item (id: 0, unique_id: {uniqueId}) successfully!");
                }
                else
                {
                    Debug.Log($"Create item (id: 0, unique_id: {uniqueId}) error!");
                }
            }
            else if (entityId == 1)
            {
                // Item
                int itemTypeId = (int)entityJson["item_type_id"];
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
            int? entityTypeId = null;

            if (EntitySource.GetItem(uniqueId) != null)
            {
                entityTypeId = 1;
            }
            else if (EntitySource.GetPlayer(uniqueId) != null)
            {
                entityTypeId = 0;
            }
            if (entityTypeId == null) return;


            if (entityTypeId == 0)
            {
                // Search the player
                Player player = EntitySource.GetPlayer(uniqueId);
                // Update the position
                Vector3 newPosition = new Vector3(
                    (float)entityJson["position"]["x"],
                    (float)entityJson["position"]["y"],
                    (float)entityJson["position"]["z"]
                );
                player.UpdatePosition(newPosition);
            }
            else if (entityTypeId == 1)
            {
                // Search the item
                Item item = EntitySource.GetItem(uniqueId);
                // Update the position
                Vector3 newPosition = new Vector3(
                    (float)entityJson["position"]["x"],
                    (float)entityJson["position"]["y"],
                    (float)entityJson["position"]["z"]
                );
                item.UpdatePosition(newPosition);

            }
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
            for (; this._recordInfo.NowRecordNum + 1 < this._recordArray.Count;
                this._recordInfo.NowRecordNum++)
            {
                JObject nowEvent = (JObject)this._recordArray[this._recordInfo.NowRecordNum + 1];
                if (this._recordInfo.NowTick < (int)nowEvent["tick"])
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
                        case "after_entity_create_event_record":
                            this.AfterEntityCreateEvent(nowEventDataJson);
                            break;
                        case "after_entity_position_change_event_record":
                            this.AfterEntityPositionChangeEvent(nowEventDataJson);
                            break;
                        default:
                            break;
                    }
                }
            }
            // Ticks
            this._recordInfo.NowTick++;
        }
        // Upend
        else
        {

        }

    }
    private void Update()
    {
        if (this._recordInfo.NowPlayState == PlayState.Play &&
            this._recordInfo.NowRecordNum < this._recordArray.Count)
        {
            if (this._recordInfo.NowDeltaTime > this._recordInfo.NowframeTime)
            {
                UpdateTick();
                this._recordInfo.NowDeltaTime = 0;
            }
            else
            {
                this._recordInfo.NowDeltaTime += Time.deltaTime;
            }
        }
    }
}

