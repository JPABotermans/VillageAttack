﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Util.Geometry.Graph;
using Util.Geometry.Polygon;
using Util.Geometry.Contour;
using UnityEngine.Assertions;
using Util.Geometry;
using System.Linq;
using Util.Algorithms.Polygon;
using Util.VisibilityGraph;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.IO;
using System.Timers;
using System.Diagnostics;
using System.Threading;
using System;
using Debug = UnityEngine.Debug;


public class SceneController : MonoBehaviour {
	[SerializeField] public GameObject[] MountainPrefabs;
	[SerializeField] public GameObject[] buttons;

	[SerializeField] public GameObject village;
	public GameObject army = null;
	[SerializeField] public SpriteRenderer[] mountainSprites;
	[SerializeField] private TextMesh TimeText;
	
	private GameObject _mountain;
	private int _current_prefab = 1;
	public List<PolyMountain> wd = new List<PolyMountain>();
	private ContourPolygon contourPoly = new ContourPolygon();
	private ContourPolygon preMinkowskiPoly = new ContourPolygon();
	private LinkedList<Polygon2D> polygons_linked_list = new LinkedList<Polygon2D>();
	private Material m_LineMaterial;
	private bool drawPreMinkowskiContour = true;
	private bool drawContour = false;
	private bool drawPolygon = false;
	private bool drawVisibilityGraph = false;

	public Color highlightcolor = Color.cyan;

	private bool contains_visibility_graph = false;

	public VisibilityGraph visibility_graph;

	private int startTime = 30;
	private float secondsCounted;
	
	private bool _army_moving = false;

	void Start () {
		secondsCounted = 30;
		Debug.Log("The sprite" + mountainSprites[_current_prefab]);
		mountainSprites[_current_prefab].color = highlightcolor;
		StartCoroutine(UpdateTime());
	}

	public void SetCurrentPrefab(int next_prefab){

		mountainSprites[_current_prefab].color = Color.white;

		_current_prefab = next_prefab;
		mountainSprites[_current_prefab].color = highlightcolor;

	}

	public void SearchPath(){
		_army_moving = true;
		army = GameObject.FindGameObjectsWithTag("Player")[0];
		village = GameObject.FindGameObjectsWithTag("Finish")[0];

		MoveArmy armyComponent = army.GetComponent<MoveArmy>();

		polygons_linked_list = new LinkedList<Polygon2D>();

		foreach(Contour contour in contourPoly.Contours )
		{
			Polygon2D new_polygon = new Polygon2D();
			foreach(Vector2D vertex in contour.Vertices )
			{
				new_polygon.AddVertexFirst(new Vector2((float)vertex.x, (float)vertex.y));
			}
			polygons_linked_list.AddLast(new_polygon);
		}

		// Remove duplicate vertices
		foreach (Polygon2D c in polygons_linked_list)
		{
			List<Vector2> toRemove = new List<Vector2>();
			for (int i = 0; i < c.VertexCount; i++)
            {
				var v = c.Vertices.ElementAt(i);
				for (int j = i+1; j < c.VertexCount; j ++)
                {
					var v2 = c.Vertices.ElementAt(j);
					if (i != j && v.x == v2.x && v.y == v2.y)
					{
						toRemove.Add(v2);
					}
				}
            }
			foreach (Vector2 i in toRemove)
            {
				c.RemoveVertex(i);
            }
			Debug.Log("Number of vertices of this polygon: " + c.Vertices.Count);
			Debug.Log("Clockwise:" + c.IsClockwise() + ", convex:" + c.IsConvex() + ", simple:" + c.IsSimple());
		}
		// Remove all contained polygons
		RemoveContainedPolygons(ref polygons_linked_list);
		Debug.Log("------------ # of polygons passed to VisibilityGraphAlgo: " + polygons_linked_list.Count);

		Vector2 control_point = new Vector2(armyComponent.transform.position[0], armyComponent.transform.position[1]);
		Vector2 village_point = new Vector2(village.transform.position[0], village.transform.position[1]);
		visibility_graph = new VisibilityGraph(polygons_linked_list, control_point , village_point);

		Stopwatch stopWatch = new Stopwatch();
        stopWatch.Start();
		NewPath(BFS(visibility_graph.g, visibility_graph.control_vertex, visibility_graph.village_vertex));
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        TimeSpan ts = stopWatch.Elapsed;
		Debug.Log("BFS took " + ts);
		stopWatch.Start();
		NewPath(DijkstrasAlgorithm(visibility_graph.g, visibility_graph.control_vertex, visibility_graph.village_vertex));
        stopWatch.Stop();
        // Get the elapsed time as a TimeSpan value.
        ts = stopWatch.Elapsed;
		Debug.Log("Dijkstas algortihm took " + ts);

		

		contains_visibility_graph = true;
	}
	
