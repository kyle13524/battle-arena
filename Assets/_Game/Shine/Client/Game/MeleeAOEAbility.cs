using System;
using UnityEngine;

public class MeleeAOEAbility : Ability
{
	protected override void OnAbility(PhotonMessageInfo info)
	{
        Debug.Log("Fire Melee AOE Ability within " + attackRange + " units");
    }
}