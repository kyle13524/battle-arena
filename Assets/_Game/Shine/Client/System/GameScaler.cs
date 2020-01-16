using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameScaler : MonoBehaviour 
{

	// Use this for initialization
	void Start () 
	{		
		Camera.main.aspect = 1280f/720f;	
	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}
}
