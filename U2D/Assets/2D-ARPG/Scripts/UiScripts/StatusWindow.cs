using UnityEngine;
using UnityEngine.UI;

public class StatusWindow : MonoBehaviour {
	public GameObject player;
	public Text charName;
	public Text lv;
	public Text atk;
	public Text def;
	public Text satk;
	public Text sdef;
	public Text exp;
	public Text nextLv;
	public Text stPoint;
	
	public Text totalAtk;
	public Text totalDef;
	public Text totalSatk;
	public Text totalSdef;
	
	public Button atkUpButton;
	public Button defUpButton;
	public Button satkUpButton;
	public Button sdefUpButton;
	
	void Start(){
		if(!player){
			player = GameObject.FindWithTag("Player");
		}
	}
	
	void Update(){
		if(!player){
			Destroy(gameObject);
			return;
		}
		Status stat = player.GetComponent<Status>();
		if(charName){
			charName.text = stat.characterName.ToString();
		}
		if(lv){
			lv.text = stat.level.ToString();
		}
		if(atk){
			atk.text = stat.atk.ToString();
		}
		if(def){
			def.text = stat.def.ToString();
		}
		if(satk){
			satk.text = stat.satk.ToString();
		}
		if(sdef){
			sdef.text = stat.sdef.ToString();
		}
		
		if(exp){
			exp.text = stat.exp.ToString();
		}
		if(nextLv){
			nextLv.text = stat.maxExp.ToString();
		}
		if(stPoint){
			stPoint.text = stat.statusPoint.ToString();
		}
		
		if(totalAtk){
			totalAtk.text = "(" + stat.totalStat.atk.ToString() + ")";
		}
		if(totalDef){
			totalDef.text = "(" + stat.totalStat.def.ToString() + ")";
		}
		if(totalSatk){
			totalSatk.text = "(" + stat.totalStat.satk.ToString() + ")";
		}
		if(totalSdef){
			totalSdef.text = "(" + stat.totalStat.sdef.ToString() + ")";
		}
		
		if(stat.statusPoint > 0){
			if(atkUpButton)
				atkUpButton.gameObject.SetActive(true);
			if(defUpButton)
				defUpButton.gameObject.SetActive(true);
			if(satkUpButton)
				satkUpButton.gameObject.SetActive(true);
			if(sdefUpButton)
				sdefUpButton.gameObject.SetActive(true);
		}else{
			if(atkUpButton)
				atkUpButton.gameObject.SetActive(false);
			if(defUpButton)
				defUpButton.gameObject.SetActive(false);
			if(satkUpButton)
				satkUpButton.gameObject.SetActive(false);
			if(sdefUpButton)
				sdefUpButton.gameObject.SetActive(false);
		}
		
	}
	
	public void UpgradeStatus(int statusId){
		//0 = Atk , 1 = Def , 2 = Satk , 3 = Sdef
		if(!player){
			return;
		}
		Status stat = player.GetComponent<Status>();
		if(statusId == 0 && stat.statusPoint > 0){
			stat.atk += 1;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 1 && stat.statusPoint > 0){
			stat.def += 1;
			stat.maxHealth += 5;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 2 && stat.statusPoint > 0){
			stat.satk += 1;
			stat.maxStamina += 3;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
		if(statusId == 3 && stat.statusPoint > 0){
			stat.sdef += 1;
			stat.statusPoint -= 1;
			stat.CalculateStatus();
		}
	}
	
	public void CloseMenu(){
		Time.timeScale = 1.0f;
		//Cursor.lockState = CursorLockMode.Locked;
		//Cursor.visible = false;
		GlobalStatus.menuOn = false;
		gameObject.SetActive(false);
	}
}
