using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Util.Geometry.Graph;

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
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Input.mousePosition;
			Debug.Log("We are clicking somewhere" + pos);
			Vector3 pos_world = Camera.main.ScreenToWorldPoint(
						new Vector3(Input.mousePosition.x,
						Input.mousePosition.y,
						Camera.main.nearClipPlane)
						);
			CreateMountain(pos_world);
		} else if (Input.GetMouseButtonUp(1)) {
			Vector3 pos = Input.mousePosition;
			Vector3 pos_world = Camera.main.ScreenToWorldPoint(
						new Vector3(Input.mousePosition.x,
						Input.mousePosition.y,
						Camera.main.nearClipPlane)
						);
			AddNewPathVertex(pos_world);
		}
	}

	private void AddNewPathVertex(Vector3 worldPosition)
    {
		Vertex vertex = new Vertex(worldPosition.x, worldPosition.y);
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		Debug.Log(army.name);
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();
		armyComponent.path.Add(vertex);
    }

	private void CreateMountain(Vector3 worldPosition)
    {
		if (worldPosition[0] > -15)
		{
			_mountain = Instantiate(MountainPrefabs[this._current_prefab]) as GameObject;
			_mountain.transform.position = new Vector3(worldPosition[0], worldPosition[1], worldPosition[2]);
		}
	}
}
