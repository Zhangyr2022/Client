using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
public class FPSDisplay : MonoBehaviour
{
    [SerializeField]
    private TMP_Text _FPSText;
    private float _fpsByDeltatime = 0.2f;
    private float _passedTime = 0.0f;
    private int _frameCount = 0;
    private float _realtimeFPS = 0.0f;
    private void Start()
    {
        this._FPSText = GameObject.Find("ObserverCanvas/FPS").GetComponent<TMP_Text>();
    }
    void Update()
    {
        GetFPS();
    }
    private void SetFPS()
    {
        //如果QualitySettings.vSyncCount属性设置，这个值将被忽略。
        //设置应用平台目标帧率为 60
        //Application.targetFrameRate = 60;
    }
    private void GetFPS()
    {
        if (_FPSText == null) return;

        _frameCount++;
        _passedTime += Time.deltaTime;
        if (_passedTime >= _fpsByDeltatime)
        {
            _realtimeFPS = _frameCount / _passedTime;
            _FPSText.text = $"{_realtimeFPS.ToString("f1")} FPS";
            _passedTime = 0.0f;
            _frameCount = 0;
        }
    }

}
