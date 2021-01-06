namespace Util.DataStructures.BST.Tests
{
    using UnityEngine;
    using UnityEditor;
    using NUnit.Framework;
    using System;
    using System.Linq;
    using Util.Geometry;
    using System.Collections;
    using System.Collections.Generic;

    public class LineSegmentAATreeTest
    {
        [Test]
        public void SameQueryPointInsertion()
        {
            LineSegmentAATree tree = new LineSegmentAATree(new Vector2(0,0));
            LineSegment seg1 = new LineSegment(new Vector2(-3, 3), new Vector2(3, 3));
            LineSegment seg2 = new LineSegment(new Vector2(-3, 3), new Vector2(3, 5));
            tree.Insert(seg1, seg1.Point1);
            tree.Insert(seg2, seg2.Point1);

            Assert.IsTrue(tree.FindNodes(new Vector2(-3, 3)).Count == 2);
            Assert.IsTrue(tree.FindNodes(new Vector2(0, 4)).Count == 1);
        }

        [Test]
        public void DeleteTest()
        {
            LineSegmentAATree tree = new LineSegmentAATree(new Vector2(0, 0));
            LineSegment seg1 = new LineSegment(new Vector2(-3, 3), new Vector2(3, 3));
            LineSegment seg2 = new LineSegment(new Vector2(-3, 5), new Vector2(3, 5));
            LineSegment seg3 = new LineSegment(new Vector2(-3, 6), new Vector2(3, 6));
            LineSegment seg4 = new LineSegment(new Vector2(-3, 7), new Vector2(3, 7));
            LineSegment seg5 = new LineSegment(new Vector2(-3, 8), new Vector2(3, 8));
            LineSegment seg6 = new LineSegment(new Vector2(-3, 9), new Vector2(3, 9));
            LineSegment seg7 = new LineSegment(new Vector2(-3, 10), new Vector2(3, 10));
            LineSegment seg8 = new LineSegment(new Vector2(-3, 11), new Vector2(3, 11));
            LineSegment seg9 = new LineSegment(new Vector2(-3, 12), new Vector2(3, 12));
            tree.Insert(seg1, seg1.Point1);
            tree.Insert(seg2, seg2.Point1);
            tree.Insert(seg3, seg3.Point1);
            tree.Insert(seg4, seg4.Point1);
            tree.Insert(seg5, seg5.Point1);
            tree.Insert(seg6, seg6.Point1);
            tree.Insert(seg7, seg7.Point1);
            tree.Insert(seg8, seg8.Point1);
            tree.Insert(seg9, seg9.Point1);

            tree.Delete(seg1, seg1.Point1);
            Assert.IsTrue(tree.Count == 8);
            Assert.IsFalse(tree.Contains(seg1));

            tree.Delete(tree.Root.Data, tree.Root.Data.Point1);
            Assert.IsTrue(tree.Count == 7);

            tree.Delete(seg9, seg9.Point1);
            Assert.IsFalse(tree.Contains(seg9));
            Assert.IsTrue(tree.Count == 6);
        }

        [Test]
        public void AngleTest()
        {
            Vector2 v = new Vector2(0, 0);
            LineSegment s = new LineSegment(v, new Vector2(1, 0));
            float x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(1, -1));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(0, -1));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(-1, -1));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(-1, 0));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(-1, 1));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(0, 1));
            x = s.Line.Angle;
            s = new LineSegment(v, new Vector2(1, 1));
            x = s.Line.Angle;
        }
    }
}