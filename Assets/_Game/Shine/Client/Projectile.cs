using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Projectile : MonoBehaviour 
{
	private new Rigidbody2D rigidbody;
	private new SpriteRenderer renderer;
	private ProjectileAbility ability;
	private Vector2 originPosition;

    private PhotonMessageInfo projectileInfo;

    private bool fired = false;

	void Awake ()
	{
		rigidbody = this.GetComponent<Rigidbody2D> ();
		renderer = this.GetComponent<SpriteRenderer> ();
	}

	public void Dispatch(Sprite sprite, ProjectileAbility ability, PhotonMessageInfo info)
	{
        this.renderer.sprite = sprite;
        this.ability = ability;
        this.projectileInfo = info;

        originPosition = this.projectileInfo.photonView.transform.position;
		rigidbody.AddForce (this.projectileInfo.photonView.GetComponent<Hero>().currentDirection * ability.shootSpeed, ForceMode2D.Impulse);
        fired = true;
    }

	void Update()
	{
        if (!fired)
            return;

        if (Vector2.Distance (transform.position, originPosition) >= ability.attackRange) 
		{
			Destroy (this.gameObject);
		}
	}

	void OnTriggerEnter2D(Collider2D coll)
	{
		BaseUnit unit = coll.GetComponent<BaseUnit> ();

        if (unit != null && projectileInfo.photonView.viewID != unit.photonView.viewID) 
		{
            if(!unit.photonView.isMine)
            {
                unit.photonView.RPC("Damage", PhotonTargets.All, ability.damage);
                if (ability.effect != null)
                {
                    unit.AddEffect(ability.effect);
                }
            }

            if(++ability.hits >= ability.maxHits)
            {
                Destroy(this.gameObject); 
            }                      
        }
	}
}
