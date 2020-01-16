using UnityEngine;
using ExitGames.Client.Photon.Chat;
using ExitGames.Client.Photon;
using System.Collections.Generic;

public class ChatManager : MonoBehaviour, IChatClientListener
{
    public static ChatManager Instance { get; private set; }
    public ChatClient ChatClient { get; private set; }
    public bool IsConnected { get; private set; }

    private const string chatAppId = "c21fcedf-ff69-425b-859e-c01348bbcfb0";
    private const string chatAppVersion = "1.0";

    public string[] Channels { get; private set; }

    // Use this for initialization
    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);

        Channels = new string[]
        {
            "Global"
        };

        ChatClient = new ChatClient(this)
        {
            ChatRegion = "US",
            UseBackgroundWorkerForSending = true
        };
    }

    void Update()
    {
        if (ChatClient != null && IsConnected)
        {
            ChatClient.Service();
        }
    }

    public void Init(string userId)
    {
        IsConnected = ChatClient.Connect(chatAppId, chatAppVersion, new ExitGames.Client.Photon.Chat.AuthenticationValues(userId));
    }

    public bool Send(string message, string channel)
    {
        return ChatClient.PublishMessage(channel, message);
    }

    public void OnConnected()
    {
        if (!IsConnected)
            return;

        Debug.Log("Successfully connected client to Photon Cloud");

        ChatClient.Subscribe(Channels);
    }

    public void OnDisconnected()
    {
        Cleanup();
        Debug.Log("Disconnected from server");
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        for (int i = 0; i < senders.Length; i++)
        {
            ChatGUIManager.Instance.AddGlobalMessage(senders[i], (string)messages[i]);
        }
    }

    void OnApplicationQuit()
    {
        Cleanup();
    }

    public void Cleanup()
    {
        if (ChatClient != null)
        {
            ChatClient.Disconnect();
            ChatClient = null;
        }
    }

    #region Ballsack

    public void OnChatStateChange(ChatState state)
    {
        Debug.Log("Connection State: " + state.ToString());
    }
    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        throw new System.NotImplementedException();
    }
    public void OnSubscribed(string[] channels, bool[] results) {}
    public void OnUnsubscribed(string[] channels)
    {
        throw new System.NotImplementedException();
    }
    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        throw new System.NotImplementedException();
    }
    public void DebugReturn(DebugLevel level, string message)
    {
        Debug.Log("Debug return: " + message);
    }

    #endregion Ballsack
}