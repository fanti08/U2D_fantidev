using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof (Status))]
[RequireComponent(typeof (BoxCollider2D))]
[RequireComponent(typeof (Rigidbody2D))]
[AddComponentMenu("2D Action-RPG Kit/Create Ally")]

public class AllyAi : MonoBehaviour {
	public enum AIState { Moving = 0, Pausing = 1 , Idle = 2 , FollowMaster = 3}
	
	[HideInInspector]
	public Transform followTarget;
	public float approachDistance = 2.0f;
	public float detectRange = 7.0f;
	public float lostSight = 40.0f;
	public float speed = 4.0f;
	public float gravity = 0;
	
	public string[] attackAnimationTrigger = new string[1];
	public BulletStatus attackPrefab;
	public Transform attackPoint;
	
	public float attackCast = 0.3f;
	public float attackDelay = 0.5f;
	
	[HideInInspector]
	public AIState followState = AIState.Idle;
	private float distance = 0.0f;
	
	[HideInInspector]
	public bool cancelAttack = false;
	private bool attacking = false;
	//private bool castSkill = false;
	private GameObject[] gos;
	
	public AudioClip attackVoice;
	public AudioClip hurtVoice;
	
	public enum WhileAtkMon{
		MeleeFwd = 0,
		Immobile = 1
	}
	public WhileAtkMon whileAttack = WhileAtkMon.Immobile;
	
	public bool aimAtTarget = true;
	private Status stat;
	[HideInInspector]
	public bool facingRight = true;
	private Animator anim;
	private bool meleefwd = false;
	
	[System.Serializable]
	public class PatrollingSetting{
		public bool enable = false;
		public float patrolSpeed = 4;
		public float idleDuration = 1.5f;
		public float moveDuration = 1.5f;
	}
	public PatrollingSetting patrolSetting;
	private int patrolState = 0; //0 = Idle , 1 = Moving.
	private float patrolWait = 0;
	private int c = 0;
	private Vector3 pfwd = Vector3.right;
	private float density = 1;
	
	[System.Serializable]
	public class SkillSetting{
		public string skillName;
		public BulletStatus skillPrefab;
		public string skillAnimationTrigger;
		public GameObject castEffect;
		public float castEffectTime = 0.5f;
		public string castAnimationTrigger;
		public float castTime = 0.5f;
		public float delayTime = 1.5f;
		public float allSkillDelay = 10.5f;
		public bool spawnAtPlayer = false;
		public int repeatBullet = 1;
		public float repeatDelay = 0.3f;
		public DirectionSet moveDirection = DirectionSet.None;
		public float moveSpeed = 7.5f;
		public string moveAnimTrigger;
		public float moveDuration = 0.5f;
	}
	
	public enum DirectionSet{
		None = 0,
		Forward = 1,
		Backward = 2,
		Up = 3,
		Down = 4
	}
	
	public SkillSetting[] skill;
	private Transform targetPointer;
	public Transform master;
	public bool deadIfNoMaster = false;
	
	void Start(){
		gameObject.tag = "Ally";
		gameObject.layer = 8; //Set to Character Layer
		
		//Create new Attack Point if you didn't have one.
		if(!attackPoint){
			attackPoint = new GameObject().transform;
			attackPoint.position = transform.position;
			attackPoint.rotation = transform.rotation;
			attackPoint.parent = this.transform;
			attackPoint.name = "AttackPoint";
		}
		stat = GetComponent<Status>();
		if(!anim && stat.mainSprite){
			anim = stat.mainSprite;
		}
		if(!anim && GetComponent<Animator>()){
			anim = GetComponent<Animator>();
		}
		stability = stat.stability; //For Skill.
		GetComponent<Rigidbody2D>().gravityScale = gravity;
		GetComponent<Rigidbody2D>().constraints = RigidbodyConstraints2D.FreezeRotation;
		if(transform.eulerAngles.y == 0){
			facingRight = true;
		}
		if(GetComponent<Collider2D>()){
			density = GetComponent<Collider2D>().density;
		}
		if(!GetComponent<AudioSource>()){
			gameObject.AddComponent<AudioSource>();
		}
		targetPointer = new GameObject().transform;
		targetPointer.position = transform.position;
		targetPointer.rotation = transform.rotation;
		targetPointer.parent = this.transform;
		targetPointer.name = "TargetPointer";

		//Set Z Axis to 0
		Vector3 pos = transform.position;
		pos.z = 0;
		transform.position = pos;
	}
	
