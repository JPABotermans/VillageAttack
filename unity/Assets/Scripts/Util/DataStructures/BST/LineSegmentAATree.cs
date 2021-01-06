﻿namespace Util.DataStructures.BST
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using UnityEngine;
    using Util.Geometry;

    /// <summary>
    /// Implementation of an AA tree, which is a special type of BST.
    /// </summary>
    /// <typeparam name="LineSegment"></typeparam>
    public class LineSegmentAATree {     
        // Sentinel.
        protected Node m_Bottom;
        protected Node m_Root;
        protected Vector2 m_point;

        /// <summary>
        /// Number of nodes of the tree.
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// Root node of the tree
        /// </summary>
        public Node Root { get { return m_Root; } }

        /// <summary>
        /// Stores data of a single node traversal, including parent and child information.
        /// </summary>
        protected internal class TraversalHistory
        {
            public TraversalHistory(Node a_Node, Node a_Parent, ECHILDSIDE a_ChildSide)
            {
                node = a_Node;
                parentNode = a_Parent;
                side = a_ChildSide;
            }

            public Node node;
            public Node parentNode;

            public enum ECHILDSIDE
            {
                LEFT,
                RIGHT,
                ISROOT
            }

            public ECHILDSIDE side;
        }

        protected enum COMPARISON_TYPE
        {
            INSERT,
            DELETE,
            FIND
        }

        public LineSegmentAATree(Vector2 orientation_point)
        {
            m_Bottom = new Node(default(LineSegment), null, null, 0);
            m_Root = m_Bottom;
            Count = 0;
            m_point = orientation_point;
        }

        public bool Contains(LineSegment data)
        {
            List<LineSegment> contents = FindNodes(data.Point1);
            contents.AddRange(FindNodes(data.Point2));
            foreach (LineSegment seg in contents)
            {
                if ((seg.Point1 == data.Point1 && seg.Point2 == data.Point2) || (seg.Point2 == data.Point1 && seg.Point1 == data.Point2))
                {
                    return true;
                }
            }
            return false;
        }

        public bool Insert(LineSegment data, Vector2 query_point)
        {
            /* if (!(data is ValueType) && EqualityComparer<T>.Default.Equals(data, default(T)) )
            {
                return false;
            }
            else */ if (m_Root == m_Bottom)
            {
                // create root node
                m_Root = CreateNode(data);
                return true;
            }
            else
            {
                Geometry.LineSegment query_line = new Geometry.LineSegment(m_point, query_point);
                Node currentNode = m_Root;
                Node parent = null;
                int comparisonResult = 1;   // initial value for loop condition to hold

                // store traversal history in stack
                Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(Count + 1, 2)) + 1);
                nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));

                while (currentNode != m_Bottom)
                {
                    parent = currentNode;
                    comparisonResult = CompareTo(query_line, currentNode.Data, COMPARISON_TYPE.INSERT);
                    TraversalHistory histEntry;

                    // switch between left or right
                    if (comparisonResult <= 0)
                    {
                        currentNode = currentNode.Left;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                        nodeStack.Push(histEntry);
                    }
                    else if (comparisonResult > 0)
                    {
                        currentNode = currentNode.Right;
                        histEntry = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                        nodeStack.Push(histEntry);
                    }
                }

                // check if traversed until bottom
                bool didInsert = false;
                Node bottom = nodeStack.Pop().node;
                if (bottom == m_Bottom) // This node must be m_Bottom.
                {
                    // use last comparison result and parent to insert new node
                    if (comparisonResult < 0)
                    {
                        parent.Left = CreateNode(data);
                        didInsert = true;
                    }
                    else if (comparisonResult > 0)
                    {
                        parent.Right = CreateNode(data);
                        didInsert = true;
                    }
                    else if (comparisonResult == 0)
                    {
                        Vector2 comparison_point;
                        if (parent.Data.Point1.Equals(query_point))
                        {
                            comparison_point = parent.Data.Point2;
                        }
                        else
                        {
                            comparison_point = parent.Data.Point1;
                        }
                        if(new LineSegment(m_point, comparison_point).Intersect(data.Line) != null)
                        {
                            parent.Left = CreateNode(data);
                            didInsert = true;
                        }
                        else
                        {
                            parent.Right = CreateNode(data);
                            didInsert = true;
                        }
                    }
                }

                // pop history stack and change shift tree when necessary
                while (nodeStack.Count != 0)
                {
                    TraversalHistory t = nodeStack.Pop();
                    Node n = t.node;
                    n = Skew(n, t.parentNode, t.side);
                    Split(n, t.parentNode, t.side);
                }

                // return whether a node was inserted
                return didInsert;
            }
        }

        public bool FindMin(out LineSegment out_MinValue)
        {
            var min = FindLeftmostNode();
            if (min != m_Bottom)
            {
                out_MinValue = min.Data;
                return true;
            }
            out_MinValue = default(LineSegment);
            return false;
        }

        public bool FindMax(out LineSegment out_MaxValue)
        {
            var max = FindRightmostNode();
            if (max != m_Bottom)
            {
                out_MaxValue = max.Data;
                return true;
            }
            out_MaxValue = default(LineSegment);
            return false;
        }

        public bool Delete(LineSegment data, Vector2 query_point)
        {
            if (m_Root == m_Bottom  /*|| EqualityComparer<T>.Default.Equals(data, default(T)) */)
            {
                return false;
            }

            LineSegment query_line = new LineSegment(m_point, query_point);
            Node currentNode = m_Root;
            Node parent = null;
            Node deleted = m_Bottom;
            Stack<TraversalHistory> nodeStack = new Stack<TraversalHistory>((int)Math.Ceiling(Math.Log(Count + 1, 2)) + 1);
            nodeStack.Push(new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.ISROOT));
            while (currentNode != m_Bottom)
            {
                parent = currentNode;
                TraversalHistory hist;
                int comparisonResult = CompareTo(query_line, currentNode.Data, COMPARISON_TYPE.DELETE);
                if (comparisonResult < 0)
                {
                    currentNode = currentNode.Left;
                    hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.LEFT);
                } else if ( comparisonResult == 0)
                {
                    deleted = currentNode;
                    currentNode = currentNode.Right;
                    hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                }
                else
                {
                    
                    currentNode = currentNode.Right;
                    hist = new TraversalHistory(currentNode, parent, TraversalHistory.ECHILDSIDE.RIGHT);
                }
                nodeStack.Push(hist);
            }

            bool didDelete = false;
            if (deleted != m_Bottom && CompareTo(query_line, deleted.Data, COMPARISON_TYPE.DELETE) == 0)
            {
                if (nodeStack.Pop().node != m_Bottom) // Pop since the last entry is m_Bottom
                {
                    throw new Exception("First node in traversal history was not Bottom!");
                }
                TraversalHistory lastHist = nodeStack.Pop();
                Node last = lastHist.node; // This is the node that is leftmost of the node that we want to delete.
                if (last.Left != m_Bottom)
                {
                    throw new Exception("Last has a left child that is not Bottom!");
                }
                deleted.Data = last.Data;
                Node copy = last.Right;
                if (copy != m_Bottom)
                {
                    last.Data = copy.Data;
                    last.Left = copy.Left;
                    last.Right = copy.Right;
                    last.Level = copy.Level;
                    // Destroy the node
                    copy.Left = null;
                    copy.Right = null;
                    copy.Data = default(LineSegment);
                }
                else
                {
                    if (lastHist.side == TraversalHistory.ECHILDSIDE.LEFT)
                    {
                        lastHist.parentNode.Left = m_Bottom;
                    }
                    else if (lastHist.side == TraversalHistory.ECHILDSIDE.RIGHT)
                    {
                        lastHist.parentNode.Right = m_Bottom;
                    }
                    else
                    {
                        m_Root = m_Bottom;
                    }
                }
                --Count;
                didDelete = true;
            }

            // pop history stack and change tree when necessary
            while (nodeStack.Count != 0)
            {
                TraversalHistory t = nodeStack.Pop();
                Node n = t.node;
                if (n.Left.Level < n.Level - 1 || n.Right.Level < n.Level - 1)
                {
                    --n.Level;
                    if (n.Right.Level > n.Level)
                    {
                        n.Right.Level = n.Level;
                    }
                    n = Skew(n, t.parentNode, t.side);
                    n.Right = Skew(n.Right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                    n.Right.Right = Skew(n.Right.Right, n.Right, TraversalHistory.ECHILDSIDE.RIGHT);
                    n = Split(n, t.parentNode, t.side);
                    n.Right = Split(n.Right, n, TraversalHistory.ECHILDSIDE.RIGHT);
                }
            }
            return didDelete;
        }




        public void Clear()
        {
            // leave existing nodes for garbage collector
            m_Bottom = new Node(default(LineSegment), null, null, 0);
            m_Root = m_Bottom;
            Count = 0;
        }

        /// <summary>
        /// Finds the leftmost node in the tree.
        /// </summary>
        /// <returns></returns>
        protected Node FindLeftmostNode()
        {
            var min = m_Root;
            while (min.Left != m_Bottom)
            {
                min = min.Left;
            }
            return min;
        }

        /// <summary>
        /// Finds the rightmost node in the tree.
        /// </summary>
        /// <returns></returns>
        protected Node FindRightmostNode()
        {
            var max = m_Root;
            while (max.Right != m_Bottom)
            {
                max = max.Right;
            }
            return max;
        }

        /// <summary>
        /// Verify the level paramater of the tree.
        /// </summary>
        /// <returns></returns>
        public bool VerifyLevels()
        {
            return VerifyLevels(m_Root, m_Root.Level);
        }

        /// <summary>
        /// Find a list of nodes with the given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public List<LineSegment> FindNodes(Vector2 data)
        {
            var nodes = new List<LineSegment>();
            FindNodes(data, m_Root, nodes);
            return nodes;
        }

        /// <summary>
        /// Verify whether the bst property holds for the entire tree.
        /// </summary>
        /// <param name="minValue"></param>
        /// <param name="maxValue"></param>
        /// <returns></returns>
        public bool VerifyBST(LineSegment minValue, LineSegment maxValue)
        {
            return VerifyBST(m_Root, minValue, maxValue, COMPARISON_TYPE.INSERT) &&
            VerifyBST(m_Root, minValue, maxValue, COMPARISON_TYPE.DELETE) &&
            VerifyBST(m_Root, minValue, maxValue, COMPARISON_TYPE.FIND);
        }

        /// <summary>
        /// Verify the order of the tree.
        /// </summary>
        /// <returns></returns>
        public bool VerifyOrder()
        {
            return VerifyOrder(m_Root, COMPARISON_TYPE.INSERT) &&
            VerifyOrder(m_Root, COMPARISON_TYPE.DELETE) &&
            VerifyOrder(m_Root, COMPARISON_TYPE.FIND);
        }

        /// <summary>
        /// Rotates tree to the right whenever levels are mismatched.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parent"></param>
        /// <param name="a_Side"></param>
        /// <returns></returns>
        private Node Skew(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.Left.Level != t.Level) return t;

            // Rotate right.
            Node oldLeft = t.Left;
            Node newLeft = oldLeft.Right;
            t.Left = newLeft;
            oldLeft.Right = t;
            if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
            {
                parent.Left = oldLeft;
            }
            else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
            {
                parent.Right = oldLeft;
            }
            else
            {
                m_Root = oldLeft;
            }
            return oldLeft;
        }

        /// <summary>
        /// Rotates tree to the left whenever levels are mismatched.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parent"></param>
        /// <param name="a_Side"></param>
        /// <returns>the new root node.</returns>
        private Node Split(Node t, Node parent, TraversalHistory.ECHILDSIDE a_Side)
        {
            if (t.Right == m_Bottom || t.Right.Right.Level != t.Level)
            {
                return t;
            }

            // Rotate left.
            Node oldRight = t.Right;
            Node newRight = oldRight.Left;
            t.Right = newRight;
            oldRight.Left = t;
            if (a_Side == TraversalHistory.ECHILDSIDE.LEFT)
            {
                parent.Left = oldRight;
            }
            else if (a_Side == TraversalHistory.ECHILDSIDE.RIGHT)
            {
                parent.Right = oldRight;
            }
            else
            {
                m_Root = oldRight;
            }
            ++oldRight.Level;
            return oldRight;
        }

        /// <summary>
        /// Create new node with pointers to bottom
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        private Node CreateNode(LineSegment data)
        {
            Node n = new Node(data, m_Bottom, m_Bottom, 1);
            ++Count;
            return n;
        }

        /// <summary>
        /// Finds next bigger value compared to given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="out_NextBiggest"></param>
        /// <returns>whether the method was succesful.</returns>
        public bool FindNextBiggest(LineSegment data, out LineSegment out_NextBiggest)
        {
            return FindNextBiggestOrSmallest(data, m_Root, true, out out_NextBiggest);
        }

        /// <summary>
        /// Finds next smaller compared to given data value.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="out_NextSmallest"></param>
        /// <returns>whether the method was succesful.</returns>
        public bool FindNextSmallest(LineSegment data, out LineSegment out_NextSmallest)
        {
            return FindNextBiggestOrSmallest(data, m_Root, false, out out_NextSmallest);
        }

        /// <summary>
        /// Finds next biggest or smallest value, depending on a_Bigger, compared to given data value
        /// from the (sub)tree rooted at node t.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="t"></param>
        /// <param name="a_Bigger"></param>
        /// <param name="out_NextBiggest"></param>
        /// <returns>whether the method was succesful.</returns>
        private bool FindNextBiggestOrSmallest(LineSegment data, Node t, bool a_Bigger, out LineSegment out_NextBiggest)
        {
            if (t == m_Bottom /* || EqualityComparer<T>.Default.Equals(data, default(T)) */)
            {
                out_NextBiggest = default(LineSegment);
                return false;
            }

            var currentNode = t;
            var nextNode = t;
            var lastSwitch = m_Bottom;
            while (nextNode != m_Bottom)
            {
                currentNode = nextNode;

                int comparisonResult = CompareTo(data, currentNode.Data, COMPARISON_TYPE.FIND);
                if (a_Bigger)
                {
                    if (comparisonResult < 0)
                    {
                        nextNode = currentNode.Left;
                        lastSwitch = currentNode;
                    }
                    else
                    {
                        nextNode = currentNode.Right;
                    }
                }
                else
                {
                    if (comparisonResult <= 0)
                    {
                        nextNode = currentNode.Left;
                    }
                    else
                    {
                        nextNode = currentNode.Right;
                        lastSwitch = currentNode;
                    }
                }
            }

            if (currentNode != m_Bottom && CompareTo(data, currentNode.Data, COMPARISON_TYPE.FIND) != 0)
            {
                out_NextBiggest = currentNode.Data;
                return true;
            }
            else if (lastSwitch != m_Bottom && CompareTo(data, currentNode.Data, COMPARISON_TYPE.FIND) == 0)
            {
                out_NextBiggest = lastSwitch.Data;
                return true;
            }

            // no bigger found, return default value
            out_NextBiggest = default(LineSegment);
            return false;
        }

        /// <summary>
        /// Find nodes with given data value in the (sub)tree rooted at t.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="t"></param>
        /// <param name="list"></param>
        private void FindNodes(Vector2 data, Node t, List<LineSegment> list)
        {
            // return if at bottom node
            if (t == m_Bottom) return;
            LineSegment query_line = new LineSegment(m_point, data);
            // recurse in left/right childs or both if equal
            if (IsEqual(query_line, t.Data, COMPARISON_TYPE.FIND))
            {
                list.Add(t.Data);
                FindNodes(data, t.Left, list);
                FindNodes(data, t.Right, list);
            }
            else if (CompareTo(query_line, t.Data, COMPARISON_TYPE.FIND) < 0)
            {
                FindNodes(data, t.Left, list);
            }
            else
            {
                FindNodes(data, t.Right, list);
            }
        }

        /// <summary>
        /// Compute size of the (sub)tree rooted at t.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        public int ComputeSize(Node t)
        {
            if (t == m_Bottom)
            {
                return 0;
            }
            else
            {
                int result = 1;
                result += ComputeSize(t.Left);
                result += ComputeSize(t.Right);
                return result;
            }
        }

        /// <summary>
        /// Verify if the level of the tree rooted at t has level less or equal to parentLevel.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="parentLevel"></param>
        /// <returns></returns>
        private bool VerifyLevels(Node t, int parentLevel)
        {
            if (t == m_Bottom && parentLevel >= 0)
            {
                return true;
            }
            else if (t != m_Bottom && t.Level <= parentLevel)
            {
                return VerifyLevels(t.Left, t.Level - 1) && VerifyLevels(t.Right, t.Level);
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// verify BST property holds for the tree.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="minKey"></param>
        /// <param name="maxKey"></param>
        /// <param name="a_ComparisonType"></param>
        /// <returns></returns>
        private bool VerifyBST(Node t, LineSegment minKey, LineSegment maxKey, COMPARISON_TYPE a_ComparisonType)
        {
            // ignore if at bottom
            if (t == m_Bottom) return true;

            try
            {
                if (CompareTo(minKey, t.Data, a_ComparisonType) > 0 ||
                            CompareTo(maxKey, t.Data, a_ComparisonType) < 0)
                {
                    return false;
                }
                else
                {
                    return VerifyBST(t.Left, minKey, t.Data, a_ComparisonType) &&
                    VerifyBST(t.Right, t.Data, maxKey, a_ComparisonType);
                }
            }
            catch (NotImplementedException)
            {
                // If a particular comparison type is not implemented, just assume that was done
                // for a good reason and return true. (Otherwise the user's application will not work anyway)
                return true;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="t"></param>
        /// <param name="a_ComparisonType"></param>
        /// <returns></returns>
        private bool VerifyOrder(Node t, COMPARISON_TYPE a_ComparisonType)
        {
            // ignore if at bottom
            if (t == m_Bottom) return true;

            try
            {
                if ((t.Left == m_Bottom || CompareTo(t.Data, t.Left.Data, a_ComparisonType) > 0) &&
                            (t.Right == m_Bottom || CompareTo(t.Data, t.Right.Data, a_ComparisonType) <= 0))
                {
                    return VerifyOrder(t.Left, a_ComparisonType) && VerifyOrder(t.Right, a_ComparisonType);
                }
                else
                {
                    return false;
                }
            }
            catch (NotImplementedException)
            {
                // If a particular comparison type is not implemented, just assume that was done
                // for a good reason and return true. (Otherwise the user's application will not work anyway)
                return true;
            }
        }

        /// <summary>
        /// Compare two values to each other.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="a_ComparisonType"></param>
        /// <returns></returns>
        protected virtual int CompareTo(LineSegment a, LineSegment b, COMPARISON_TYPE a_ComparisonType)
        {
            if(a.IsOverlapping(b)){
                return 0;
            }
            Vector2? intersection = a.Intersect(b);
            if(intersection == null)
            {
                return -1;
            }
            else
            {
                if(intersection == a.Point1 || intersection == a.Point2)
                {
                    return 0;
                }
                else
                {
                    return 1;
                }
            }
            
        }

        /// <summary>
        /// Check if two values are equal.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <param name="a_ComparisonType"></param>
        /// <returns></returns>
        protected virtual bool IsEqual(LineSegment a, LineSegment b, COMPARISON_TYPE a_ComparisonType)
        {
            return CompareTo(a, b, a_ComparisonType) == 0;
        }

        /// <summary>
        /// Class for all nodes in the tree.
        /// </summary>
        public class Node
        {
            public Node Left { get; set; }

            public Node Right { get; set; }

            public LineSegment Data { get; set; }

            /// <summary>
            /// Gives the level 
            /// </summary>
            public int Level { get; set; }

            public Node(LineSegment a_Data, Node a_Left, Node a_Right, int a_Level)
            {
                Data = a_Data;
                Left = a_Left ?? this;
                Right = a_Right ?? this;
                Level = a_Level;
            }
        }
    }
}
