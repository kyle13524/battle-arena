using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameGUIManager : MonoBehaviour
{
    public static GameGUIManager Instance { get; private set; }

    public Slider HPBar;

	private bool loaded = false;

    // Use this for initialization
    void Awake ()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(this);
	}

	void Start () 
	{
		InitializeUIElements();
	}

	void InitializeUIElements()
	{
		HPBar = GameObject.Find("HPBar").GetComponent<Slider>();
	}

	public void SetHPBarValue(float value) 
	{
		if (HPBar == null)
			return;

		HPBar.value = value;
	}
}
