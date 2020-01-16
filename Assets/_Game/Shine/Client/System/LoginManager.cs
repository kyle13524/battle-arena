using System;
using UnityEngine;

public class LoginManager : MonoBehaviour
{
    public DateTime LoginTime { get; private set; }

	public void Login(string username)
	{
        NetworkManager.Instance.Connect(username);
	}

    public void Encrypt()
    {

    }

    public void Decrypt()
    {

    }
}
