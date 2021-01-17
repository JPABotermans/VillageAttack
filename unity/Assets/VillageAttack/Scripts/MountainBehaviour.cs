using General.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Graph;
using Util.Geometry.Polygon;

public class MountainBehaviour : MonoBehaviour {
	[SerializeField] private SceneController controller;
	[SerializeField] public Vector2 scale;
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
		Polygon2D polygon = new Polygon2D();
		myMesh = gameObject.GetComponent<Polygon2DMesh>();

		polygon.AddVertex(new Vector2(worldPosition[0] - 0.5f*scale[0], worldPosition[1] - 0.5f));
		polygon.AddVertex(new Vector2(worldPosition[0], worldPosition[1] + scale[1] - 0.5f));
		polygon.AddVertex(new Vector2(worldPosition[0] + scale[0], worldPosition[1] - 0.5f));

		myMesh.Polygon = polygon;
		myPolygon = polygon;
	}

}