	void Update(){
		if(deadIfNoMaster && !master){
			stat.Death();
			return;
		}
		if(Time.timeScale == 0.0f || stat.freeze || GlobalStatus.freezeAll){
			followState = AIState.Idle;
			if(GetComponent<Rigidbody2D>().gravityScale > 0){
				GetComponent<Rigidbody2D>().velocity = new Vector2(0 , GetComponent<Rigidbody2D>().velocity.y);
			}else{
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			}
			if(anim){
				anim.SetBool("run" , false);
			}
			return;
		}
		FindClosestEnemy();
		
		if(meleefwd){
			if(GetComponent<Rigidbody2D>().gravityScale > 0){
				GetComponent<Rigidbody2D>().velocity = new Vector2(0 , GetComponent<Rigidbody2D>().velocity.y);
			}else{
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			}
			if(GetComponent<Rigidbody2D>().gravityScale == 0){
				Vector3 dir = attackPoint.TransformDirection(Vector3.right);
				GetComponent<Rigidbody2D>().velocity = dir * 4;
			}else{
				Vector3 dir = attackPoint.TransformDirection(Vector3.right);
				GetComponent<Rigidbody2D>().AddForce(dir * 3200 * density * Time.deltaTime);
			}
		}
		
		if(!followTarget && !master){
			if(followState == AIState.Moving || followState == AIState.Pausing){
				followState = AIState.Idle;
				if(anim){
					anim.SetBool("run" , false);
				}
			}
			return;
		}
		//-----------------------------------
		if(followTarget){
			distance = (transform.position - followTarget.position).magnitude;
		}
		
		if(skill.Length > 0){
			if(!onSkill && followState != AIState.Idle && distance <= skillDistance){
				if(wait >= skillDelay){
					CancelAttack();
					StartCoroutine("UseSkill");
					wait = 0;
				}else{
					wait += Time.deltaTime;
				}
			}
		}
		
		if(stat.flinch){
			cancelAttack = true;
			GetComponent<Rigidbody2D>().velocity = stat.knock * stat.knockForce;
			return;
		}

		if(master && (master.position - transform.position).magnitude > 30.0f){
			Vector3 pos = master.position;
			pos.y += 0.5f;
			transform.position = pos;
		}
		
		if(onMoving){
			if(fwdSkill && distance <= approachDistance){
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
				return;
			}
			/*CharacterController controller = GetComponent<CharacterController>();
			Vector3 forward = transform.TransformDirection(movDir);
			controller.Move(forward * movSpd * Time.deltaTime);
			
			Vector3 lookTo = followTarget.position;
			lookTo.y = transform.position.y;
			transform.LookAt(lookTo);*/
			GetComponent<Rigidbody2D>().velocity = movDir * movSpd;
		}
		
		if(attacking){
			return;
		}
		//------------------------------------
		if(followState == AIState.FollowMaster && master){
			//---------------------------------
			if((master.position - transform.position).magnitude <= 2.5f){
				followState = AIState.Idle;
				if(anim){
					anim.SetBool("run" , false);
				}
			}else{
				LookAtMaster();
				Vector2 destination = new Vector2((transform.position.x - master.position.x), (transform.position.y - master.position.y)) * speed * 20 * Time.deltaTime;
				GetComponent<Rigidbody2D>().velocity = -destination;
				if(anim){
					anim.SetBool("run" , true);
				}
			}
			//---------------------------------
		}else if(followState == AIState.Moving && followTarget){
			if(anim){
				anim.SetBool("run" , true);
			}
			LookAtTarget();

			if(master && (master.position - transform.position).magnitude > detectRange + 2.5f){
				followState = AIState.FollowMaster;
				if(anim){
					anim.SetBool("run" , true);
				}
			}else if(distance <= approachDistance) {
				followState = AIState.Pausing;
			}else if(distance >= lostSight){
				//Lost Sight
				GetComponent<Status>().health = GetComponent<Status>().maxHealth;
				followState = AIState.Idle;
				if(anim){
					anim.SetBool("run" , false);
				}
			}else{
				//transform.position = Vector2.MoveTowards(transform.position, followTarget.position, speed * Time.deltaTime);
				if(GetComponent<Rigidbody2D>().gravityScale > 0){
					GetComponent<Rigidbody2D>().velocity = transform.TransformDirection(Vector3.right) * speed;
				}else{
					if(targetPointer){
						Vector3 dir = followTarget.position - targetPointer.position;
						float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
						targetPointer.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
						GetComponent<Rigidbody2D>().velocity = targetPointer.TransformDirection(Vector3.right) * speed;
					}else{
						Vector2 destination = new Vector2((transform.position.x - followTarget.position.x), (transform.position.y - followTarget.position.y)) * speed * 10 * Time.deltaTime;
						GetComponent<Rigidbody2D>().velocity = -destination;
					}
				}
				
			}
		}else if(followState == AIState.Pausing && followTarget){
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			if(anim){
				anim.SetBool("run" , false);
			}
			//----Attack----
			StartCoroutine("Attack");

			if(master && (master.position - transform.position).magnitude > 5.0f){
				followState = AIState.FollowMaster;
				if(anim){
					anim.SetBool("run" , true);
				}
			}else if(distance > approachDistance){
				followState = AIState.Moving;
			}
		}else if(followState == AIState.Idle){
			if(patrolSetting.enable){
				if(patrolState == 1){//Moving
					GetComponent<Rigidbody2D>().velocity = pfwd * patrolSetting.patrolSpeed;
				}
				//----------------------------
				if(patrolWait >= patrolSetting.idleDuration && patrolState == 0){
					//Set to Moving Mode.
					int r = Random.Range(0 , 10);
					if(r >= 5){
						Vector3 rot = transform.eulerAngles;
						rot.y = 0;
						transform.eulerAngles = rot;
						facingRight = true;
					}else{
						Vector3 rot = transform.eulerAngles;
						rot.y = 180;
						transform.eulerAngles = rot;
						facingRight = false;
					}
					if(anim){
						anim.SetBool("run" , true);
					}
					//Random Movement Direction
					pfwd = transform.TransformDirection(Vector3.right);
					if(gravity == 0){
						pfwd.y = Random.Range(-1.1f , 1.1f);
					}
					patrolWait = 0; // Reset wait time.
					patrolState = 1; // Change State to Move.
				}
				if(patrolWait >= patrolSetting.moveDuration && patrolState == 1){
					//Set to Idle Mode.
					if(anim){
						anim.SetBool("run" , false);
					}
					GetComponent<Rigidbody2D>().velocity = Vector2.zero;
					patrolWait = 0;
					patrolState = 0;
				}
				patrolWait += Time.deltaTime;
				//-----------------------------
			}else{
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			}
			//----------------Idle Mode--------------
			//int getHealth = GetComponent<Status>().maxHealth - GetComponent<Status>().health;
			if(distance < detectRange && followTarget){
				followState = AIState.Moving;
			}else if(master && (master.position - transform.position).magnitude > 3.0f){
				followState = AIState.FollowMaster;
				if(anim){
					anim.SetBool("run" , true);
				}
			}
		}
	}
	
