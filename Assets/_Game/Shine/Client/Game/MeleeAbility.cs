using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeAbility : Ability 
{
    protected override void OnAbility(PhotonMessageInfo info)
    {
        RaycastHit[] rayHits = Physics.RaycastAll(new Ray(owner.transform.position, owner.currentDirection), attackRange);
        foreach(RaycastHit hit in rayHits)
        {
            BaseUnit unit = hit.transform.GetComponent<BaseUnit>();
            if(unit != null)
            {
                if(owner.photonView.viewID != unit.photonView.viewID)
                {
                    if (!unit.photonView.isMine)
                    {
                        unit.photonView.RPC("Damage", PhotonTargets.All, damage);
                        if (effect != null)
                        {
                            unit.AddEffect(effect);
                        }

                    }

                    if(++hits >= maxHits)
                    {
                        break;
                    }
                }
            }
        }
    }
}
