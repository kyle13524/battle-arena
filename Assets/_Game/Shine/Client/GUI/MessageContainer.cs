using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MessageContainer : MonoBehaviour
{
	[SerializeField]
	private Text senderText;
	[SerializeField]
	private Text messageText;
	[SerializeField]
	private Text sendTimeText;

	public string Sender
	{ 
		get { return senderText.text; } 
		set { senderText.text = value; } 
	}

	public string Message 
	{ 
		get { return messageText.text; } 
		set { messageText.text = value; } 
	}

	public DateTime SendTime 
	{ 
		get { return DateTime.Parse(sendTimeText.text); } 
		set { sendTimeText.text = value.ToString(); } 
	}

    public Color Color
    {
        set { senderText.color = value; }
    }

	public void Extend(string message)
	{
		this.Message += Environment.NewLine + message;
	}

    public void ChangeColor(Color color)
    {
        senderText.color = color;
    }

	public string GetDateFormattedString()
	{
		return string.Empty;
	}
}