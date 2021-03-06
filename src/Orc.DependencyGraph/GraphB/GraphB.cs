// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GraphB.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.DependencyGraph.GraphB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GraphB<T> : Orc.Sort.TopologicalSort.TopologicalSort<T>, IGraph<T>
        where T : IEquatable<T>
    {
        #region Fields
        protected IList<INode<T>> _graphList;
        protected IList<INode<T>> _graphSort;

        protected List<int> _levelList;
        #endregion

        #region Constructors
        public GraphB()
            : this(true, false, 0)
        {
        }

        public GraphB(bool usesPriority, bool usesTracking, int capacity)
            : base(usesPriority, usesTracking)
        {
            _graphList = new List<INode<T>>(capacity);
            _graphSort = new List<INode<T>>(capacity);
            _levelList = new List<int>(capacity);
        }
        #endregion

        #region Properties
        public int CountNodes
        {
            get { return _graphList.Count; }
        }

        public int CountLevels
        {
            get { return _levelList.Max() + 1; }
        }

        public IEnumerable<INode<T>> Nodes { get; private set; }
        #endregion

        #region IGraph<T> Members
        public bool CanSort()
        {
            try
            {
                Sort();
            }
            catch (TopologicalSortException)
            {
                return false;
            }

            return true;
        }

        public void AddSequence(IEnumerable<T> sequence)
        {
            var sequence_count = sequence.Count();

            if (sequence_count == 0)
            {
                throw new ArgumentException("Adding failed because sequence cannot be empty.");
            }

            base.Add(sequence);

            int node_level = -1;

            foreach (var node in sequence)
            {
                int key = NodeKey(node);

                if (_graphList.Count <= key)
                {
                    _graphList.Add(new Node<T>(this, key));
                    _levelList.Add(node_level + 1);
                }

                node_level = _levelList[key];
            }

            if (sequence_count == 1)
            {
                return;
            }

            int key_next = 0;
            int key_prev = NodeKey(sequence.First());

            foreach (var node in sequence.Skip(1))
            {
                key_next = NodeKey(node);

                int lvl_diff = _levelList[key_prev] - _levelList[key_next] + 1;
                int lvl_root = 0;

                if (lvl_diff > 0)
                {
                    _levelList[key_prev] -= lvl_diff;
                    lvl_root = Math.Min(lvl_root, _levelList[key_prev]);

                    foreach (int key_prec in GetPrecedents(key_prev, false, false))
                    {
                        _levelList[key_prec] -= lvl_diff;
                        lvl_root = Math.Min(lvl_root, _levelList[key_prec]);
                    }
                }

                if (lvl_root < 0)
                {
                    for (int key = 0; key < _levelList.Count; key++)
                    {
                        _levelList[key] -= lvl_root;
                    }
                }

                key_prev = key_next;
            }

            _levelList[key_next] = GetPrecedents(key_next, true, false).Max(i => _levelList[i]) + 1;
        }

        public void AddSequences(IEnumerable<IEnumerable<T>> sequences)
        {
            foreach (var sequence in sequences)
            {
                AddSequence(sequence);
            }
        }

        public INode<T> Find(T node)
        {
            int key;

            if (!nodesDict.TryGetValue(node, out key))
            {
                return null;
            }

            return _graphList[key];
        }

        public IOrderedEnumerable<INode<T>> GetNodes(int level)
        {
            return new OrderedEnumerable<INode<T>>(() => _graphList.Where(n => n.Level == level));
        }

        public IOrderedEnumerable<INode<T>> GetNodesBetween(int levelFrom, int levelTo)
        {
            return new OrderedEnumerable<INode<T>>(() => _graphList.Where(n => levelFrom <= n.Level && n.Level <= levelTo).OrderBy(n => n.Level));
        }

        public new IOrderedEnumerable<INode<T>> Sort()
        {
            base.Sort();

            if (nodesSort == null)
            {
                //  return null;
                throw new TopologicalSortException("Topological sort failed due to loops in the graph");
            }

            if (nodesSort.Count != _graphSort.Count)
            {
                _graphSort = nodesSort.Select(Find).ToList();
            }

            return new OrderedEnumerable<INode<T>>(() => _graphSort);
        }

        public IEnumerable<INode<T>> GetRootNodes()
        {
            return GetNodes(0);
        }

        public IEnumerable<INode<T>> GetLeafNodes()
        {
            return GetNodes(_levelList.Max());
        }
        #endregion

        #region Methods
        public IEnumerable<INode<T>> GetNodesRelatedTo(T node)
        {
            int key;
            if (!nodesDict.TryGetValue(node, out key))
            {
                throw new ArgumentException("node note present in graph");
            }

            var set = new SortedSet<int>();
            set.UnionWith(GetPrecedents(key, false, false));
            set.UnionWith(GetDependents(key, false, false));

            return set.Select(k => _graphList[k]);
        }

        public IEnumerable<INode<T>> GetNodesRelatedTo(T node, int levelFrom, int levelTo)
        {
            return GetNodesRelatedTo(node).Where(n => levelFrom <= n.Level && n.Level <= levelTo);
        }
        #endregion

        #region Nested type: Node
        /*
        protected override int NodeKey(T node)
        {
            int key = base.NodeKey(node);

            if (graphList.Count <= key)
            {
                graphList.Add(new Node<T>(this, key));
                levelList.Add(-1);
            }

            return key;
        }
        */
        public class Node<N> : INode<N>
            where N : IEquatable<N>
        {
            #region Fields
            private readonly int key;
            #endregion

            #region Constructors
            public Node(GraphB<N> graph, int index)
            {
                Graph = graph;
                key = index;
            }
            #endregion

            #region Properties
            public N Value
            {
                get { return Graph.nodesList[key]; }
            }

            public GraphB<N> Graph { get; private set; }

            public int Level
            {
                get { return Graph._levelList[key]; }
            }

            // relativeLevel < 0
            public IOrderedEnumerable<INode<N>> Precedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(key, false, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel > 0
            public IOrderedEnumerable<INode<N>> Descendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(key, false, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // parents
            public IOrderedEnumerable<INode<N>> ImmediatePrecedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(key, true, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // children
            public IOrderedEnumerable<INode<N>> ImmediateDescendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(key, true, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel == 0
            public IOrderedEnumerable<INode<N>> TerminatingPrecedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(key, false, true).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel == Graph.CountLevel-1
            public IOrderedEnumerable<INode<N>> TerminatingDescendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(key, false, true).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            public INode<N> Next
            {
                get
                {
                    if (key + 1 >= Graph._graphList.Count)
                        return null;
                    else
                        return Graph._graphList[key + 1];
                }
            }

            public INode<N> Previous
            {
                get
                {
                    if (key - 1 < 0)
                    {
                        return null;
                    }

                    return Graph._graphList[key - 1];
                }
            }
            #endregion

            #region INode<N> Members
            public IOrderedEnumerable<INode<N>> GetNeighbours(int relativeLevelFrom, int relativeLevelTo)
            {
                var levelFrom = Level + relativeLevelFrom;
                var levelTo = Level + relativeLevelTo;

                return new OrderedEnumerable<INode<N>>(() => Graph.GetNodesRelatedTo(Value, levelFrom, levelTo).OrderBy(n => n.Level));
            }
            #endregion

            #region Methods
            public override string ToString()
            {
                return $"Node({Value},{Level},{ImmediatePrecedents.Count()},{ImmediateDescendants.Count()})";
            }
            #endregion
        }
        #endregion
    }
}