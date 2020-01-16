using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChatGUIManager : MonoBehaviour 
{
    public static ChatGUIManager Instance { get; private set; }

	[SerializeField]
	private GameObject messagePanel;
	[SerializeField]
	private GameObject messageContainerPrefab;
	[SerializeField]
	private InputField messageInputField;
    [SerializeField]
    private ScrollRect messageScrollRect;

    private List<MessageContainer> currentGlobalMessages = new List<MessageContainer>();
	private List<MessageContainer> currentPrivateMessages = new List<MessageContainer>();

	public int maxGlobalMessages;
	public float messageSpacing;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
    }

    void Start()
	{
		LoadMessages();
	}

	private void LoadMessages()
	{
		//Load recent messages from database here based on type
	}

	public void AddGlobalMessage(string sender, string message)
	{
		var lastMessage = currentGlobalMessages.LastOrDefault();
		if (lastMessage != null && lastMessage.Sender == sender && (DateTime.Now - lastMessage.SendTime).Seconds < 30f)
		{
			lastMessage.Extend(message);
		} 
		else 
		{
			if(currentGlobalMessages.Count >= maxGlobalMessages) 
			{
				var first = currentGlobalMessages.FirstOrDefault();
				currentGlobalMessages.Remove(first);
				Destroy(first.gameObject);
			}

			var newMessage = Instantiate(
				messageContainerPrefab,
				messagePanel.transform)
			.GetComponent<MessageContainer>();

			newMessage.Sender = sender;
			newMessage.Message = message;
			newMessage.SendTime = DateTime.Now;
            newMessage.Color = (PhotonNetwork.player.UserId == sender) ? Color.red : Color.black;

			currentGlobalMessages.Add(newMessage);
		}

        Canvas.ForceUpdateCanvases();
        messageScrollRect.verticalNormalizedPosition = 0f;
    }

	public void OnGlobalSendButtonPressed()
	{
		string sender = PhotonNetwork.player.UserId;
		string message = messageInputField.text;
        
        if(!string.IsNullOrEmpty(message))
        {
            ChatManager.Instance.Send(message, ChatType.Global.ToString());
            messageInputField.text = string.Empty;
        }
    }
}

public enum ChatType
{
	Global,
	Private
}