	void LookAtTarget(){
		if(!followTarget){
			return;
		}
		Vector3 delta = followTarget.position - transform.position; 
		
		if(delta.x >= 0 && !facingRight){
			Vector3 rot = transform.eulerAngles;
			rot.y = 0;
			transform.eulerAngles = rot;
			facingRight = true;
		}else if (delta.x < 0 && facingRight){
			Vector3 rot = transform.eulerAngles;
			rot.y = 180;
			transform.eulerAngles = rot;
			facingRight = false;
		}
	}

	void LookAtMaster(){
		if(!master){
			return;
		}
		Vector3 delta = master.position - transform.position; 
		
		if(delta.x >= 0 && !facingRight){
			Vector3 rot = transform.eulerAngles;
			rot.y = 0;
			transform.eulerAngles = rot;
			facingRight = true;
		}else if (delta.x < 0 && facingRight){
			Vector3 rot = transform.eulerAngles;
			rot.y = 180;
			transform.eulerAngles = rot;
			facingRight = false;
		}
	}
	
	void FindClosestEnemy(){ 
		// Find Closest Player   
		List<GameObject> gosList = new List<GameObject>(GameObject.FindGameObjectsWithTag("Enemy"));
		//gosList.AddRange(GameObject.FindGameObjectsWithTag("Ally"));
		
		gos = gosList.ToArray() as GameObject[];
		
		if(gos.Length > 0){
			float distance = Mathf.Infinity; 
			Vector3 position = transform.position; 
			
			foreach(GameObject go in gos) { 
				Vector3 diff = (go.transform.position - position); 
				float curDistance = diff.sqrMagnitude; 
				if(curDistance < distance) { 
					//------------
					followTarget = go.transform; 
					distance = curDistance;
				} 
			} 
		}
	}
	
