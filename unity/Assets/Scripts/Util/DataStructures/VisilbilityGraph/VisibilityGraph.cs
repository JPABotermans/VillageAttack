namespace Util.VisibilityGraph
{
    using System.Collections;
    using System.Collections.Generic;
    using Util.Geometry;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Geometry.Graph;

    public class VisibilityGraph
    {
        private LinkedList<Vertex> m_vertices;
        private LinkedList<Polygon2D> m_polygons;
        private AdjacencyListGraph g;

        public VisibilityGraph(LinkedList<Polygon2D> polygons, Vector2 control_point, Vector2 village)
        {
            m_polygons = polygons;
            m_vertices = new LinkedList<Vertex>();
            m_vertices.AddLast(new Vertex(control_point));
            m_vertices.AddLast(new Vertex(village));
            foreach (Polygon2D p in polygons)
            {
                foreach (Vector2 v in p.Vertices)
                {
                    Vertex point = new Vertex(v);
                    m_vertices.AddLast(point);
                }
            }
            LinkedList<Edge> edges = new LinkedList<Edge>();
            foreach(Vertex origin in m_vertices)
            {
                LinkedList<Vertex> visible_vertices = this.VisibleVertices(origin);
                foreach(Vertex target in visible_vertices)
                {
                    float weight = (origin.Pos - target.Pos).magnitude;
                    Edge e = new Edge(origin, target, weight);
                    edges.AddLast(e);
                }
            }
            g = new AdjacencyListGraph(m_vertices, edges);
        }

        private LinkedList<Vertex> VisibleVertices(Vertex v)
        {
            LinkedList<Vertex> result = new LinkedList<Vertex>();
            foreach(Vertex p in this.m_vertices) { 
                if(this.IsVisible(v, p))
                {
                    result.AddLast(p);
                }
                
            }
            return result;
        }

        private bool IsVisible(Vertex p, Vertex v)
        {
            if (p.Pos.Equals(v.Pos))
            {
                return false;
            }
            LineSegment seg = new LineSegment(p.Pos, v.Pos);
            foreach(Polygon2D poly in this.m_polygons)
            {
                if(poly.OnBoundary(p.Pos) && poly.OnBoundary(v.Pos) && poly.Contains((v.Pos + p.Pos) / 2))
                {
                    return false;
                }
                foreach(LineSegment intersecting_segment in poly.Segments)
                {
                    Vector2? intersecting_point = seg.Intersect(intersecting_segment);
                    if (intersecting_point != null)
                    {
                        if(!(intersecting_point.Equals(v.Pos) || intersecting_point.Equals(p.Pos)))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }

    }
}
