using UnityEngine;

public class NetworkManager : MonoBehaviour
{
    public static NetworkManager Instance { get; private set; }

    public bool IsConnected { get; private set; }
    public string CurrentRoomName { get; private set; }
    public RoomOptions RoomOptions { get; private set; }

    // Use this for initialization
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        DontDestroyOnLoad(this);

        PhotonNetwork.sendRate = 12;
        PhotonNetwork.sendRateOnSerialize = 12;

        RoomOptions options = new RoomOptions
        {
            MaxPlayers = 2,
            PublishUserId = true,
            CleanupCacheOnLeave = true
        };
    }

    public void Connect(string username)
    {
        AuthenticationValues values = new AuthenticationValues();
        values.AuthType = CustomAuthenticationType.None;
        values.UserId = username;
        PhotonNetwork.AuthValues = values;
        IsConnected = PhotonNetwork.ConnectUsingSettings("1.0");
    }

    public void OnConnectedToMaster()
    {
        if (!IsConnected)
            return;

        Debug.Log("Connected to photon master server: " + PhotonNetwork.player.UserId);

        if (SceneManagerHelper.ActiveSceneName.Equals("Login"))
        {
            PhotonNetwork.LoadLevel("Lobby");
        }
    }

    public void StartMatchmaking()
    {
        Debug.Log("Started matchmaking...");
        PhotonNetwork.JoinRandomRoom();
    }

    public void StopMatchmaking()
    {
        if (PhotonNetwork.inRoom)
            PhotonNetwork.LeaveRoom();
        Debug.Log("Stopped matchmaking");
    }

    // In this case there is no room ready for us to join so create our own
    public void OnPhotonRandomJoinFailed()
    {
        Debug.Log("No match found... So going to create one!");
        PhotonNetwork.CreateRoom(null, RoomOptions, null);
    }

    public void OnJoinedRoom()
    {
        Debug.Log("Joined some sort of room - however we will do nothing for now to test");

        if (PhotonNetwork.isMasterClient)
        {
            ExitGames.Client.Photon.Hashtable properties = new ExitGames.Client.Photon.Hashtable
            {
                { "MatchFound", false }
            };
            PhotonNetwork.room.SetCustomProperties(properties);
        }

        //PhotonNetwork.isMessageQueueRunning = false;
        //PhotonNetwork.LoadLevel("Game");
    }

    public void OnPhotonCustomRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
    {
        Debug.Log("Onphotoncustomroompropertieschagned");
    }

    public void OnPhotonPlayerConnected(PhotonPlayer player)
    {
        Debug.Log("OnPhotonPlayerConnected: " + player + " | Is local client: " + player.IsLocal);
    }
}
