using UnityEngine;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
public class Hero : BaseUnit
{
    private Vector2 realPosition = Vector2.zero;
    private Vector2 sendPosition = Vector2.zero;

    private Animator anim;
    private SpriteRenderer spriteRenderer;
    private Rigidbody2D heroRigidbody;

    void Awake()
    {
        heroRigidbody = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        currentHealth = maxHealth;
        OnDeathEvent = new OnDeathEvent();
        OnDeathEvent.AddListener(OnDeath);
    }

    [PunRPC]
    public void SpawnOpponentCastle(string castleId, Vector2 basePos)
    {
        if (PhotonNetwork.isMasterClient)
        {
            PhotonNetwork.InstantiateSceneObject("Units/Castle/" + castleId, basePos, Quaternion.identity, 0, null).GetComponent<Castle>();
        }
    }

    [PunRPC]
    public void UseBasicAbility(PhotonMessageInfo info)
    {
        if (basicAbility.CooledDown())
        {
            //anim.SetTrigger("Basic");
            basicAbility.Deploy(info);
        }
    }

    [PunRPC]
    public void UseUltimateAbility(PhotonMessageInfo info)
    {
		if (ultimateAbility.CooledDown ()) 
		{
			//anim.SetTrigger("Ultimate");
			ultimateAbility.Deploy(info);
		}
    }

    [PunRPC]
    public void Respawn(float restoreAmount, PhotonMessageInfo info)
    {
        spriteRenderer.enabled = false;
        this.enabled = false;

        this.transform.position = new Vector2(
        PhotonNetwork.isMasterClient 
        ? Map.Instance.spawnSpots[0].transform.position.x 
        : Map.Instance.spawnSpots[1].transform.position.x,

        PhotonNetwork.isMasterClient 
        ? Map.Instance.spawnSpots[0].transform.position.y 
        : Map.Instance.spawnSpots[1].transform.position.y);

        Restore(restoreAmount);

        this.enabled = true;
        spriteRenderer.enabled = true;
    }

    public void OnDeath(PhotonMessageInfo info)
    {
        // Ensure the ondeath event came from our player
        if (PhotonNetwork.player.IsLocal && info.sender != PhotonNetwork.player)
        {
            Debug.Log("You have been killed by player: " + info.sender);

            photonView.RPC("Respawn", PhotonTargets.All, maxHealth);
        }
    }

    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.isWriting)
        {
            sendPosition = transform.position;
            stream.SendNext(sendPosition);
            stream.SendNext(spriteRenderer.flipX);
            stream.SendNext(anim.GetBool("Walking"));
        }
        else
        {
            realPosition = (Vector2)stream.ReceiveNext();
            spriteRenderer.flipX = (bool)stream.ReceiveNext();
            anim.SetBool("Walking", (bool)stream.ReceiveNext());
        }
    }

    void FixedUpdate()
    {
        if (photonView != null && PhotonNetwork.inRoom && !photonView.isMine)
        {
            transform.position = Vector2.Lerp(this.transform.position, realPosition, Time.fixedDeltaTime * 16.7f);
            currentDirection.x = (spriteRenderer.flipX ? -1 : 1);
        }
    }
}
