using UnityEngine;

public abstract class Ability
{
    public string id;
    public string name;
    public float damage;
    public float attackSpeed;
    public float attackRange;
    public int maxHits;
    public int hits;
    public AbilityEffect effect;
    public BaseUnit owner;

	public float lastDeployTime;

	public bool CooledDown()
	{
		return (Time.time >= lastDeployTime + attackSpeed);
	}

	public void Deploy (PhotonMessageInfo info)
	{
        OnAbility(info);
		lastDeployTime = Time.time;
	}

	protected abstract void OnAbility(PhotonMessageInfo info);
}

public enum AbilityType
{
    Melee,
    MeleeAOE,
    Projectile,
    ProjectileAOE
}