using General.Model;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Graph;
using Util.Geometry.Polygon;

public class MoveArmy : MonoBehaviour {

	private Vector2 speed = new Vector2(1.5f, 1.5f);
	private Vector3 startPos = new Vector3(-11f, -3.5f, -10f);
	private Rigidbody2D _body;
	private BoxCollider2D _box;
	private Material m_LineMaterial;
	public List<Vertex> path = new List<Vertex>();
	private static float epsilon = 0.5f;
	private Polygon2DMesh myMesh;
	public Polygon2D myPolygon = new Polygon2D();

	// Use this for initialization
	void Start () {
		_body = GetComponent<Rigidbody2D>();
		_box = GetComponent<BoxCollider2D>();

		//initialize the polygon and its mesh
		Polygon2D polygon = new Polygon2D();
		polygon.AddVertex(new Vector2(-1, -1));
		polygon.AddVertex(new Vector2(-1, 1));
		polygon.AddVertex(new Vector2(1, 1));
		polygon.AddVertex(new Vector2(1, -1));
		myMesh = gameObject.GetComponent<Polygon2DMesh>();
		myMesh.Polygon = polygon;
		myPolygon = polygon;
	}

	void Awake()
	{
		Shader shader = Shader.Find("Hidden/Internal-Colored");
		m_LineMaterial = new Material(shader)
		{
			hideFlags = HideFlags.HideAndDontSave
		};
		m_LineMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
		m_LineMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
		m_LineMaterial.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
		m_LineMaterial.SetInt("_ZWrite", 0);
	}

	// Update is called once per frame
	void Update() {
		float deltaX = Input.GetAxis("Horizontal") * speed.x;
		float deltaY = Input.GetAxis("Vertical") * speed.y;

		// Follow a path
		Vertex firstOnPath = path.Count > 0 ? path[0] : null;
		if (firstOnPath != null)
        {
			if (Vector2.Distance((Vector2)_body.transform.position, firstOnPath.Pos) <= epsilon)
			{
				path.RemoveAt(0);
			}
			else
			{
				deltaX =  firstOnPath.Pos.x - _body.transform.position.x;
				deltaY =  firstOnPath.Pos.y - _body.transform.position.y;
			}
		}

		Vector2 movement = new Vector2(deltaX, deltaY);
		movement.Normalize();
		movement.Scale(speed);
		_body.velocity = movement;

		DrawPath();

	}

	private void OnRenderObject()
	{
		// Apply the line material
		m_LineMaterial.SetPass(0);
		DrawPath();
	}

	private void DrawPath()
    {
		if (path.Count == 0) return;
		GL.Begin(GL.LINE_STRIP);
		GL.Color(Color.white);
		GL.Vertex(_body.transform.position);
		foreach (Vertex v in path)
        {
			GL.Vertex(new Vector3(v.Pos.x, v.Pos.y, 0));
		}
		GL.End();
    }
}
