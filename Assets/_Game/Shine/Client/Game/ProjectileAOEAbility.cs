using System;
using UnityEngine;

public class ProjectileAOEAbility : Ability
{
	protected override void OnAbility(PhotonMessageInfo info)
	{
        Debug.Log("Select position for AOE ability somewhere within " + attackRange + " units");
    }
}