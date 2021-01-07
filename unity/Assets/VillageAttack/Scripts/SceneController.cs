using System.Collections;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using UnityEngine;
using Util.Geometry.Graph;
using Util.Geometry.Polygon;
using General.UI;
using General.Model;
using Util.Geometry.Contour;
using UnityEngine.Assertions;
using Util.Geometry;
using System.Linq;
using Util.Algorithms.Polygon;
using Util.VisibilityGraph;
using Util.Geometry.Graph;

public class SceneController : MonoBehaviour {
	[SerializeField] public GameObject[] MountainPrefabs;
	[SerializeField] public GameObject[] buttons;

	[SerializeField] public GameObject village;
	private GameObject _mountain;
	private int _current_prefab = 1;
	public List<PolyMountain> wd = new List<PolyMountain>();
	private ContourPolygon contourPoly = new ContourPolygon();
	private Material m_LineMaterial;
	public Color highlightcolor = Color.cyan;

	private bool contains_visibility_graph = false;

	public VisibilityGraph visibility_graph;

	// Use this for initialization
	void Start () {
		SpriteRenderer _sprite = buttons[_current_prefab].GetComponentInChildren<SpriteRenderer>();
		_sprite.color = highlightcolor;
	}

	public void SetCurrentPrefab(int next_prefab){
		SpriteRenderer _sprite = buttons[_current_prefab].GetComponentInChildren<SpriteRenderer>();
		_sprite.color = Color.white;

		_current_prefab = next_prefab;
		_sprite = buttons[_current_prefab].GetComponentInChildren<SpriteRenderer>();
		_sprite.color = highlightcolor;

	}

	public void SearchPath(){
		Debug.Log("Start Searching.");
		Debug.Log("The countour polygon "+ contourPoly);
		Debug.Log("The countour polygon Contours "+ contourPoly.Contours);
		// Debug.Log("The types "+ typeof(contourPoly));// + typeof(contourPoly.Contours));
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		village = GameObject.FindGameObjectsWithTag("Finish")[0];

		MoveArmy armyComponent = army.GetComponent<MoveArmy>();

		Debug.Log("The control point " + armyComponent.transform.position);
		Debug.Log("The village point " + village.transform.position);
		LinkedList<Polygon2D> polygons_linked_list = new LinkedList<Polygon2D>();

		foreach(Contour contour in contourPoly.Contours )
		{
			Debug.Log("contour " +contour.Vertices);
			Polygon2D new_polygon = new Polygon2D();
			foreach(Vector2D vertex in contour.Vertices )
			{
				new_polygon.AddVertex(new Vector2((float) vertex.x, (float) vertex.y));
			}
			polygons_linked_list.AddLast(new_polygon);
		}
		Debug.Log("Test " + contourPoly.Contours[0].Vertices);

		Vector2 control_point = new Vector2(armyComponent.transform.position[0], armyComponent.transform.position[1]);
		Vector2 village_point = new Vector2(village.transform.position[0], village.transform.position[1]);
		visibility_graph = new VisibilityGraph(polygons_linked_list, control_point , village_point);

		contains_visibility_graph = true;
	}
	
