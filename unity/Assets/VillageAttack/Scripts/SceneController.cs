using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;

public class SceneController : MonoBehaviour {
	[SerializeField] public GameObject[] MountainPrefabs;
	private GameObject _mountain;
	private int _current_prefab = 0;

	// Use this for initialization
	void Start () {
		
	}

	public void SetCurrentPrefab(int next_prefab){
		_current_prefab = next_prefab;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0)){
			Vector3 pos = Input.mousePosition;
			Debug.Log("We are clicking somewhere" + pos);
			Vector3 pos_world = Camera.main.ScreenToWorldPoint(
						new Vector3(Input.mousePosition.x, 
						Input.mousePosition.y, 
						Camera.main.nearClipPlane)
						);
			
			if (pos_world[0]> -15)
			{
			_mountain = Instantiate(MountainPrefabs[this._current_prefab]) as GameObject;
			_mountain.transform.position = new Vector3(pos_world[0], pos_world[1], pos_world[2]);
			// _mountain.transform.localScale = new Vector3(1, 1,);
			}
			
			

		}
	}
}
