namespace Util.VisibilityGraph.Tests
{
    using UnityEngine;
    using UnityEditor;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using Util.Geometry.Polygon;
    using System.Collections;
    using System.Collections.Generic;


    [TestFixture]
    public class VisibilityGraphTest
    {
        private VisibilityGraph visibilityGraph;
        
        private void Construct()
        {
            LinkedList<Polygon2D> polygons = new LinkedList<Polygon2D>();
            for(int i = 1; i < 10; i++)
            {
                LinkedList<Vector2> vertices = new LinkedList<Vector2>();
                vertices.AddLast(new Vector2(i * 5, i * i * 5));
                vertices.AddLast(new Vector2((i * 5) + 1, i * i * 5));
                vertices.AddLast(new Vector2((i * 5) + 1, (i * i * 5) + 1));
                vertices.AddLast(new Vector2(i * 5, (i * i * 5) + 1));
                Polygon2D p = new Polygon2D(vertices);
                polygons.AddLast(p);
            }
            Vector2 origin = new Vector2(0, 0);
            Vector2 target = new Vector2(10, 0);
            visibilityGraph = new VisibilityGraph(polygons, origin, target);
        }

        [Test]
        public void isTested()
        {
            Construct();
        }
    }
}