	// Update is called once per frame
	void Update () {
		StartCoroutine(UpdateTime());

		if (Input.GetKeyUp(KeyCode.Z))
		{
			drawContour = !drawContour;
		} else if (Input.GetKeyUp(KeyCode.X)) {
			drawPolygon = !drawPolygon;
		} else if (Input.GetKeyUp(KeyCode.C)) {
			drawVisibilityGraph = !drawVisibilityGraph;
		}

		if (Input.GetMouseButtonDown(0))
		{
			Vector3 pos = Input.mousePosition;
			Vector3 pos_world = Camera.main.ScreenToWorldPoint(
						new Vector3(Input.mousePosition.x,
						Input.mousePosition.y,
						Camera.main.nearClipPlane)
						);
			CreateMountain(pos_world);
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

	private void NewPath(List<Vertex> path)
    {
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();
		armyComponent.path = path;
    }

	private void CreateMountain(Vector3 worldPosition)
    {
		// Mountain is placed only if it is nog too close to the village & army
		// and if it is in the map bounds
		Vector2 mouseWorldPos = new Vector2(worldPosition.x, worldPosition.y);
		bool tooCloseToVillage = village != null && Vector2.Distance(village.transform.position, mouseWorldPos) <= 4f;
		bool tooCloseToArmy = army != null && Vector2.Distance(army.transform.position, mouseWorldPos) <= 4f;
		if ((worldPosition[0] > -15) && (worldPosition[0] < 20) && village != null && !tooCloseToArmy && !tooCloseToVillage)
		{
			// Instantiate the mountain prefab
			Contour c = new Contour();
			GameObject _mountain = Instantiate(MountainPrefabs[this._current_prefab]) as GameObject;
			_mountain.transform.position = new Vector3(worldPosition[0], worldPosition[1], worldPosition[2]);
			MountainBehaviour mountain_script = _mountain.GetComponent<MountainBehaviour>();
			mountain_script.SetPolygon(worldPosition);

			foreach (Vector2 v in mountain_script.myPolygon.Vertices){
				c.AddVertex(new Vector2D(v.x, v.y));
			}
			preMinkowskiPoly = MergeContours(new ContourPolygon(new List<Contour> { c }), preMinkowskiPoly);
			contourPoly = MergeContours(new ContourPolygon(new List<Contour>{ MinkowskiSum(c) }), contourPoly);

			// SearchPath();
		}
	}

	private List<Vertex> BFS(AdjacencyListGraph g, Vertex root, Vertex goal)
    {
		Queue<List<Vertex>> queue = new Queue<List<Vertex>>();
		List<Vertex> path = new List<Vertex>();
		path.Add(root);
		queue.Enqueue(path);

		while (queue.Count != 0)
        {
			List<Vertex> p = queue.Dequeue();
			Vertex v = p.Last();
			if (object.ReferenceEquals(v, goal))
            {
				Debug.Log("Found GOAL VERTEX!");
				return p;
            }
			foreach (Edge e in g.EdgesOf(v))
            {
				List<Vertex> newP = new List<Vertex>();
				newP.AddRange(p);
				Vertex v2 = object.ReferenceEquals(v, e.Start) ? e.End : e.Start;
				newP.Add(v2);
				queue.Enqueue(newP);
            }
        }

		Debug.Log("Could not find any path to the village.");
		return path;
    }

	private Vertex MinValue(Dictionary<Vertex, float> distances ){
		float min_distance = 10000.0f;
		Vertex best_vertex = distances.First().Key;
		foreach(KeyValuePair<Vertex, float> entry in distances){
			if (min_distance > entry.Value)
			{
				best_vertex = entry.Key;
				min_distance = entry.Value;
			}
		}
		return best_vertex;
	}

	
	private List<Vertex> ReversePath(Dictionary<Vertex, Vertex> previous_dict, Vertex end_vertex){
		List<Vertex> path = new List<Vertex>();
		path.Add(end_vertex);

		while (previous_dict.ContainsKey(end_vertex)){
			end_vertex = previous_dict[end_vertex];
			path.Add(end_vertex);
		}
		path.Add(end_vertex);
		path.Reverse();
		return path;
	}
	
	private List<Vertex> DijkstrasAlgorithm(AdjacencyListGraph g, Vertex root, Vertex goal)
    {
		//Use dictonary as priority queue (yes hacky)...
		Dictionary<Vertex, float> queue = new Dictionary<Vertex, float>(); 
		Dictionary<Vertex, Vertex> previous = new Dictionary<Vertex, Vertex>();

		// Use list for mainting all handled vertices (yes, even more hacky)
		List<Vertex> handled_vertices = new List<Vertex>();
		
		queue.Remove(root);
		queue.Add(root, 0.0f);

		while (queue.Count != 0)
        {
			Vertex _vertex = MinValue(queue);

			if (object.ReferenceEquals(_vertex, goal))
            {
				Debug.Log("Found GOAL VERTEX!");
				return ReversePath(previous, _vertex);
            }
			foreach (Edge e in g.EdgesOf(_vertex))
            {

				Vertex v2 = object.ReferenceEquals(_vertex, e.Start) ? e.End : e.Start;
				if (!handled_vertices.Contains(v2)){
					float temp_distance = queue[_vertex] + (new Vector2( _vertex.Pos.x, _vertex.Pos.y) - new Vector2(v2.Pos.x, v2.Pos.y)).magnitude;
					if (!queue.ContainsKey(v2)){
						queue.Add(v2, 1000.0f);
					}
					if (temp_distance < queue[v2]) {
						queue.Remove(v2);
						queue.Add(v2, temp_distance);
						if (previous.ContainsKey(v2)){
							previous.Remove(v2);
						}
						previous.Add(v2, _vertex);
					}
				}
			}
			queue.Remove(_vertex);
			handled_vertices.Add(_vertex);
        }

		Debug.Log("Could not find any path to the goal, we are using BFS.");
		return BFS(g, root, goal);
    }


	private ContourPolygon MergeContours(ContourPolygon newContour, ContourPolygon oldContour)
    {
		if (contourPoly.Contours.Count != 0)
		{
			var martinez = new Martinez(oldContour, newContour, Martinez.OperationType.Union);
			return martinez.Run();
			foreach (Contour c in oldContour.Contours) {
				foreach(Vector2D v in c.Vertices)
                {
					foreach(Vector2D v2 in c.Vertices)
                    {
						if (v != v2 && v.x == v2.x && v.y == v2.y)
                        {
							Debug.Log("Found a duplicate");
							c.Vertices.Remove(v2);
                        }
                    }
                }
            }
		} else {
			return newContour;
		}
	}

	private void RemoveContainedPolygons(ref LinkedList<Polygon2D> polygons)
    {
		List<Polygon2D> toRemove = new List<Polygon2D>();
		foreach (Polygon2D p in polygons)
		{
			foreach (Polygon2D p2 in polygons)
			{
				if (object.ReferenceEquals(p, p2)) continue;

				bool contained = true;
				foreach (Vector2 v in p.Vertices)
				{
					if (!p2.ContainsInside(v))
                    {
						contained = false;
						break;
                    }
				}
				if (contained)
                {
					toRemove.Add(p);
                }
			}
		}
		foreach (Polygon2D p in toRemove)
        {
			polygons.Remove(p);
        }
	}

	private Contour MinkowskiSum(Contour oldContour)
    {
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();
		List<Vector2> sumVertices = new List<Vector2>();
		foreach (Vector2 v in armyComponent.myPolygon.Vertices)
        {
			foreach (Vector2D v2 in oldContour.Vertices)
            {
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

		if (drawPreMinkowskiContour)
        {
			foreach (Contour c in preMinkowskiPoly.Contours)
			{
				DrawContour(c);
			}
		}

		if (drawContour) {
			foreach (Contour c in contourPoly.Contours)
			{
				DrawContour(c);
			}
		}

		if (drawPolygon)
		{
			Color c = new Color(0, 0, 0);
			foreach (Polygon2D p in polygons_linked_list)
			{
				DrawPolygon(p, c);
				c = new Color(c.r+0.2f, c.g, c.b+0.2f);
			}
		}

		if (contains_visibility_graph && drawVisibilityGraph){
			DrawVisibilityGraph();
		}

		DrawHintPolygon();
	}

	private void DrawContour(Contour c)
    {
		Assert.IsTrue(c.VertexCount > 1);

		GL.Begin(GL.LINE_STRIP);
		float t = Mathf.Sin(Time.time)*Mathf.Sin(Time.time);
		GL.Color(new Color(
				Mathf.Lerp(1f, 0.6f, t),
				Mathf.Lerp(0.5f, 0.7f, t),
				Mathf.Lerp(0.3f, 1.0f, t)
		 ));
		foreach (Vector2D v in c.Vertices)
		{
			GL.Vertex(new Vector3((float)v.x, (float)v.y, 0));
		}
		var last = c.Vertices.First();
		GL.Vertex(new Vector3((float)last.x, (float)last.y, 0));
		GL.End();
	}

	private void DrawHintPolygon()
    {
		Vector3 pos = Input.mousePosition;
		Vector3 pos_world = Camera.main.ScreenToWorldPoint(
					new Vector3(Input.mousePosition.x,
					Input.mousePosition.y,
					Camera.main.nearClipPlane)
					);


		GL.Begin(GL.LINE_STRIP);

		bool tooCloseToVillage = village != null && Vector2.Distance(village.transform.position, pos_world) <= 4f;
		bool tooCloseToArmy = army != null && Vector2.Distance(army.transform.position, pos_world) <= 4f;
		if (tooCloseToArmy || tooCloseToVillage)
		{
			GL.Color(Color.red);
		} else { 
			GL.Color(Color.blue);
		}
		
		MountainBehaviour b = MountainPrefabs[this._current_prefab].GetComponent<MountainBehaviour>();

		// Draw the polygon on the position where mountain will be placed
		// using the scaling of the selected mountain
		Debug.Log(b.scale.ToString());
		GL.Vertex(new Vector3(pos_world.x - 0.5f*b.scale[0], pos_world.y - 0.5f, 0));
		GL.Vertex(new Vector3(pos_world.x, pos_world.y + b.scale[1] - 0.5f, 0));
		GL.Vertex(new Vector3(pos_world.x + b.scale[0], pos_world.y - 0.5f, 0));
		GL.Vertex(new Vector3(pos_world.x - 0.5f * b.scale[0], pos_world.y - 0.5f, 0));
		GL.End();

		// Draw player polygon
		army = GameObject.FindGameObjectsWithTag("Player")[0];
		MoveArmy armyComponent = army.GetComponent<MoveArmy>();

		GL.Begin(GL.LINE_STRIP);
		foreach (Vector2 v in armyComponent.myMesh.Polygon.Vertices)
		{
			GL.Vertex(transform.TransformPoint(new Vector3(v.x + army.transform.position.x, v.y + army.transform.position.y, 0)));
		}
		var last = armyComponent.myPolygon.Vertices.First();
		GL.Vertex(transform.TransformPoint(new Vector3(last.x + army.transform.position.x, last.y + army.transform.position.y, 0)));
		GL.End();
	}

	private void DrawPolygon(Polygon2D p, Color c)
    {
		Assert.IsTrue(p.VertexCount > 1);

		GL.Begin(GL.LINE_STRIP);
		GL.Color(c);
		foreach (Vector2 v in p.Vertices)
		{
			GL.Vertex(new Vector3(v.x, v.y, 0));
		}
		var last = p.Vertices.First();
		GL.Vertex(new Vector3(last.x, last.y, 0));
		GL.End();

		// Draw vertices as crosses
		GL.Begin(GL.LINES);
		GL.Color(Color.blue);
		foreach (Vector2 v in p.Vertices)
		{
			GL.Vertex(new Vector3(v.x, v.y, 0));
			GL.Vertex(new Vector3(v.x+0.1f, v.y+0.1f, 0));
			GL.Vertex(new Vector3(v.x, v.y, 0));
			GL.Vertex(new Vector3(v.x - 0.1f, v.y - 0.1f, 0));
			GL.Vertex(new Vector3(v.x, v.y, 0));
			GL.Vertex(new Vector3(v.x + 0.1f, v.y - 0.1f, 0));
			GL.Vertex(new Vector3(v.x, v.y, 0));
			GL.Vertex(new Vector3(v.x - 0.1f, v.y + 0.1f, 0));
		}
		GL.End();
	}

	private void DrawVisibilityGraph(){
		GL.Begin(GL.LINES);
		GL.Color(Color.red);
        var g = visibility_graph.g;
		foreach (Edge e in visibility_graph.g.Edges)
		{
			GL.Vertex(new Vector3(e.Start.Pos[0], e.Start.Pos[1], 0));
			GL.Vertex(new Vector3(e.End.Pos[0], e.End.Pos[1], 0));
		}
		GL.End();
	}
	private IEnumerator UpdateTime(){
		if (_army_moving){
		secondsCounted -= Time.deltaTime;
		TimeText.text = "Seconds left " +  (int) secondsCounted;
		if (secondsCounted< 0){
			SceneManager.LoadScene("Won");
		}
		GameObject army = GameObject.FindGameObjectsWithTag("Player")[0];
		
		
		
		var distance =  (army.transform.position - village.transform.position).magnitude;
		if (distance < 0.5) {
			SceneManager.LoadScene("Lost");
		}

		}
		else {
			TimeText.text = "Seconds left: 60 " ;
		}
		yield return new WaitForSeconds(0.5f);
	}
}
