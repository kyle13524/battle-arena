using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Hero))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class HeroController : MonoBehaviour 
{
	[SerializeField]
	private float jumpHeight;

	private Hero hero;
	private Animator anim;
	private new SpriteRenderer renderer;
	private new Rigidbody2D rigidbody;
	private Bounds playerBounds;
	private bool isGrounded;

	void Awake () 
	{		
		hero = this.GetComponent<Hero>();
		anim = this.GetComponent<Animator>();
		renderer = this.GetComponent<SpriteRenderer>();
		rigidbody = this.GetComponent<Rigidbody2D>();
		playerBounds = this.GetComponent<Collider2D>().bounds;
	}

	void Update()
	{
        if (!hero.photonView.isMine)
            return;

		Move(Input.GetAxisRaw("Horizontal"));     
		if(Input.GetButtonDown("Jump") && isGrounded)
		{
			Jump();
		}	

		if (Input.GetButtonDown ("BasicAttack")) 
		{
            hero.photonView.RPC("UseBasicAbility", PhotonTargets.All);// hero.UseBasicAbility ();
		}
		else if (Input.GetButtonDown ("UltimateAttack")) 
		{
			//hero.UseUltimateAbility (info);
		}

    }

    public void Move(float horizontal)
	{
		if (horizontal > 0)
		{
			anim.SetBool("Walking", true);
			hero.currentDirection = Vector2.right;
			renderer.flipX = false;
		}
		else if (horizontal < 0)
		{
			anim.SetBool("Walking", true);
			hero.currentDirection = Vector2.left;
			renderer.flipX = true;
		}
		else
		{
			anim.SetBool("Walking", false);
		}

		Vector2 newPosition = transform.position;
		newPosition.x += horizontal * hero.moveSpeed * Time.deltaTime;
		this.SetPosition(newPosition);
	}

	public void Jump()
	{
        Debug.Log("Jump: " + jumpHeight);
		rigidbody.AddForce(transform.up * jumpHeight, ForceMode2D.Impulse);
		isGrounded = false;
	}

	public void SetPosition(Vector2 position)
	{
		if (!PlayerCollidedWithBoundary(position))
		{
			this.transform.position = position;
		}
	}

	public bool PlayerCollidedWithBoundary(Vector2 newPosition)
	{
		float halfPlayerWidth = playerBounds.size.x / 2f;
		float halfPlayerHeight = playerBounds.size.y / 2f;

		Bounds mapBounds = Map.Instance.mapBounds;
		if (newPosition.x - halfPlayerWidth <= mapBounds.min.x ||
			newPosition.x + halfPlayerWidth >= mapBounds.max.x ||
			newPosition.y - halfPlayerHeight <= mapBounds.min.y ||
			newPosition.y + halfPlayerHeight >= mapBounds.max.y)
		{
			return true;
		}

		return false;
	}

	private void OnCollisionEnter2D(Collision2D collision)
	{
		if(collision.gameObject.CompareTag("Ground"))
		{
			isGrounded = true;
		}
	}

	private void OnCollisionExit2D(Collision2D collision)
	{
		if (collision.gameObject.CompareTag("Ground"))
		{
			isGrounded = false;
		}
	}
}
