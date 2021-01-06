using General.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Graph;
using Util.Geometry.Polygon;

public class MountainBehaviour : MonoBehaviour {
	[SerializeField] private SceneController controller;
	[SerializeField] private Vector2 scale;
	private Rigidbody2D _body;
	private BoxCollider2D _box;
	public Polygon2DMesh myMesh;
	public Polygon2D myPolygon = new Polygon2D();
	// Use this for initialization

	void Start () {
	
	}
	// Update is called once per frame
	void Update () {
		
	}

	public void SetPolygon(Vector3 worldPosition) {
		// _body = GetComponent<Rigidbody2D>();
		// _box = GetComponent<BoxCollider2D>();

		Polygon2D polygon = new Polygon2D();
		myMesh = gameObject.GetComponent<Polygon2DMesh>();
		Debug.Log("Hallo from Set Polygon " + transform.position);

		polygon.AddVertex(new Vector2(worldPosition[0]-1*scale[1], worldPosition[1]));
		polygon.AddVertex(new Vector2(worldPosition[0], worldPosition[1]+1*scale[1]));
		polygon.AddVertex(new Vector2(worldPosition[0]+1*scale[0], worldPosition[1]));

		myMesh.Polygon = polygon;
		myPolygon = polygon;
		Debug.Log("Hallo from Set Polygon " + myMesh.Polygon);

	}

	// void UpdatePolygon(Vector2 worldPosition) {
	// 	foreach(Vector2 vector in shape) {
	// 		myPolygon.AddVertex(new Vector2(vector[0]+worldPosition[0], vector[1]+worldPosition[1]));
	// 	}

	// }

	// public void SetLocation(Vector3 location) {
	// 	this.transform.position = location;
	// }
}
