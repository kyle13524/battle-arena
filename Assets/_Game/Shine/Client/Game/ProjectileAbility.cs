using System;
using UnityEngine;

public class ProjectileAbility : Ability
{
	[Header("Projectile Settings")]
	[SerializeField]
	private GameObject projectilePrefab;
	[SerializeField]
	private Sprite projectileSprite;

	public float shootSpeed;

	protected override void OnAbility(PhotonMessageInfo info)
	{
        Projectile projectile = UnityEngine.Object.Instantiate(projectilePrefab, info.photonView.transform.position, Quaternion.identity).GetComponent<Projectile>();
        projectile.Dispatch(projectileSprite, this, info);
    }
}