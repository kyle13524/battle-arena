using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Castle : BaseUnit 
{
	[Header("Castle Options")]
	public int level;
	public List<Transform> artillerySlots = new List<Transform> ();
		
	void OnDestroyEvent()
	{
		//End game with this castle as a loser
	}
}
