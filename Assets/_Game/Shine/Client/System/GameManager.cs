using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField]
    private GameObject playerPrefab;

	[SerializeField]
	private GameEvent OnPlayerHealthChanged;

    public Player PlayerHandle { get; set; }

    public static GameManager Instance { get; set; }

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        DontDestroyOnLoad(this);
    }

    void Start()
	{
        SceneManager.sceneLoaded += OnSceneLoaded;
	}

	void Update()
    {
        if(PhotonNetwork.inRoom)
        {
			//GameGUIManager.Instance.SetHPBarValue(PlayerHandle.hero.currentHealth / 1000f);
        }     
    }

    public void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.buildIndex > 0)
            PhotonNetwork.isMessageQueueRunning = true;
        
        if (scene.buildIndex == 1)
        {
            GameLoader.Instance.GameLoadedEvent.AddListener(OnGameLoadedEvent);
            GameLoader.Instance.Load();
        }

        if (scene.buildIndex == 2)
        {
            PlayerHandle.basePosition = PhotonNetwork.isMasterClient ? Map.Instance.spawnSpots[0].transform.position : Map.Instance.spawnSpots[1].transform.position;
            PlayerHandle.SpawnHero();
            PlayerHandle.BuildBase();
            SetCameraTarget(PlayerHandle.hero.gameObject);
        }
    }

    void OnGameLoadedEvent()
    {
        ChatManager.Instance.Init(PhotonNetwork.player.UserId);

        LobbyGUIManager.Instance.Initialize();
        InitializeLobbyPlayer();
    }

    void InitializeLobbyPlayer()
    {
        PlayerHandle = Instantiate(playerPrefab).GetComponent<Player>();
        PlayerHandle.basePosition = Map.Instance.spawnSpots[0].transform.position;
    }

    private void SetCameraTarget(GameObject target)
    {
        Camera.main.GetComponent<CameraController>().SetTarget(target.transform);
    }
}
