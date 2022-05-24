using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour {
	Image hpBar;
	Image stmBar;
	Image expBar;
	Text hpText;
	Text stmText;
	Text lvText;
	public GameObject player;
	public SkillTreeUi skillTree;

	void Start(){
		if(!player){
			player = GameObject.FindWithTag("Player");
		}

		hpBar = GameObject.Find("HpGauge").GetComponent<Image>();
		stmBar = GameObject.Find("MpGauge").GetComponent<Image>();
		expBar = GameObject.Find("ExpGauge").GetComponent<Image>();
		hpText = GameObject.Find("HP-Text").GetComponent<Text>();
		stmText = GameObject.Find("MP-Text").GetComponent<Text>();
		lvText = GameObject.Find("LvText").GetComponent<Text>();
	}
	
	void Update(){
		if(!player){
			Destroy(gameObject);
			return;
		}
		Status stat = player.GetComponent<Status>();
		
		int maxHp = stat.totalStat.health;
		float hp = stat.health;
		int maxMp = stat.totalStat.stamina;
		float stm = stat.stamina;
		int exp = stat.exp;
		float maxExp = stat.maxExp;
		float curHp = hp/maxHp;
		float curMp = stm/maxMp;
		float curExp = exp/maxExp;

		//HP Gauge
		if(curHp > hpBar.fillAmount){
			hpBar.fillAmount += 1 / 1 * Time.unscaledDeltaTime;
			if(hpBar.fillAmount > curHp){
				hpBar.fillAmount = curHp;
			}
		}	
		if(curHp < hpBar.fillAmount){
			hpBar.fillAmount -= 1 / 1 * Time.unscaledDeltaTime;
			if(hpBar.fillAmount < curHp){
				hpBar.fillAmount = curHp;
			}
		}
		
		//STM Gauge
		if(curMp > stmBar.fillAmount){
			stmBar.fillAmount += 1 / 1 * Time.unscaledDeltaTime;
			if(stmBar.fillAmount > curMp){
				stmBar.fillAmount = curMp;
			}
		}	
		if(curMp < stmBar.fillAmount){
			stmBar.fillAmount -= 1 / 1 * Time.unscaledDeltaTime;
			if(stmBar.fillAmount < curMp){
				stmBar.fillAmount = curMp;
			}
		}
		
		//EXP Gauge
		if(expBar){
			expBar.fillAmount = curExp;
		}
		if(lvText){
			lvText.text = stat.level.ToString();
		}
		if(hpText){
			hpText.text = hp.ToString() + "/" + maxHp.ToString();
		}
		if(stmText){
			stmText.text = stm.ToString() + "/" + maxMp.ToString();
		}
	}
}
