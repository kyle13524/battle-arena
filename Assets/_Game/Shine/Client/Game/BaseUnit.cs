using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class OnDeathEvent : UnityEvent<PhotonMessageInfo> { }

[RequireComponent(typeof(PhotonView))]
public abstract class BaseUnit : Photon.MonoBehaviour
{
	[Header("Unit Options")]
    public string id;
    public float moveSpeed;
	public float maxHealth;
    public int rarity;
	public Ability basicAbility;
	public Ability ultimateAbility;
    public Vector2 currentDirection;

	public GameEvent OnHealthChanged;

	protected OnDeathEvent OnDeathEvent;

    private List<AbilityEffect> activeEffects;
    public float currentHealth;

    [PunRPC]
    public void Damage(float damage, PhotonMessageInfo info)
    {
        if (currentHealth - damage > 0)
        {
            currentHealth -= damage;
        }
        else
        {
            currentHealth = 0;
            OnDeathEvent.Invoke(info);
        }

		OnHealthChanged.Raise();
    }
    
	public void Restore(float restore)
    {
        if (currentHealth + restore <= maxHealth)
        {
            currentHealth += restore;
        }
        else
        {
            currentHealth = maxHealth;
        }
    }

	public void AddEffect(AbilityEffect effect)
    {
        if (!activeEffects.Contains(effect))
        {
            activeEffects.Add(effect);
            StartCoroutine(TickEffect(effect));
        }
    }

	//TODO: May need to cancel all coroutines when the player dies. Could tick next spawn.
    IEnumerator TickEffect(AbilityEffect effect)
    {
        float start = Time.time;
        if (effect.rate <= 0)
        {
            ApplyEffect(effect);
            yield return new WaitForSeconds(effect.duration);
        }
        else
        {
            while (Time.time < start + effect.duration)
            {
                ApplyEffect(effect);
                yield return new WaitForSeconds(effect.rate);
            }      
        }

        RevertEffect(effect);
        activeEffects.Remove(effect);
    }

    void ApplyEffect(AbilityEffect effect)
    {
        switch(effect.type)
        {
            case AbilityEffect.Type.DOT:
                // TODO fix this
                Damage(effect.value, new PhotonMessageInfo());
                break;
            case AbilityEffect.Type.HOT:
                Restore(effect.value);
                break;
            case AbilityEffect.Type.SLOW:
                moveSpeed *= effect.value;
                break;
            case AbilityEffect.Type.SPEED:
				moveSpeed /= effect.value;
                break;
            case AbilityEffect.Type.DMG_DEC:
                basicAbility.damage = basicAbility.damage * effect.value;
                ultimateAbility.damage *= effect.value;
                break;
            case AbilityEffect.Type.DMG_INC:
                basicAbility.damage /= effect.value;
                ultimateAbility.damage /= effect.value;
                break;
            case AbilityEffect.Type.HP_DEC:
                currentHealth = maxHealth *= effect.value;
                break;
            case AbilityEffect.Type.HP_INC:
                currentHealth = maxHealth /= effect.value;
                break;
        }
    }

    void RevertEffect(AbilityEffect effect)
    {
        switch (effect.type)
        {
            case AbilityEffect.Type.SLOW:
				moveSpeed /= effect.value;
                break;
            case AbilityEffect.Type.SPEED:
				moveSpeed *= effect.value;
                break;
            case AbilityEffect.Type.DMG_DEC:
                basicAbility.damage /= effect.value;
                ultimateAbility.damage /= effect.value;
                break;
            case AbilityEffect.Type.DMG_INC:
                basicAbility.damage *= effect.value;
                ultimateAbility.damage *= effect.value;
                break;
            case AbilityEffect.Type.HP_DEC:
                float healthRatio = currentHealth / maxHealth; //validate this
                maxHealth /= effect.value;
                currentHealth = maxHealth * healthRatio;
                break;
            case AbilityEffect.Type.HP_INC:
                healthRatio = currentHealth / maxHealth; //validate this
                maxHealth *= effect.value;
                currentHealth = maxHealth * healthRatio;
                break;
        }
    }

    public enum Type 
    {
        Hero,
        Artillery,
        Minion
    }
}