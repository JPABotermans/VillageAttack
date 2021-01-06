namespace Util.VisibilityGraph.Tests
{
    using UnityEngine;
    using UnityEditor;
    using Util.Geometry;
    using Util.Geometry.Graph;
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
        
        private void ConstructDefault(Vector2 origin, Vector2 target)
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
            visibilityGraph = new VisibilityGraph(polygons, origin, target);
        }

        private void ConstructCollinearObstacles(Vector2 origin, Vector2 target)
        {
            LinkedList<Polygon2D> polygons = new LinkedList<Polygon2D>();
            for (int i = 1; i < 10; i++)
            {
                LinkedList<Vector2> vertices = new LinkedList<Vector2>();
                vertices.AddLast(new Vector2(i * 5, (i % 2) * 2));
                vertices.AddLast(new Vector2((i * 5) + 1, (i % 2) * 2));
                vertices.AddLast(new Vector2((i * 5) + 1, 1));
                vertices.AddLast(new Vector2(i * 5,  1));
                Polygon2D p = new Polygon2D(vertices);
                polygons.AddLast(p);
            }
            visibilityGraph = new VisibilityGraph(polygons, origin, target);
        }


        [Test]
        public void StatusInitializationBasic()
        {
            Vector2 origin = new Vector2(0, 0);
            Vector2 target = new Vector2(12, (float)0.5);
            ConstructDefault(origin, target);
            var g = visibilityGraph.g;
            Assert.IsTrue(g.DegreeOf(new Vertex(origin)) == 28);
            Assert.IsTrue(g.DegreeOf(new Vertex(new Vector2(5, 5))) == 4);
        }

        [Test]
        public void StatusInitializationCollinear()
        {
            Vector2 origin = new Vector2(4, (float)1.5);
            Vector2 target = new Vector2(12, (float)0.5);
            ConstructCollinearObstacles(origin, target);
            var g = visibilityGraph.g;
            Assert.IsTrue(g.DegreeOf(new Vertex(origin)) == 2);

        }

        [Test]
        public void CollinearObstacles()
        {

        }

    }
}