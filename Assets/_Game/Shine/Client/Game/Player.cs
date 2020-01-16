using UnityEngine;

public class Player : MonoBehaviour
{
	private const float heroPositionOffset = 4.0f;

	public int level;
    public string username;
	public Hero hero;
	public Castle castle;
    //public Artillery[] artillery;
    //public Minion[] minions;

	public Vector2 basePosition;
    
    public void BuildBase()
	{
        if (PhotonNetwork.inRoom)
        {
            if (PhotonNetwork.isMasterClient)
                castle = PhotonNetwork.InstantiateSceneObject("Units/Castle/" + castle.id, basePosition, Quaternion.identity, 0, null).GetComponent<Castle>();
            else
                // FIX DOUBLE CASTLE SPAWN ON PLAYER RECONNECT
                hero.photonView.RPC("SpawnOpponentCastle", PhotonTargets.OthersBuffered, castle.id, basePosition);
        }
    }

    public void SpawnHero()
    {
        Vector2 heroPosition = basePosition + Vector2.right * heroPositionOffset;
        if (PhotonNetwork.inRoom)
        {
            Debug.Log("HeroID:" + hero.id);
            hero = PhotonNetwork.Instantiate("Units/Hero/" + hero.id, heroPosition, Quaternion.identity, 0).GetComponent<Hero>();
            hero.name = PhotonNetwork.player.UserId;
            hero.basicAbility.lastDeployTime = -hero.basicAbility.attackSpeed;
            hero.ultimateAbility.lastDeployTime = -hero.ultimateAbility.attackSpeed;
        }
    }
}