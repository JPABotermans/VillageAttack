namespace Util.VisibilityGraph
{
    using System.Collections;
    using System;
    using System.Collections.Generic;
    using Util.Geometry;
    using UnityEngine;
    using Util.Geometry.Polygon;
    using Util.Geometry.Graph;
    using Util.DataStructures.Queue;
    using Util.DataStructures.BST;


    
    public class VisibilityGraph
    {
        public LinkedList<Vertex> m_vertices;
        private LinkedList<Polygon2D> m_polygons;
        private LinkedList<Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?>> m_vertex_association_data;
        public AdjacencyListGraph g { get; private set; }

        public Vertex control_vertex;
        public Vertex village_vertex;

        public VisibilityGraph(LinkedList<Polygon2D> polygons, Vector2 control_point, Vector2 village)
        {
            m_polygons = polygons;
            m_vertices = new LinkedList<Vertex>();
            control_vertex = new Vertex(control_point);
            m_vertices.AddLast(control_vertex);
            village_vertex = new Vertex(village);
            m_vertices.AddLast(village_vertex);
            m_vertex_association_data = new LinkedList<Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?>>();
            m_vertex_association_data.AddLast(new Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?>(control_point, null, null, control_vertex, null));
            m_vertex_association_data.AddLast(new Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?>(village, null, null, village_vertex, null));

            foreach (Polygon2D p in polygons)
            {
                foreach (Vector2 v in p.Vertices)
                {
                    Vertex point = new Vertex(v);
                    m_vertices.AddLast(point);
                    m_vertex_association_data.AddLast(new Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?>(v, p.Prev(v), p.Next(v), point, p.IsClockwise()));
                }
            }
            LinkedList<Edge> edges = new LinkedList<Edge>();
            foreach (Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> origin in m_vertex_association_data)
            {
                LinkedList<Vertex> visible_vertices = this.VisibleVertices(origin);
                foreach (Vertex target in visible_vertices)
                {
                    float weight = (origin.Item1 - target.Pos).magnitude;
                    Edge e = new Edge(origin.Item4, target, weight);
                    edges.AddLast(e);
                }
            }
            g = new AdjacencyListGraph(m_vertices, edges);
        }

        public LinkedList<Vertex> VisibleVertices(Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> v)
        {
            LinkedList<Vertex> result = new LinkedList<Vertex>();

            BinaryHeap<Tuple<Vector2, Vector2?, Vector2?, bool?, float>> queue = InitializeEventQueue(v);
            LineSegmentAATree status = InitializeStatus(v);

            while (queue.Count > 0){
                Tuple<Vector2, Vector2?, Vector2?, bool?, float> Event = queue.Pop();
                bool visible = HandleEvent(Event, v, ref status);
                if (visible)
                {
                    result.AddLast(new Vertex(Event.Item1));
                }
            }
            return result;
        }

        public static float GetAngle(Vertex v1, Vertex v2)
        {
            if(v1.Pos.x <= v2.Pos.x && v1.Pos.y <= v2.Pos.y)
            {
                return new Line(v1.Pos, v2.Pos).Angle;
            }
            else if(v1.Pos.x < v2.Pos.x && v1.Pos.y > v2.Pos.y)
            {
                return (2 * (float)Math.PI) + new Line(v1.Pos, v2.Pos).Angle;
            }
            else 
            {
                return (float)Math.PI + new Line(v1.Pos, v2.Pos).Angle;
            }
        }

        public BinaryHeap<Tuple<Vector2, Vector2?, Vector2?, bool?, float>> InitializeEventQueue(Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> v)
        {
            var queue = new BinaryHeap<Tuple<Vector2, Vector2?, Vector2?, bool?, float>>(new TupleComparer(v.Item1));
            foreach (Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> p in m_vertex_association_data)
            {
                if (!(p.Item1 == v.Item1))
                {
                    queue.Push(new Tuple<Vector2, Vector2?, Vector2?, bool?, float>(p.Item1, p.Item2, p.Item3, p.Item5, GetAngle(v.Item4, p.Item4)));
                }
            }
            return queue;
        }

        public LineSegmentAATree InitializeStatus(Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> v)
        {
            LineSegmentAATree status = new LineSegmentAATree(v.Item1);
            LineSegment xAxis = new LineSegment(v.Item1, new Vector2(float.PositiveInfinity, v.Item1.y));
            foreach (Polygon2D p in m_polygons)
            {
                foreach (LineSegment s in p.Segments)
                {
                    Vector2? intersection = xAxis.Intersect(s);
                    if (intersection != null && !s.IsOnSegment(v.Item1))
                    {
                        if(s.IsEndpoint((Vector2)intersection))
                        {
                            Vector2 otherPoint = s.Point1;
                            if(intersection == s.Point1)
                            {
                                otherPoint = s.Point2;
                            }
                            if(otherPoint.y < ((Vector2) intersection).y)
                            {
                                status.Insert(s, (Vector2)intersection);
                            }
                        }
                        else
                        {
                            status.Insert(s, (Vector2)intersection);
                        }
                    }                    
                }
            }
            return status;
        }

        public bool HandleEvent(Tuple<Vector2, Vector2?, Vector2?, bool?, float> Event, Tuple<Vector2, Vector2?, Vector2?, Vertex, bool?> v,ref LineSegmentAATree status)
        {
            bool seg1InStatus;
            bool seg2InStatus;
            bool visible;
            LineSegment VisibleSegment;
            if (Event.Item2 != null && Event.Item2 != null)
            {
                LineSegment seg1 = new LineSegment(Event.Item1, (Vector2)Event.Item2);
                seg1InStatus = status.Contains(seg1);
                LineSegment seg2 = new LineSegment(Event.Item1, (Vector2)Event.Item3);
                seg2InStatus = status.Contains(seg2);
                if (seg1InStatus)
                {
                    status.Delete(seg1, Event.Item1);
                }
                if (seg2InStatus)
                {
                    status.Delete(seg2, Event.Item1);
                }
                status.FindMin(out VisibleSegment);
                bool directsinside = DirectsInside(v.Item1, v.Item2, v.Item3, Event.Item1, v.Item5);
                if (VisibleSegment != null)
                {
                    bool no_intersects = VisibleSegment.Intersect(new LineSegment(v.Item1, Event.Item1)) == null;
                    visible = !directsinside && no_intersects && (!DirectsInside(Event.Item1, Event.Item2, Event.Item3, v.Item1, Event.Item4));
                }
                else
                {
                    visible = (!directsinside) && (!DirectsInside(Event.Item1, Event.Item2, Event.Item3, v.Item1, Event.Item4));
                }
                if (!seg1InStatus && !seg1.IsOnSegment(v.Item1))
                {
                    status.Insert(seg1, Event.Item1);
                }
                if (!seg2InStatus && !seg2.IsOnSegment(v.Item1))
                {
                    status.Insert(seg2, Event.Item1);
                }
                
            }
            else
            {
                status.FindMin(out VisibleSegment);
                if (VisibleSegment != null)
                {
                    visible = !DirectsInside(v.Item1, v.Item2, v.Item3, Event.Item1, v.Item5) && VisibleSegment.Intersect(new LineSegment(v.Item1, Event.Item1)) == null;
                }
                else
                {
                    visible = !DirectsInside(v.Item1, v.Item2, v.Item3, Event.Item1, v.Item5);
                }
            }
            
            return visible;
        }

        public static bool DirectsInside(Vector2 p, Vector2? previous, Vector2? next, Vector2 query, bool? clockwise)
        {
            if (previous == null || next == null || clockwise == null)
            {
                return false;
            }
            else
            {
                LineSegment seg1 = new LineSegment((Vector2)previous, p);
                LineSegment seg2 = new LineSegment(p, (Vector2)next);
                if (ConvexAngle(p, (Vector2) previous, (Vector2) next, (bool)clockwise))
                {
                    if ((bool)clockwise != seg1.IsRightOf(query) || (bool)clockwise != seg2.IsRightOf(query) || seg1.Line.IsOnLine(query) || seg2.Line.IsOnLine(query))
                    {
                        return false;
                    }
                }
                else
                {
                    if (((bool)clockwise != seg1.IsRightOf(query) && (bool)clockwise != seg2.IsRightOf(query)) || seg1.Line.IsOnLine(query) || seg2.Line.IsOnLine(query))
                    {
                        return false;
                    }
                }
                
                return true;
            }
        }

        private static bool ConvexAngle(Vector2 p, Vector2 previous, Vector2 next, bool clockwise)
        {
            LineSegment seg1 = new LineSegment(previous, p);
            LineSegment seg2 = new LineSegment(p, next);
            return seg1.IsRightOf(next) == clockwise;
        }
    }

    public class Tuple<T, U, V, W, X> 
    {

        public T Item1 { get; private set; }
        public U Item2 { get; private set; }
        public V Item3 { get; private set; }
        public W Item4 { get; private set; }
        public X Item5 { get; private set; }

        public Tuple(T item1, U item2, V item3, W item4, X item5)
        {
            Item1 = item1;
            Item2 = item2;
            Item3 = item3;
            Item4 = item4;
            Item5 = item5;
        }

    }

    public class TupleComparer : IComparer<Tuple<Vector2, Vector2?, Vector2?, bool?, float>>
    {
        Vector2 m_point;

        public TupleComparer(Vector2 orientation_point)
        {
            m_point = orientation_point;
        }
        public int Compare(Tuple<Vector2, Vector2?, Vector2?, bool?, float> x, Tuple<Vector2, Vector2?, Vector2?, bool?, float> y)
        {
            if (x.Item5 == y.Item5)
            { 
                if((x.Item1 - m_point).magnitude < (y.Item1 - m_point).magnitude)
                {
                    return -1;
                } else if((x.Item1 - m_point).magnitude > (y.Item1 - m_point).magnitude)
                {
                    return 1;
                }
                else
                {
                    return 0;
                }
                
            }
            if (x.Item5 < y.Item5)
            {
                return -1;
            }
            else
            {
                return 1;
            }
        }
    }
}
