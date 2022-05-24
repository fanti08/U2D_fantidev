using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BulletStatus))]

public class SummonSkill : MonoBehaviour {
	public AllyAi summonPrefab;

	[Tooltip("Summon can get stronger by player's level.")]
	public bool autoSetLvStatus = false; //Summon can get stronger by player's level.
	private int currentLv = 1;
	public int maxLevel = 100;
	
	public StatusParam minStatus;
	public StatusParam maxStatus;
	
	private int min = 0;
	private int max = 0;
	
	void Start(){
		GameObject source = GetComponent<BulletStatus>().shooter;
		if(source.GetComponent<AttackTrigger>().minion){
			source.GetComponent<AttackTrigger>().minion.GetComponent<Status>().Death();
		}
		Vector3 spawnPoint = transform.position;
		spawnPoint.x += Random.Range(-3 , 3);
		spawnPoint.y += Random.Range(1 , 1.5f);
		transform.position = spawnPoint;

		Transform mn = Instantiate(summonPrefab.transform , transform.position , source.transform.rotation) as Transform;
		if(mn.GetComponent<AllyAi>()){
			mn.GetComponent<AllyAi>().master = source.transform;
			mn.GetComponent<AllyAi>().deadIfNoMaster = true;
		}
		Physics2D.IgnoreCollision(source.GetComponent<Collider2D>(), mn.GetComponent<Collider2D>());

		source.GetComponent<AttackTrigger>().minion = mn;
		if(autoSetLvStatus){
			currentLv = source.GetComponent<Status>().level;
			CalculateStatLv(mn);
		}
		Destroy(gameObject , 0.1f);
	}
	
	public void CalculateStatLv(Transform mn){
		Status stat = mn.GetComponent<Status>();
		//[min_stat*(max_lv-lv)/(max_lv- 1)] + [max_stat*(lv- 1)/(max_lv- 1)]
		
		//Atk
		min = minStatus.atk * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.atk * (currentLv - 1)/(maxLevel - 1);
		stat.atk = min + max;
		//Def
		min = minStatus.def * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.def * (currentLv - 1)/(maxLevel - 1);
		stat.def = min + max;
		//Satk
		min = minStatus.satk * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.satk * (currentLv - 1)/(maxLevel - 1);
		stat.satk = min + max;
		//Sdef
		min = minStatus.sdef * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.sdef * (currentLv - 1)/(maxLevel - 1);
		stat.sdef = min + max;
		
		//HP
		min = minStatus.maxHealth * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.maxHealth * (currentLv - 1)/(maxLevel - 1);
		stat.maxHealth = min + max;
		stat.health = stat.maxHealth;
		//STM
		min = minStatus.maxStamina * (maxLevel - currentLv)/(maxLevel - 1);
		max = maxStatus.maxStamina * (currentLv - 1)/(maxLevel - 1);
		stat.maxStamina = min + max;
		stat.stamina = stat.maxStamina;
	}
}

[System.Serializable]
public class StatusParam{
	public int atk = 5;
	public int def = 5;
	public int satk = 5;
	public int sdef = 5;
	public int maxHealth = 100;
	public int maxStamina = 100;
}
