using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LobbyGUIManager : MonoBehaviour
{
    public static LobbyGUIManager Instance { get; private set; }

    public Text PlayerUsernameLabel { get; private set; }
    private Button playButton;
    
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    public void Initialize()
    {
        GameObject canvasObject = GameObject.Find("Canvas");
        PlayerUsernameLabel = canvasObject.transform.Find("Game/Header/PlayerUsername").GetComponent<Text>();
        PlayerUsernameLabel.text = "Welcome back, " + PhotonNetwork.player.UserId;

		playButton = canvasObject.transform.Find("Game/GamePanel/PlayButton").GetComponent<Button>();
        playButton.onClick.AddListener(() => NetworkManager.Instance.StartMatchmaking());
    }
}
