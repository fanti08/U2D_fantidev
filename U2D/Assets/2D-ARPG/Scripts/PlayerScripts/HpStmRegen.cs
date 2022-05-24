using UnityEngine;

public class HpStmRegen : MonoBehaviour {
	public int hpRegen = 0;
	public int stmRegen = 3;
	public float hpRegenDelay = 3.0f;
	public float stmRegenDelay = 3.0f;
	
	private float hpTime = 0.0f;
	private float stmTime = 0.0f;
	private Status stat;
	
	void Start(){
		stat = GetComponent<Status>();
	}
	
	void Update(){
		if(hpRegen > 0 && stat.health < stat.totalStat.health){
			if(hpTime >= hpRegenDelay){
				HPRecovery();
			}else{
				hpTime += Time.deltaTime;
			}
		}
		//----------------------------------------------------
		if(stmRegen > 0 && stat.stamina < stat.totalStat.stamina){
			if(stmTime >= stmRegenDelay){
				STMRecovery();
			}else{
				stmTime += Time.deltaTime;
			}
		}
	}
	
	void HPRecovery(){
		int amount = stat.totalStat.health * hpRegen / 100;
		if(amount <= 1){
			amount = 1;
		}
		stat.health += amount;
		hpTime = 0.0f;
		if(stat.health >= stat.totalStat.health){
			stat.health = stat.totalStat.health;
		}
	}
	
	void STMRecovery(){
		int amount = stat.totalStat.stamina * stmRegen / 100;
		if(amount <= 1){
			amount = 1;
		}
		stat.stamina += amount;
		stmTime = 0.0f;
		if(stat.stamina >= stat.totalStat.stamina){
			stat.stamina = stat.totalStat.stamina;
		}
	}
}
