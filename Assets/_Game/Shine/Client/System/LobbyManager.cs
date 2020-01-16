using System.Linq;
using UnityEngine;

public class LobbyManager : MonoBehaviour
{
    [SerializeField]
    private GameObject lobbyHero;
    [SerializeField]
    private GameObject lobbyCastle;

    void Start()
    {
        //LoadLobbyUnits();
    }

    void LoadLobbyUnits()
    {
        Player playerHandle = GameManager.Instance.PlayerHandle;
        playerHandle.castle = GetPlayerCurrentCastle(playerHandle);
        playerHandle.hero = GetPlayerCurrentHero(playerHandle);

        Sprite castleSprite = Resources.Load<Sprite>("Units/Castle/Sprites/" + playerHandle.castle.id);
        lobbyCastle.GetComponentInChildren<SpriteRenderer>().sprite = castleSprite;

        var animController = Resources.Load<RuntimeAnimatorController>("Units/Hero/AnimControllers/" + playerHandle.hero.id);
        lobbyHero.GetComponentInChildren<Animator>().runtimeAnimatorController = animController;

        PositionLobbyUnits();
    }

    void PositionLobbyUnits()
    {
        lobbyCastle.transform.position = GameManager.Instance.PlayerHandle.basePosition;
        lobbyHero.transform.position = GameManager.Instance.PlayerHandle.basePosition + Vector2.right * 4f; //Reference to hero size should be made here
    }

    //TODO: Handlers here for changing the lobby settings along with the newly selected units.

    private Castle GetPlayerCurrentCastle(Player playerHandle)
    {
        Castle[] castles = DataLibrary.Instance.GetUnits<Castle>();
        return castles.FirstOrDefault(c => c.level == playerHandle.level);
    }

    private Hero GetPlayerCurrentHero(Player playerHandle)
    {
        Hero[] heroes = DataLibrary.Instance.GetUnits<Hero>();
        return heroes.FirstOrDefault(h => h.id == playerHandle.hero.id);
    }
}
