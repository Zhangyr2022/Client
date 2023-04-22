using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class EscapeController : MonoBehaviour
{
    private GameObject _escapePanelGameObject;
    private Button _continueGameButton;
    private Button _backToMenuButton;
    // Start is called before the first frame update
    void Start()
    {
        this._escapePanelGameObject = GameObject.Find("Canvas/Escape");
        this._continueGameButton = GameObject.Find("Canvas/Escape/ContinueGameButton").GetComponent<Button>();
        this._backToMenuButton = GameObject.Find("Canvas/Escape/BackToMenuButton").GetComponent<Button>();

        this._continueGameButton.onClick.AddListener(() =>
        {
            this._escapePanelGameObject.SetActive(false);
        });

        this._backToMenuButton.onClick.AddListener(() =>
        {
            // ·ÖÀà
        });

        this._escapePanelGameObject.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.Return))
        {
            this._escapePanelGameObject.SetActive(true); 
        }
    }
}