	void CancelAttack(){
		c = 0;
		attacking = false;
		StopCoroutine("Attack");
	}
	
	IEnumerator Attack(){
		cancelAttack = false;
		if(!stat.flinch && !stat.freeze && !attacking){
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
			attacking = true;
			if(whileAttack == WhileAtkMon.MeleeFwd){
				StartCoroutine(MeleeDash());
			}
			if(anim && attackAnimationTrigger[c] != ""){
				anim.SetTrigger(attackAnimationTrigger[c]);
			}
			LookAtTarget();
			if(aimAtTarget && followTarget){
				//attackPoint.LookAt(followTarget.position);
				Vector3 dir = followTarget.position - attackPoint.position;
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				attackPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			}
			yield return new WaitForSeconds(attackCast);
			
			if(!cancelAttack){
				if(attackVoice && !stat.flinch){
					GetComponent<AudioSource>().PlayOneShot(attackVoice);
				}
				Transform bulletShootout = Instantiate(attackPrefab.transform, attackPoint.position , attackPoint.rotation) as Transform;
				bulletShootout.gameObject.SetActive(true);
				bulletShootout.GetComponent<BulletStatus>().Setting(stat.atk , stat.matk , "Player" , this.gameObject);
				c++;
				if(c >= attackAnimationTrigger.Length){
					c = 0;
				}
				yield return new WaitForSeconds(attackDelay);
				attacking = false;
				CheckDistance();
				/*if(distance > approachDistance + 0.55f){
					c = 0;
				}*/
			}else{
				c = 0;
				attacking = false;
			}
		}
	}
	
	void CheckDistance(){
		if(master && (master.position - transform.position).magnitude > detectRange + 4.5f){
			followState = AIState.FollowMaster;
			if(anim){
				anim.SetBool("run" , true);
			}
			return;
		}
		if(!followTarget || GlobalStatus.freezeAll){
			followState = AIState.Idle;
			if(anim){
				anim.SetBool("run" , false);
			}
			return;
		}
		float distancea = (followTarget.position - transform.position).magnitude;
		if(distancea > approachDistance + 2){
			if(master){
				followState = AIState.FollowMaster;
			}else{
				followState = AIState.Idle;
			}
		}
		if(distancea > approachDistance + 0.5f){
			c = 0;
		}
	}
	
	IEnumerator MeleeDash(){
		meleefwd = true;
		yield return new WaitForSeconds(0.2f);
		meleefwd = false;
		if(GetComponent<Rigidbody2D>().gravityScale > 0){
			GetComponent<Rigidbody2D>().velocity = new Vector2(0 , GetComponent<Rigidbody2D>().velocity.y);
		}else{
			GetComponent<Rigidbody2D>().velocity = Vector2.zero;
		}
	}
	
