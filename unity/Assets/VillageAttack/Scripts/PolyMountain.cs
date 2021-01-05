using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Polygon;
using General.Model;

public class PolyMountain : MonoBehaviour {

	public Polygon2D Polygon
	{
		get { return myPolygon; }
		set {
			myPolygon = value;
			Polygon2DMesh myMesh = gameObject.GetComponent<Polygon2DMesh>();
			myMesh.Polygon = value;
		}
	}
	private Polygon2D myPolygon;

	// Use this for initialization
	void Start () {
	
	}
	
	void Awake()
    {
		/*Polygon2D polygon = new Polygon2D();
        polygon.AddVertex(new Vector2(0, 0));
        polygon.AddVertex(new Vector2(5, 5));
        polygon.AddVertex(new Vector2(7, 7));
        polygon.AddVertex(new Vector2(10, 0));
		Polygon2DMesh myMesh = gameObject.GetComponent<Polygon2DMesh>();
		myMesh.Polygon = polygon;
		myPolygon = polygon;*/
	}

	// Update is called once per frame
	void Update () {

	}
}
