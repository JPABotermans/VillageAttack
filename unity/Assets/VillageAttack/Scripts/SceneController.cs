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

public class SceneController : MonoBehaviour {
	[SerializeField] public GameObject[] MountainPrefabs;
	private GameObject _mountain;
	private int _current_prefab = 1;
	public List<PolyMountain> mountains = new List<PolyMountain>();
	private ContourPolygon contourPoly = new ContourPolygon();
	private Material m_LineMaterial;

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
		if (worldPosition[0] > -15)
		{
			// Instantiate the mountain prefab
			Contour c = new Contour();
			c.AddVertex(new Vector2D(-3 + worldPosition[0], worldPosition[1]));
			c.AddVertex(new Vector2D(-3 + worldPosition[0], 3 + worldPosition[1]));
			c.AddVertex(new Vector2D(3 + worldPosition[0], 3 + worldPosition[1]));
			c.AddVertex(new Vector2D(3 + worldPosition[0], worldPosition[1]));
			// Merge the minkowski sum of the new contour with the remaining contours
			MergeContours(new ContourPolygon(new List<Contour>{ MinkowskiSum(c) }));

			_mountain = Instantiate(MountainPrefabs[this._current_prefab]) as GameObject;
			_mountain.transform.position = new Vector3(worldPosition[0], worldPosition[1], worldPosition[2]);
		}
	}
	private void MergeContours(ContourPolygon newContour)
    {
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
		foreach (Contour c in contourPoly.Contours)
        {
			DrawContour(c);
        }
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
		//GL.Color(Color.red);
		foreach (Vector2D v in c.Vertices)
		{
			GL.Vertex(new Vector3((float)v.x, (float)v.y, 0));
		}
		var last = c.Vertices.First();
		GL.Vertex(new Vector3((float)last.x, (float)last.y, 0));
		GL.End();
	}
}