	private bool onSkill = false;
	private float wait = 0;
	private GameObject eff;
	private bool onMoving = false;
	private Vector3 movDir = Vector3.zero;
	private float movSpd = 5;
	private bool stability = false;
	public float skillDistance = 5.5f;
	private float skillDelay = 5.5f;
	private bool fwdSkill = false;
	
	IEnumerator UseSkill(){
		if(!GetComponent<Status>().freeze){
			int s = 0;
			if(skill.Length > 1){
				s = Random.Range(0 , skill.Length);
			}
			onSkill = true;
			attacking = true;
			stat.stability = true;
			cancelAttack = false;
			fwdSkill = false;
			
			if(aimAtTarget && followTarget){
				Vector3 dir = followTarget.position - attackPoint.position;
				float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
				attackPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
			}
			//Moving
			if(skill[s].moveDirection != DirectionSet.None && followTarget){
				
				if(anim){
					if(skill[s].moveAnimTrigger != ""){
						anim.SetTrigger(skill[s].moveAnimTrigger);
					}else{
						anim.SetBool("run" , true);
					}
				}
				LookAtTarget();
				if(skill[s].moveDirection == DirectionSet.Forward){
					fwdSkill = true;
					movDir = attackPoint.TransformDirection(Vector3.right);
				}
				if(skill[s].moveDirection == DirectionSet.Backward){
					movDir = attackPoint.TransformDirection(Vector3.left);
				}
				if(skill[s].moveDirection == DirectionSet.Up){
					movDir = Vector3.up;
				}
				if(skill[s].moveDirection == DirectionSet.Down){
					movDir = Vector3.down;
				}
				movSpd = skill[s].moveSpeed;
				
				onMoving = true;
				yield return new WaitForSeconds(skill[s].moveDuration);
				onMoving = false;
				GetComponent<Rigidbody2D>().velocity = Vector2.zero;
				if(anim){
					anim.SetBool("run" , false);
				}
			}
			
			//Cast Effect
			if(skill[s].castEffect && followTarget){
				if(anim && skill[s].castAnimationTrigger != ""){
					anim.SetTrigger(skill[s].castAnimationTrigger);
				}
				eff = Instantiate(skill[s].castEffect , transform.position , Quaternion.identity) as GameObject;
				eff.transform.parent = this.transform;
				yield return new WaitForSeconds(skill[s].castEffectTime);
			}
			//Call UseSkill Function in AIsetC Script.
			
			for(int a = 0; a < skill[s].repeatBullet; a++){
				if(!stat.freeze){
					if(anim && skill[s].skillAnimationTrigger != ""){
						anim.SetTrigger(skill[s].skillAnimationTrigger);
					}
					LookAtTarget();
					yield return new WaitForSeconds(skill[s].castTime);
					if(aimAtTarget && followTarget){
						Vector3 dir = followTarget.position - attackPoint.position;
						float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
						attackPoint.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
					}
					
					Transform bulletShootout = Instantiate(skill[s].skillPrefab.transform , attackPoint.position , attackPoint.rotation) as Transform;
					bulletShootout.gameObject.SetActive(true);
					bulletShootout.GetComponent<BulletStatus>().Setting(stat.atk , stat.matk , "Player" , this.gameObject);
					if(skill[s].spawnAtPlayer && followTarget){
						bulletShootout.position = followTarget.position;
					}
					//print(a);
					if(a < skill[s].repeatBullet -1){
						yield return new WaitForSeconds(skill[s].repeatDelay);
					}
				}else{
					onSkill = false;
					attacking = false;
					a = skill[s].repeatBullet;
					skillDelay = skill[s].allSkillDelay;
					fwdSkill = false;
					if(!stability){
						stat.stability = false;
					}
					if(eff){
						Destroy(eff);
					}
					yield break;
				}
			}
			yield return new WaitForSeconds(skill[s].castTime);
			if(eff){
				Destroy(eff);
			}
			yield return new WaitForSeconds(skill[s].delayTime);
			onSkill = false;
			attacking = false;
			fwdSkill = false;
			if(!stability){
				stat.stability = false;
			}
			skillDelay = skill[s].allSkillDelay;

			CheckDistance();
		}
	}
	//-----------
}
