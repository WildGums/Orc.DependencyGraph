namespace Orc.DependencyGraph.GraphB
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class GraphB<T> : Orc.Sort.TopologicalSort.TopologicalSort<T>, IGraph<T>
        where T : IEquatable<T>
    {
        protected IList<INode<T>> _graphList;
        protected IList<INode<T>> _graphSort;

        protected List<int> _levelList;

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

            Nodes = Array.Empty<INode<T>>();
        }

        public int CountNodes
        {
            get { return _graphList.Count; }
        }

        public int CountLevels
        {
            get { return _levelList.Max() + 1; }
        }

        public IEnumerable<INode<T>> Nodes { get; private set; }

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
            ArgumentNullException.ThrowIfNull(sequence);

            var sequence_count = sequence.Count();
            if (sequence_count == 0)
            {
                throw new ArgumentException("Adding failed because sequence cannot be empty.");
            }

            base.Add(sequence);

            var node_level = -1;

            foreach (var node in sequence)
            {
                var key = NodeKey(node);

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

            var key_next = 0;
            var key_prev = NodeKey(sequence.First());

            foreach (var node in sequence.Skip(1))
            {
                key_next = NodeKey(node);

                var lvl_diff = _levelList[key_prev] - _levelList[key_next] + 1;
                var lvl_root = 0;

                if (lvl_diff > 0)
                {
                    _levelList[key_prev] -= lvl_diff;
                    lvl_root = Math.Min(lvl_root, _levelList[key_prev]);

                    foreach (var key_prec in GetPrecedents(key_prev, false, false))
                    {
                        _levelList[key_prec] -= lvl_diff;
                        lvl_root = Math.Min(lvl_root, _levelList[key_prec]);
                    }
                }

                if (lvl_root < 0)
                {
                    for (var key = 0; key < _levelList.Count; key++)
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
            ArgumentNullException.ThrowIfNull(sequences);

            foreach (var sequence in sequences)
            {
                AddSequence(sequence);
            }
        }

        public INode<T>? Find(T node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (!nodesDict.TryGetValue(node, out var key))
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

            if (nodesSort is null)
            {
                //  return null;
                throw new TopologicalSortException("Topological sort failed due to loops in the graph");
            }

            if (nodesSort.Count != _graphSort.Count)
            {
                _graphSort = nodesSort.Select(Find)
                    .Where(node => node is not null)
                    .ToList()!;
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

        public IEnumerable<INode<T>> GetNodesRelatedTo(T node)
        {
            ArgumentNullException.ThrowIfNull(node);

            if (!nodesDict.TryGetValue(node, out var key))
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
            ArgumentNullException.ThrowIfNull(node);

            return GetNodesRelatedTo(node).Where(n => levelFrom <= n.Level && n.Level <= levelTo);
        }

        public class Node<N> : INode<N>
            where N : IEquatable<N>
        {
            private readonly int _key;

            public Node(GraphB<N> graph, int index)
            {
                ArgumentNullException.ThrowIfNull(graph);

                Graph = graph;
                _key = index;
            }

            public N Value
            {
                get { return Graph.nodesList[_key]; }
            }

            public GraphB<N> Graph { get; private set; }

            public int Level
            {
                get { return Graph._levelList[_key]; }
            }

            // relativeLevel < 0
            public IOrderedEnumerable<INode<N>> Precedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(_key, false, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel > 0
            public IOrderedEnumerable<INode<N>> Descendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(_key, false, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // parents
            public IOrderedEnumerable<INode<N>> ImmediatePrecedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(_key, true, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // children
            public IOrderedEnumerable<INode<N>> ImmediateDescendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(_key, true, false).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel == 0
            public IOrderedEnumerable<INode<N>> TerminatingPrecedents
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetPrecedents(_key, false, true).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            // relativeLevel == Graph.CountLevel-1
            public IOrderedEnumerable<INode<N>> TerminatingDescendants
            {
                get { return new OrderedEnumerable<INode<N>>(() => Graph.GetDependents(_key, false, true).OrderBy(i => Graph._levelList[i]).Select(i => Graph._graphList[i])); }
            }

            public INode<N>? Next
            {
                get
                {
                    if (_key + 1 >= Graph._graphList.Count)
                    {
                        return null;
                    }
                    else
                    {
                        return Graph._graphList[_key + 1];
                    }
                }
            }

            public INode<N>? Previous
            {
                get
                {
                    if (_key - 1 < 0)
                    {
                        return null;
                    }

                    return Graph._graphList[_key - 1];
                }
            }

            public IOrderedEnumerable<INode<N>> GetNeighbours(int relativeLevelFrom, int relativeLevelTo)
            {
                var levelFrom = Level + relativeLevelFrom;
                var levelTo = Level + relativeLevelTo;

                return new OrderedEnumerable<INode<N>>(() => Graph.GetNodesRelatedTo(Value, levelFrom, levelTo).OrderBy(n => n.Level));
            }

            public override string ToString()
            {
                return $"Node({Value},{Level},{ImmediatePrecedents.Count()},{ImmediateDescendants.Count()})";
            }
        }
    }
}
