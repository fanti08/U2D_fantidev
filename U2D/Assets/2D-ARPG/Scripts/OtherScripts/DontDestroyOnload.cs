using UnityEngine;

public class DontDestroyOnload : MonoBehaviour {

	void Awake(){
		DontDestroyOnLoad(transform.gameObject);
	}
}
