using System.Collections;
using UnityEngine;
using Spine.Unity;

[RequireComponent(typeof (Rigidbody2D))]
[RequireComponent(typeof (AttackTrigger))]
[RequireComponent(typeof (BoxCollider2D))]
[AddComponentMenu("2D Action-RPG Kit/Create Player(Top Down)")]

public class TopdownInputController2D : MonoBehaviour {
	private SkeletonAnimation anim;
	private Rigidbody2D rb;
	public float speed = 6;
	private float dirX, dirY;
	private Status stat;
	private bool moving = false;
	private AttackTrigger atk;

	public bool canDash = false;
	public float dashSpeed = 15;
	public float dashDuration = 0.5f;
	private bool onDashing = false;
	public JoystickCanvas joyStick;// For Mobile
	private float moveHorizontal;
	private float moveVertical;

    [SerializeField]
    private AnimationReferenceAsset runAnim, dashAnim, idleAnim;
    private AnimationReferenceAsset currentAnimation;

    public void ResetAnimation()
    {
        currentAnimation = null;
    }
    private void SetAnimation(AnimationReferenceAsset animAsset, bool loop)
    {
        if (animAsset != currentAnimation)
        {
            anim.state.SetAnimation(0, animAsset, loop);
            currentAnimation = animAsset;
        }
    }

	void Start(){
		rb = GetComponent<Rigidbody2D>();
		rb.gravityScale = 0;
		rb.constraints = RigidbodyConstraints2D.FreezeRotation;
		stat = GetComponent<Status>();
		atk = GetComponent<AttackTrigger>();
		//if(!anim && stat.mainSprite){
		//	anim = stat.mainSprite;
		//}
		if(!anim && GetComponentInChildren<SkeletonAnimation>()){
			anim = GetComponentInChildren<SkeletonAnimation>();
        }
        currentAnimation = idleAnim;
        SetAnimation(idleAnim, true);
    }

	void Update(){
		if(Time.timeScale == 0.0f || stat.freeze || GlobalStatus.freezeAll || GlobalStatus.freezePlayer || !stat.canControl && !atk.meleefwd){
			rb.velocity = Vector2.zero;
			if(onDashing){
				CancelDash();
			}
			if(anim){
                SetAnimation(idleAnim, true);
			}
			return;
		}
		if(onDashing){
			return;
		}
		if(canDash && Input.GetKeyDown(KeyCode.Mouse1)){
			StartCoroutine("Dash");
		}

		if(joyStick){
			if(Input.GetButton("Horizontal") || Input.GetButton("Vertical")){
				moveHorizontal = Input.GetAxis("Horizontal");
				moveVertical = Input.GetAxis("Vertical");
			}else{
				moveHorizontal = joyStick.position.x;
				moveVertical = joyStick.position.y;
			}
		}else{
			moveHorizontal = Input.GetAxis("Horizontal");
			moveVertical = Input.GetAxis("Vertical");
		}
		//Flip Right Side
		if(moveHorizontal > 0.1 && !atk.facingRight){
			atk.facingRight = true;
			Vector3 rot = transform.eulerAngles;
			rot.y = 0;
			transform.eulerAngles = rot;
		}
		//Flip Left Side
		if(moveHorizontal < -0.1 && atk.facingRight){
			atk.facingRight = false;
			Vector3 rot = transform.eulerAngles;
			rot.y = 180;
			transform.eulerAngles = rot;
		}
	}
	
	void FixedUpdate(){
		if(Time.timeScale == 0.0f || stat.freeze || GlobalStatus.freezeAll || GlobalStatus.freezePlayer || stat.flinch || !stat.canControl || stat.block){
			moveHorizontal = 0;
			moveVertical = 0;
			//rb.velocity = Vector2.zero;
			return;
		}
		if(onDashing){
			if(Input.GetKeyUp(KeyCode.Mouse1) || atk.onAttacking){
				CancelDash();
			}

			if(atk.aimAtMouse){
				atk.LookAtMouse();
				Vector3 dir = atk.attackPoint.TransformDirection(Vector3.right);
				GetComponent<Rigidbody2D>().velocity = dir * dashSpeed;
			}else{
				Vector3 dir = transform.TransformDirection(Vector3.right);
				GetComponent<Rigidbody2D>().velocity = dir * dashSpeed;
			}
			return;
		}

		dirX = moveHorizontal * speed;
		dirY = moveVertical * speed;

		rb.velocity = new Vector2(dirX , dirY);
		if(moveHorizontal != 0 || moveVertical != 0){
			moving = true;
			if(anim){
                SetAnimation(runAnim, true);
			}
		}else if(moving){
			moving = false;
			if(anim){
                SetAnimation(idleAnim, true);
			}
		}
	}

	public void DashButton(){
		StartCoroutine("Dash");
	}

	public IEnumerator Dash(){
		if(!onDashing){
			if(stat.block){
				stat.GuardBreak("cancelGuard");
			}
			if(atk.aimAtMouse){
				atk.LookAtMouse();
			}
			onDashing = true;
            SetAnimation(dashAnim, false);
			yield return new WaitForSeconds(dashDuration);
			CancelDash();
		}
	}
	
	public void CancelDash(){
		StopCoroutine("Dash");
        SetAnimation(idleAnim, true);
        onDashing = false;
	}

}