	// Update is called once per frame
	void Update () {
		if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Input.mousePosition;
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

	private void AddNewPathVertex(Vector3 worldPosition)
    {
		Vertex vertex = new Vertex(worldPosition.x, worldPosition.y);
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();
		armyComponent.path.Add(vertex);
    }

	private void CreateMountain(Vector3 worldPosition)
    {
		// Debug.Log("We are clicking somewhere" + worldPosition);

		if ((worldPosition[0] > -15) && (worldPosition[0] < 20))
		{
			// Instantiate the mountain prefab
			

			Contour c = new Contour();
			// c.AddVertex(new Vector2D(-2 + worldPosition[0], worldPosition[1]));
			// c.AddVertex(new Vector2D(-2 + worldPosition[0], 2 + worldPosition[1]));
			// c.AddVertex(new Vector2D(2 + worldPosition[0], 2 + worldPosition[1]));
			// c.AddVertex(new Vector2D(2 + worldPosition[0], worldPosition[1]));
			// Merge the minkowski sum of the new contour with the remaining contours
			// MergeContours(new ContourPolygon(new List<Contour>{ MinkowskiSum(c) }));

			GameObject _mountain = Instantiate(MountainPrefabs[this._current_prefab]) as GameObject;
			_mountain.transform.position = new Vector3(worldPosition[0], worldPosition[1], worldPosition[2]);
			MountainBehaviour mountain_script = _mountain.GetComponent<MountainBehaviour>();
			mountain_script.SetPolygon(worldPosition);
			// Debug.Log("This is the mountain prefab " + _mountain);
			// Debug.Log("This is the mountain_script " + mountain_script.myPolygon.Vertices);

			// Vector3 pscale = Vector3.one;
			foreach (Vector2 v in mountain_script.myPolygon.Vertices){
				Debug.Log("Hallo from Create mountain"+ v);
				c.AddVertex(new Vector2D(v.x, v.y));
			}
			MergeContours(new ContourPolygon(new List<Contour>{ MinkowskiSum(c) }));
			// _mountain.transform.localScale = pscale;
			// GameObject _mountain_script = _mountain.GetComponent<Mountain>() as GameObject;
			// _mountain.UpdatePolygon(new Vector2(worldPosition[0], worldPosition[1]));
			SearchPath();
		}
	}
	private void MergeContours(ContourPolygon newContour)
    {
		Debug.Log("Hello from Merge Contours");
		if (contourPoly.Contours.Count != 0)
		{
			var martinez = new Martinez(contourPoly, newContour, Martinez.OperationType.Union);
			contourPoly = martinez.Run();
		} else {
			contourPoly = newContour;
		}
	}

	private Contour MinkowskiSum(Contour oldContour)
    {
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();
		List<Vector2> sumVertices = new List<Vector2>();
		foreach (Vector2 v in armyComponent.myPolygon.Vertices)
        {
			// Debug.Log("Hallo from Mikowski sum, this is an army component: "+ v);
			foreach (Vector2D v2 in oldContour.Vertices)
            {
				// Debug.Log("Hallo from Mikowski sum, this is an contour component: "+ v2);
				sumVertices.Add(new Vector2(v.x + (float)v2.x, v.y + (float)v2.y));
            }
        }

		// Mountain and army are both convex, so the convex hull algorithm should not change
		// the shape of the resulting Minkowski sum but only remove the vertices inside the polygon
		ContourPolygon result = Util.Algorithms.Polygon.ConvexHull.ComputeConvexHull(sumVertices).ToContourPolygon();
		Assert.IsTrue(result.Contours.Count == 1, "The minkowski sum returned more than one polygon!");
		return result.Contours.First();
	}

	private void OnRenderObject()
	{
		// Apply the line material
		m_LineMaterial.SetPass(0);
		foreach (Contour c in contourPoly.Contours)
        {
			DrawContour(c);
        }

		if (contains_visibility_graph){
			DrawVisibilityGraph();
		}
	}

	private void DrawContour(Contour c)
    {
		/*
		Assert.IsTrue(c.VertexCount > 1);

		GL.Begin(GL.LINE_STRIP);
		float t = Mathf.Sin(Time.time)*Mathf.Sin(Time.time);
		GL.Color(new Color(
				Mathf.Lerp(1f, 0.6f, t),
				Mathf.Lerp(0.5f, 0.7f, t),
				Mathf.Lerp(0.3f, 1.0f, t)
		 ));
		//GL.Color(Color.red);
		foreach (Vector2D v in c.Vertices)
		{
			GL.Vertex(new Vector3((float)v.x, (float)v.y, 0));
		}
		var last = c.Vertices.First();
		GL.Vertex(new Vector3((float)last.x, (float)last.y, 0));
		GL.End();
		*/
	}

	private void DrawVisibilityGraph(){
		GL.Begin(GL.LINE_STRIP);
		float t = Mathf.Sin(Time.time)*Mathf.Sin(Time.time);
		
		GL.Color(Color.red);
		foreach (Edge e in visibility_graph.g.Edges)
		{
			GL.Vertex(new Vector3(e.Start.Pos[0], e.Start.Pos[1], 0));
			GL.Vertex(new Vector3(e.End.Pos[0], e.End.Pos[1], 0));
		}
		var last = visibility_graph.m_vertices.First();
		GL.Vertex(new Vector3((float)last.Pos[0], (float)last.Pos[1], 0));
		GL.End();

	}
}
