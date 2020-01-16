using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoginGUIManager : MonoBehaviour
{
    private Button loginButton;
    private InputField usernameField;
    private InputField passwordField;

	// Use this for initialization
	void Start ()
    {
        usernameField = GameObject.Find("Canvas").transform.Find("LoginPanel").Find("UsernameInputField").GetComponent<InputField>();
        passwordField = GameObject.Find("Canvas").transform.Find("LoginPanel").Find("PasswordInputField").GetComponent<InputField>();
        loginButton = GameObject.Find("Canvas").transform.Find("LoginPanel").Find("LoginButton").GetComponent<Button>();
        loginButton.onClick.AddListener(OnLogin);
    }

    private void OnLogin()
    {
        if (!string.IsNullOrEmpty(usernameField.text) && !string.IsNullOrEmpty(passwordField.text))
        {
            FindObjectOfType<LoginManager>().Login(usernameField.text);
            // touch db's vagina here..
        }
        else
        {
            Debug.Log("Fields cannot be empty");
        }
    }
}
