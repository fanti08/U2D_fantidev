﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Inventory : MonoBehaviour{
	public ItemData database;
	public int cash = 500;
	public int[] itemSlot = new int[16];
	public int[] itemQuantity = new int[16];
	public int[] equipment = new int[8];

	public int weaponEquip = 0;
	public bool allowWeaponUnequip = false;
	public int subWeaponEquip = 0;
	public int armorEquip = 0;
	public int hatEquip = 0;
	public int glovesEquip = 0;
	public int bootsEquip = 0;
	public int accessoryEquip = 0;

	void Start(){
		InitialSetting();
	}

	public void InitialSetting(){
		if(weaponEquip > 0){
			int tempEq = weaponEquip;
			weaponEquip = 0;
			EquipItem(tempEq , 9999);
		}
		//Reset Power of Current Weapon & Armor
		SettingEquipmentStatus();
		StartCoroutine(DelayUpdateUI());
	}
	
	IEnumerator DelayUpdateUI(){
		yield return new WaitForSeconds(0.05f);
		UpdateAmmoUI();
	}

	public void UseItem(int slot){
		int id = itemSlot[slot];
		if(database.usableItem[id].unusable){
			return;
		}
		GetComponent<Status>().Heal(database.usableItem[id].hpRecover , database.usableItem[id].stmRecover);
		if(database.usableItem[id].sendMsg != ""){
			SendMessage(database.usableItem[id].sendMsg , SendMessageOptions.DontRequireReceiver);
		}
		if(database.usableItem[id].useEffect){
			Instantiate(database.usableItem[id].useEffect , transform.position , transform.rotation);
		}
		if(database.usableItem[id].attackItem.enable){
			//-------
		}
		if(!database.usableItem[id].unlimited){
			itemQuantity[slot]--;
		}
		if(itemQuantity[slot] <= 0){
			itemSlot[slot] = 0;
			itemQuantity[slot] = 0;
		}
		AutoSortItem();
		GetComponent<AttackTrigger>().UpdateShortcut();
	}

	public void UseItemFromID(int id){
		int slot = FindItemSlot(id);
		if(slot < itemSlot.Length){
			UseItem(slot);
		}
	}

	public void EquipItemFromID(int id){
		int slot = FindEquipmentSlot(id);
		if(slot < equipment.Length){
			EquipItem(id , slot);
		}
	}

	public void EquipItem(int id , int slot){
		if(id == 0){
			return;
		}
		//Backup Your Current Equipment before Unequip
		int tempEquipment = 0;
		
		if((int)database.equipment[id].EquipmentType == 0){//Equipment = Weapon
			if(GetComponent<Status>().block){
				GetComponent<Status>().GuardBreak("cancelGuard");
			}
			//Weapon Type
			tempEquipment = weaponEquip;
			weaponEquip = id;
			if(database.equipment[id].attackPrefab.Length > 0){
				GetComponent<AttackTrigger>().attackPrefab = new BulletStatus[database.equipment[id].attackPrefab.Length];
				for(int a = 0; a < database.equipment[id].attackPrefab.Length; a++){
					//GetComponent<AttackTrigger>().attackPrefab[a] = new BulletStatus();
					GetComponent<AttackTrigger>().attackPrefab[a] = database.equipment[id].attackPrefab[a];
				}
			}
			GetComponent<AttackTrigger>().weaponType = database.equipment[id].weaponType;
			int reqId = database.equipment[id].requireItemId;
			GetComponent<AttackTrigger>().requireItemId = reqId;
			GetComponent<AttackTrigger>().requireItemName = database.usableItem[reqId].itemName;
			GetComponent<AttackTrigger>().attackSoundEffect = database.equipment[id].soundEffect;

			GetComponent<AttackTrigger>().attackAnimationTrigger = database.equipment[id].attackAnimationTrigger;
			GetComponent<AttackTrigger>().blockingAnimationTrigger = database.equipment[id].blockingAnimationTrigger;
			GetComponent<AttackTrigger>().whileAttack = database.equipment[id].whileAttack;
			GetComponent<AttackTrigger>().canBlock = database.equipment[id].canBlock;
			GetComponent<AttackTrigger>().charge = database.equipment[id].chargeAttack;
			GetComponent<AttackTrigger>().attackCast = database.equipment[id].attackCast;
			GetComponent<AttackTrigger>().attackDelay = database.equipment[id].attackDelay;
			GetComponent<AttackTrigger>().attackSoundEffect = database.equipment[id].soundEffect;
			//Update Show Ammo UI
			if(reqId > 0 && ShowAmmo.showAmmo){
				ShowAmmo.showAmmo.OnOffShowing(true);
				int sl = FindItemSlot(reqId);
				int am = 0;
				Sprite spr = database.usableItem[reqId].icon;
				if(sl < itemQuantity.Length){
					am = itemQuantity[sl];
				}	
				ShowAmmo.showAmmo.UpdateSprite(spr);
				ShowAmmo.showAmmo.UpdateAmmo(am);
			}else if(ShowAmmo.showAmmo){
				ShowAmmo.showAmmo.OnOffShowing(false);
			}
			if(WeaponTooltips.showWeaponTooltips){
				WeaponTooltips.showWeaponTooltips.SetTooltip(database.equipment[id].weaponType);
			}
		}else if((int)database.equipment[id].EquipmentType == 1){
			//Armor Type
			tempEquipment = armorEquip;
			armorEquip = id;
		}else if((int)database.equipment[id].EquipmentType == 2){
			//Accessory Type
			tempEquipment = accessoryEquip;
			accessoryEquip = id;
		}else if((int)database.equipment[id].EquipmentType == 3){
			//Headgear Type
			tempEquipment = hatEquip;
			hatEquip = id;
		}else if((int)database.equipment[id].EquipmentType == 4){
			//Gloves Type
			tempEquipment = glovesEquip;
			glovesEquip = id;
		}else if((int)database.equipment[id].EquipmentType == 5){
			//Boots Type
			tempEquipment = bootsEquip;
			bootsEquip = id;
		}
		if(slot <= equipment.Length){
			equipment[slot] = 0;
		}
		//Reset Power of Current Weapon & Armor
		SettingEquipmentStatus();
		AutoSortEquipment();
		AddEquipment(tempEquipment);
	}

	void SettingEquipmentStatus(){
		if(!GetComponent<Status>()){
			return;
		}
		Status stat = GetComponent<Status>();
		//Reset Power of Current Weapon & Armor
		//Set New Variable of Weapon
		stat.additionStat.atk = database.equipment[weaponEquip].attack;
		stat.additionStat.def = database.equipment[weaponEquip].defense;
		stat.additionStat.satk = database.equipment[weaponEquip].special;
		stat.additionStat.sdef = database.equipment[weaponEquip].magicDefense;
		stat.additionStat.health = database.equipment[weaponEquip].hpBonus;
		stat.additionStat.stamina = database.equipment[weaponEquip].mpBonus;
		//Set New Variable of Armor
		stat.additionStat.atk += database.equipment[armorEquip].attack;
		stat.additionStat.def += database.equipment[armorEquip].defense;
		stat.additionStat.satk += database.equipment[armorEquip].special;
		stat.additionStat.sdef += database.equipment[armorEquip].magicDefense;
		stat.additionStat.health += database.equipment[armorEquip].hpBonus;
		stat.additionStat.stamina += database.equipment[armorEquip].mpBonus;
		//Set New Variable of Hat
		stat.additionStat.atk += database.equipment[hatEquip].attack;
		stat.additionStat.def += database.equipment[hatEquip].defense;
		stat.additionStat.satk += database.equipment[hatEquip].special;
		stat.additionStat.sdef += database.equipment[hatEquip].magicDefense;
		stat.additionStat.health += database.equipment[hatEquip].hpBonus;
		stat.additionStat.stamina += database.equipment[hatEquip].mpBonus;
		//Set New Variable of Gloves
		stat.additionStat.atk += database.equipment[glovesEquip].attack;
		stat.additionStat.def += database.equipment[glovesEquip].defense;
		stat.additionStat.satk += database.equipment[glovesEquip].special;
		stat.additionStat.sdef += database.equipment[glovesEquip].magicDefense;
		stat.additionStat.health += database.equipment[glovesEquip].hpBonus;
		stat.additionStat.stamina += database.equipment[glovesEquip].mpBonus;
		//Set New Variable of Boots
		stat.additionStat.atk += database.equipment[bootsEquip].attack;
		stat.additionStat.def += database.equipment[bootsEquip].defense;
		stat.additionStat.satk += database.equipment[bootsEquip].special;
		stat.additionStat.sdef += database.equipment[bootsEquip].magicDefense;
		stat.additionStat.health += database.equipment[bootsEquip].hpBonus;
		stat.additionStat.stamina += database.equipment[bootsEquip].mpBonus;
		//Set New Variable of Accessory
		stat.additionStat.atk += database.equipment[accessoryEquip].attack;
		stat.additionStat.def += database.equipment[accessoryEquip].defense;
		stat.additionStat.satk += database.equipment[accessoryEquip].special;
		stat.additionStat.sdef += database.equipment[accessoryEquip].magicDefense;
		stat.additionStat.health += database.equipment[accessoryEquip].hpBonus;
		stat.additionStat.stamina += database.equipment[accessoryEquip].mpBonus;
		//Elemental Resist
		stat.eqElemental.normal = database.equipment[weaponEquip].elementlResistance.normal + database.equipment[armorEquip].elementlResistance.normal + database.equipment[accessoryEquip].elementlResistance.normal + database.equipment[hatEquip].elementlResistance.normal + database.equipment[glovesEquip].elementlResistance.normal + database.equipment[bootsEquip].elementlResistance.normal;
		stat.eqElemental.fire = database.equipment[weaponEquip].elementlResistance.fire + database.equipment[armorEquip].elementlResistance.fire + database.equipment[accessoryEquip].elementlResistance.fire + database.equipment[hatEquip].elementlResistance.fire + database.equipment[glovesEquip].elementlResistance.fire + database.equipment[bootsEquip].elementlResistance.fire;
		stat.eqElemental.ice = database.equipment[weaponEquip].elementlResistance.ice + database.equipment[armorEquip].elementlResistance.ice + database.equipment[accessoryEquip].elementlResistance.ice + database.equipment[hatEquip].elementlResistance.ice + database.equipment[glovesEquip].elementlResistance.ice + database.equipment[bootsEquip].elementlResistance.ice;
		stat.eqElemental.thunder = database.equipment[weaponEquip].elementlResistance.thunder + database.equipment[armorEquip].elementlResistance.thunder + database.equipment[accessoryEquip].elementlResistance.thunder + database.equipment[hatEquip].elementlResistance.thunder + database.equipment[glovesEquip].elementlResistance.thunder + database.equipment[bootsEquip].elementlResistance.thunder;
		stat.eqElemental.earth = database.equipment[weaponEquip].elementlResistance.earth + database.equipment[armorEquip].elementlResistance.earth + database.equipment[accessoryEquip].elementlResistance.earth + database.equipment[hatEquip].elementlResistance.earth + database.equipment[glovesEquip].elementlResistance.earth + database.equipment[bootsEquip].elementlResistance.earth;
		stat.eqElemental.poison = database.equipment[weaponEquip].elementlResistance.poison + database.equipment[armorEquip].elementlResistance.poison + database.equipment[accessoryEquip].elementlResistance.poison + database.equipment[hatEquip].elementlResistance.poison + database.equipment[glovesEquip].elementlResistance.poison + database.equipment[bootsEquip].elementlResistance.poison;
		stat.eqElemental.wind = database.equipment[weaponEquip].elementlResistance.wind + database.equipment[armorEquip].elementlResistance.wind + database.equipment[accessoryEquip].elementlResistance.wind + database.equipment[hatEquip].elementlResistance.wind + database.equipment[glovesEquip].elementlResistance.wind + database.equipment[bootsEquip].elementlResistance.wind;
		stat.eqElemental.holy = database.equipment[weaponEquip].elementlResistance.holy + database.equipment[armorEquip].elementlResistance.holy + database.equipment[accessoryEquip].elementlResistance.holy + database.equipment[hatEquip].elementlResistance.holy + database.equipment[glovesEquip].elementlResistance.holy + database.equipment[bootsEquip].elementlResistance.holy;
		stat.eqElemental.darkness = database.equipment[weaponEquip].elementlResistance.darkness + database.equipment[armorEquip].elementlResistance.darkness + database.equipment[accessoryEquip].elementlResistance.darkness + database.equipment[hatEquip].elementlResistance.darkness + database.equipment[glovesEquip].elementlResistance.darkness + database.equipment[bootsEquip].elementlResistance.darkness;
		//Status Resist
		stat.eqResist.poisonResist = database.equipment[weaponEquip].statusResist.poisonResist + database.equipment[armorEquip].statusResist.poisonResist + database.equipment[accessoryEquip].statusResist.poisonResist + database.equipment[hatEquip].statusResist.poisonResist + database.equipment[glovesEquip].statusResist.poisonResist + database.equipment[bootsEquip].statusResist.poisonResist;
		stat.eqResist.stunResist = database.equipment[weaponEquip].statusResist.stunResist + database.equipment[armorEquip].statusResist.stunResist + database.equipment[accessoryEquip].statusResist.stunResist + database.equipment[hatEquip].statusResist.stunResist + database.equipment[glovesEquip].statusResist.stunResist + database.equipment[bootsEquip].statusResist.stunResist;
		stat.eqResist.silenceResist = database.equipment[weaponEquip].statusResist.silenceResist + database.equipment[armorEquip].statusResist.silenceResist + database.equipment[accessoryEquip].statusResist.silenceResist + database.equipment[hatEquip].statusResist.silenceResist + database.equipment[glovesEquip].statusResist.silenceResist + database.equipment[bootsEquip].statusResist.silenceResist;
		stat.eqResist.frozenResist = database.equipment[weaponEquip].statusResist.frozenResist + database.equipment[armorEquip].statusResist.frozenResist + database.equipment[accessoryEquip].statusResist.frozenResist + database.equipment[hatEquip].statusResist.frozenResist + database.equipment[glovesEquip].statusResist.frozenResist + database.equipment[bootsEquip].statusResist.frozenResist;
		
		stat.hiddenStatus.doubleJump = false;
		if(database.equipment[weaponEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		if(database.equipment[armorEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		if(database.equipment[hatEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		if(database.equipment[glovesEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		if(database.equipment[bootsEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		if(database.equipment[accessoryEquip].canDoubleJump){
			stat.hiddenStatus.doubleJump = true;
		}
		stat.hiddenStatus.autoGuard = database.equipment[weaponEquip].autoGuard + database.equipment[armorEquip].autoGuard + database.equipment[accessoryEquip].autoGuard + database.equipment[hatEquip].autoGuard + database.equipment[glovesEquip].autoGuard + database.equipment[bootsEquip].autoGuard;
		stat.hiddenStatus.drainTouch = database.equipment[weaponEquip].drainTouch + database.equipment[armorEquip].drainTouch + database.equipment[accessoryEquip].drainTouch + database.equipment[hatEquip].drainTouch + database.equipment[glovesEquip].drainTouch + database.equipment[bootsEquip].drainTouch;
		stat.hiddenStatus.stmReduce = database.equipment[weaponEquip].stmReduce + database.equipment[armorEquip].stmReduce + database.equipment[accessoryEquip].stmReduce + database.equipment[hatEquip].stmReduce + database.equipment[glovesEquip].stmReduce + database.equipment[bootsEquip].stmReduce;
		
		stat.CalculateStatus();
	}

	public void SwapWeapon(){
		int tempEq = weaponEquip; //Store Main Weapon Data
		
		if(subWeaponEquip == 0){
			//Use Unequip Instead if no Sub Weapon equipped
			weaponEquip = 0; // Set to 0 because we didn't want to add it to inventory after swap.
			UnEquip(0);
			subWeaponEquip = tempEq;
			return;
		}
		weaponEquip = 0; // Set to 0 because we didn't want to add it to inventory after swap.
		EquipItem(subWeaponEquip , equipment.Length + 10);
		subWeaponEquip = tempEq;
	}

	public void UnEquip(int id){
		bool full = false;
		if((int)database.equipment[id].EquipmentType == 0){
			full = AddEquipment(weaponEquip);
		}else if((int)database.equipment[id].EquipmentType == 1){
			full = AddEquipment(armorEquip);
		}else if((int)database.equipment[id].EquipmentType == 2){
			full = AddEquipment(accessoryEquip);
		}else if((int)database.equipment[id].EquipmentType == 3){
			full = AddEquipment(hatEquip);
		}else if((int)database.equipment[id].EquipmentType == 4){
			full = AddEquipment(glovesEquip);
		}else if((int)database.equipment[id].EquipmentType == 5){
			full = AddEquipment(bootsEquip);
		}
		if(!full){
			if((int)database.equipment[id].EquipmentType == 0){
				if(GetComponent<Status>().block){
					GetComponent<Status>().GuardBreak("cancelGuard");
				}
				weaponEquip = 0;
				
				GetComponent<AttackTrigger>().weaponType = database.equipment[0].weaponType;
				int reqId = 0;
				GetComponent<AttackTrigger>().requireItemId = reqId;
				GetComponent<AttackTrigger>().requireItemName = "";
				UpdateAmmoUI();
				GetComponent<AttackTrigger>().canBlock = database.equipment[0].canBlock;
				
				GetComponent<AttackTrigger>().attackPrefab = database.equipment[0].attackPrefab;
				GetComponent<AttackTrigger>().attackAnimationTrigger = database.equipment[0].attackAnimationTrigger;
				GetComponent<AttackTrigger>().blockingAnimationTrigger = database.equipment[0].blockingAnimationTrigger;
				GetComponent<AttackTrigger>().whileAttack = database.equipment[0].whileAttack;

				GetComponent<AttackTrigger>().attackSoundEffect = database.equipment[0].soundEffect;
				GetComponent<AttackTrigger>().charge = database.equipment[0].chargeAttack;
				if(WeaponTooltips.showWeaponTooltips){
					WeaponTooltips.showWeaponTooltips.SetTooltip(database.equipment[0].weaponType);
				}
			}else if((int)database.equipment[id].EquipmentType == 1){
				armorEquip = 0;
			}else if((int)database.equipment[id].EquipmentType == 2){
				accessoryEquip = 0;
			}else if((int)database.equipment[id].EquipmentType == 3){
				hatEquip = 0;
			}else if((int)database.equipment[id].EquipmentType == 4){
				glovesEquip = 0;
			}else if((int)database.equipment[id].EquipmentType == 5){
				bootsEquip = 0;
			}
			//Reset Power of Current Weapon & Armor
			SettingEquipmentStatus();
		} 
	}

	public bool AddItem(int id , int quan){
		bool full = false;
		bool geta = false;
		
		int pt = 0;
		while(pt < itemSlot.Length && !geta){
			if(itemSlot[pt] == id){
				itemQuantity[pt] += quan;
				geta = true;
			}else if(itemSlot[pt] == 0){
				itemSlot[pt] = id;
				itemQuantity[pt] = quan;
				geta = true;
			}else{
				pt++;
				if(pt >= itemSlot.Length){
					full = true;
					print("Full");
				}
			}
		}
		UpdateAmmoUI();
		
		int slot = FindItemSlot(id);
		if(slot < itemSlot.Length){
			if(itemQuantity[slot] <= 0){
				itemSlot[slot] = 0;
				itemQuantity[slot] = 0;
				AutoSortItem();
			}
		}
		GetComponent<AttackTrigger>().UpdateShortcut();
		return full;
	}
	
	public bool AddEquipment(int id){
		bool full = false;
		bool geta = false;
		
		int pt = 0;
		while(pt < equipment.Length && !geta){
			if(equipment[pt] == 0){
				equipment[pt] = id;
				geta = true;
			}else{
				pt++;
				if(pt >= equipment.Length){
					full = true;
					print("Full");
				}
			}
		}
		GetComponent<AttackTrigger>().UpdateShortcut();
		return full;
	}

	//------------AutoSort----------
	public void AutoSortItem(){
		int pt = 0;
		int nextp = 0;
		bool  clearr = false;
		while(pt < itemSlot.Length){
			if(itemSlot[pt] == 0){
				nextp = pt + 1;
				while(nextp < itemSlot.Length && !clearr){
					if(itemSlot[nextp] > 0){
						//Fine Next Item and Set
						itemSlot[pt] = itemSlot[nextp];
						itemQuantity[pt] = itemQuantity[nextp];
						itemSlot[nextp] = 0;
						itemQuantity[nextp] = 0;
						clearr = true;
					}else{
						nextp++;
					}
				}
				//Continue New Loop
				clearr = false;
				pt++;
			}else{
				pt++;
			}
		}
		UpdateAmmoUI();
	}
	
	public void AutoSortEquipment(){
		int pt = 0;
		int nextp = 0;
		bool  clearr = false;
		while(pt < equipment.Length){
			if(equipment[pt] == 0){
				nextp = pt + 1;
				while(nextp < equipment.Length && !clearr){
					if(equipment[nextp] > 0){
						//Fine Next Item and Set
						equipment[pt] = equipment[nextp];
						equipment[nextp] = 0;
						clearr = true;
					}else{
						nextp++;
					}
				}
				//Continue New Loop
				clearr = false;
				pt++;
			}else{
				pt++;
			}
		}
	}

	public bool CheckItem(int id , int type, int qty){
		bool having = false;
		bool geta = false;
		//type 0 = Usable , 1 = Equipment
		
		int pt = 0;
		
		//================Usable==================
		if(type == 0){
			while(pt < itemSlot.Length && !geta){
				if(itemSlot[pt] == id){
					if(itemQuantity[pt] >= qty){
						having = true;
					}
					geta = true;
				}else{
					pt++;
				}
				//--------------------------
			}
		}
		//=================Equipment=================
		if(type == 1){
			while(pt < equipment.Length && !geta){
				if(equipment[pt] == id){
					having = true;
					geta = true;
				}else{
					pt++;
				}
				//--------------------------
			}
		}
		return having;
	}

	public int FindItemSlot(int id){
		bool geta = false;
		int pt = 0;
		while(pt < itemSlot.Length && !geta){
			if(itemSlot[pt] == id){
				geta = true;
			}else{
				pt++;
				if(pt >= itemSlot.Length){
					pt = itemSlot.Length + 50;//No Item
					print("No Item");
				}
			}
		}
		return pt;
	}
	
	public int FindEquipmentSlot(int id){
		bool geta = false;
		int pt = 0;
		while(pt < equipment.Length && !geta){
			if(equipment[pt] == id){
				geta = true;
			}else{
				pt++;
				if(pt >= equipment.Length){
					pt = equipment.Length + 50;//No Item
					print("No Item");
				}
			}
		}
		return pt;
	}

	public bool RemoveItem(int id , int amount){
		bool haveItem = false;
		int slot = FindItemSlot(id);
		if(slot < itemSlot.Length){
			if(itemQuantity[slot] > 0){
				itemQuantity[slot] -= amount;
				haveItem = true;
			}
			if(itemQuantity[slot] <= 0){
				itemSlot[slot] = 0;
				itemQuantity[slot] = 0;
				AutoSortItem();
			}
		}
		UpdateAmmoUI();
		GetComponent<AttackTrigger>().UpdateShortcut();
		return haveItem;
	}

	public bool RemoveEquipment(int id){
		bool haveItem = false;
		int slot = FindEquipmentSlot(id);
		if(slot < equipment.Length){
			equipment[slot] = 0;
			AutoSortEquipment();
			haveItem = true;
		}
		GetComponent<AttackTrigger>().UpdateShortcut();
		return haveItem;
	}

	public void UpdateAmmoUI(){
		//Update Show Ammo UI
		if(!GetComponent<AttackTrigger>()){
			return;
		}
		int reqId = GetComponent<AttackTrigger>().requireItemId;

		if(reqId > 0 && ShowAmmo.showAmmo){
			ShowAmmo.showAmmo.OnOffShowing(true);
			int sl = FindItemSlot(reqId);
			int am = 0;
			//Sprite spr = database.usableItem[reqId].iconSprite;
			if(sl < itemQuantity.Length){
				am = itemQuantity[sl];
			}			
			//ShowAmmoC.showAmmo.UpdateSprite(spr);
			ShowAmmo.showAmmo.UpdateAmmo(am);
		}else if(ShowAmmo.showAmmo){
			ShowAmmo.showAmmo.OnOffShowing(false);
		}
	}
}
