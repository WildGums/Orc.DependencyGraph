namespace Orc.DependencyGraph.GraphD
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class GraphFast<T>
        : IInternalGraph<T> where T : IEquatable<T>
    {
        #region Constants
        private const int DefaultCapacity = 4;
        #endregion

        #region Fields
        private readonly Dictionary<T, InternalNodeFast<T>> _nodes;

        private int _countLevels;
        private bool _isDirty = false;
        #endregion

        #region Constructors
        public GraphFast(int capacity)
        {
            _nodes = new Dictionary<T, InternalNodeFast<T>>(capacity);
        }

        public GraphFast()
            : this(DefaultCapacity)
        {
        }

        public GraphFast(IEnumerable<T> initialSequence)
            : this()
        {
            AddSequence(initialSequence);
        }

        public GraphFast(IEnumerable<IEnumerable<T>> initialSequences)
            : this()
        {
            AddSequences(initialSequences);
        }

        /// <summary>
        /// Creates temporary light-weight copy which is used by CanSort(sequence) method
        /// </summary>
        /// <param name="initialGraph"></param>
        private GraphFast(GraphFast<T> initialGraph)
            : this(initialGraph.CountNodes)
        {
            if (initialGraph._isDirty)
                initialGraph.ReinitializeKeys();

            var nodeKeys = new InternalNodeFast<T>[initialGraph.CountNodes];
            foreach (var node in initialGraph._nodes)
            {
                nodeKeys[node.Value.Key] = new InternalNodeFast<T>(node.Value.Value, this, node.Value.Key);
                _nodes[node.Key] = node.Value;
            }

            foreach (var node in initialGraph._nodes.Values)
            {
                foreach (var edge in node.Edges)
                {
                    nodeKeys[node.Key].Edges.Add(nodeKeys[edge.Key]);
                }
            }
        }
        #endregion

        #region Properties
        public int CountNodes
        {
            get { return _nodes.Count; }
        }

        public int CountLevels
        {
            get
            {
                ComputeLevels();
                return _countLevels;
            }
        }

        public int ReferencePoint { get; private set; }
        #endregion

        #region IInternalGraph<T> Members
        IInternalNode<T> IInternalGraph<T>.GetOrCreateNode(T publicNode)
        {
            return GetOrCreateNode(publicNode);
        }

        public INode<T> Find(T node)
        {
            return _nodes[node];
        }

        public void AddSequence(IEnumerable<T> sequence)
        {
            var nodes = sequence
                .Select(publicNode => GetOrCreateNode(publicNode))
                .ToArray();

            for (var i = 0; i < nodes.Length - 1; i++)
            {
                AddEdge(source: nodes[i], destination: nodes[i + 1]);
            }

            _isDirty = true;
        }

        public void AddSequences(IEnumerable<IEnumerable<T>> sequences)
        {
            foreach (var sequence in sequences)
            {
                AddSequence(sequence);
            }
        }

        /// <summary>
        /// Returns whether graph can be sorted in Topological order
        /// </summary>
        /// <returns></returns>
        public bool CanSort()
        {
            try
            {
                foreach (var node in Sort()) ;
            }
            catch (TopologicalSortException)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Creates temporaty graph with a new sequence and try to perform topological sorting
        /// </summary>
        /// <param name="sequence"></param>
        /// <returns></returns>
        public bool CanSort(IEnumerable<T> sequence)
        {
            var tempGraph = new GraphFast<T>(this);
            tempGraph.AddSequence(sequence);
            return tempGraph.CanSort();
        }

        public IOrderedEnumerable<INode<T>> GetNodes(int level)
        {
            return new OrderedEnumerable<INode<T>>(() => GetNodesInternal(level));
        }

        public IOrderedEnumerable<INode<T>> GetNodesBetween(int levelFrom, int levelTo)
        {
            return new OrderedEnumerable<INode<T>>(() => GetNodesBetweenInternal(levelFrom, levelTo));
        }

        /// <summary>
        /// Returns nodes in topological order
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<INode<T>> Sort()
        {
            return new OrderedEnumerable<INode<T>>(InternalSort);
        }

        public IEnumerable<INode<T>> GetRootNodes()
        {
            return GetNodes(0);
        }

        public IEnumerable<INode<T>> GetLeafNodes()
        {
            return GetNodes(CountLevels - 1);
        }

        void IInternalGraph<T>.ToFile(string filePath)
        {
            var sb = new StringBuilder();
            sb.AppendLine("From,To");

            foreach (var edge in (this as IInternalGraph<T>).Edges)
            {
                sb.AppendLine(edge[0].Value + "," + edge[1].Value);
            }

            sb.Length--;
            sb.Length--;

            File.WriteAllText(filePath, sb.ToString());
        }
        #endregion

        #region Methods
        private InternalNodeFast<T> GetOrCreateNode(T publicNode)
        {
            InternalNodeFast<T> node;
            if (!_nodes.TryGetValue(publicNode, out node))
            {
                node = new InternalNodeFast<T>(publicNode, this);
                _nodes.Add(publicNode, node);
            }

            return node;
        }

        private void AddEdge(InternalNodeFast<T> source, InternalNodeFast<T> destination)
        {
            if (source.Edges.Count(_ => _ == destination) == 0)
            {
                source.Edges.Add(destination);
            }

            if (destination.Parents.Count(_ => _ == source) == 0)
            {
                destination.Parents.Add(source); // try to move inside the previous if
}
        }

        private void ComputeLevels()
        {
            if (!_isDirty)
            {
                return;
            }

            ReinitializeKeys();

            // find the deepest node
            var orderedNodes = Sort();
            var deepestNode = new InternalNodeFast<T>(default(T), null) {ReferenceRelativeLevel = int.MinValue};
            foreach (InternalNodeFast<T> node in orderedNodes)
            {
                node.ReferenceRelativeLevel = GetMaxLevel(node.Parents) + 1;
                if (node.ReferenceRelativeLevel > deepestNode.ReferenceRelativeLevel)
                {
                    deepestNode = node;
                }
            }

            var referenceNode = deepestNode;
            ReferencePoint = 0;

            // go up and compute levels of parent nodes.
            int minLevel = VisitRelations(referenceNode);
            ReferencePoint = minLevel * (-1);
            _countLevels = deepestNode.Level + 1;
            _isDirty = false;
        }

        private void ReinitializeKeys()
        {
            var key = 0;
            foreach (var node in _nodes.Values)
            {
                node.Key = key++;
            }
        }

        private int VisitRelations(InternalNodeFast<T> startNode)
        {
            int minLevel = int.MaxValue;
            var stack = new Stack<InternalNodeFast<T>>();
            var visitedNodes = new bool[_nodes.Count];
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes[node.Key] = true;

                if (node.ReferenceRelativeLevel < minLevel)
                {
                    minLevel = node.ReferenceRelativeLevel;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes[parent.Key])
                    {
                        continue;
                    }

                    parent.ReferenceRelativeLevel = node.ReferenceRelativeLevel - 1;
                    stack.Push(parent);
                }

                foreach (var child in node.Edges)
                {
                    if (visitedNodes[child.Key])
                    {
                        continue;
                    }

                    child.ReferenceRelativeLevel = node.ReferenceRelativeLevel + 1;
                    stack.Push(child);
                }
            }

            return minLevel;
        }

        private static int GetMaxLevel(List<InternalNodeFast<T>> internalNodes)
        {
            if (internalNodes.Count == 0)
            {
                return -1;
            }

            return internalNodes.Max(_ => _.ReferenceRelativeLevel);
        }

        private IEnumerable<INode<T>> GetNodesInternal(int level)
        {
            return GetNodes(_nodes.Values.First(), (_ => _.Level == level));
        }

        private IEnumerable<INode<T>> GetNodes(INode<T> startNode, Func<IInternalNode<T>, bool> predicate)
        {
            ComputeLevels();
            var stack = new Stack<InternalNodeFast<T>>();
            var visitedNodes = new bool[_nodes.Count];
            stack.Push(startNode as InternalNodeFast<T>);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                if (visitedNodes[node.Key])
                {
                    continue;
                }

                visitedNodes[node.Key] = true;

                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes[parent.Key])
                    {
                        continue;
                    }

                    stack.Push(parent);
                }

                foreach (var child in node.Edges)
                {
                    if (visitedNodes[child.Key])
                    {
                        continue;
                    }

                    stack.Push(child);
                }
            }
        }

        internal IEnumerable<INode<T>> GetPrecedents(IInternalNode<T> startNode, Func<IInternalNode<T>, bool> predicate)
        {
            ComputeLevels();
            var visitedNodes = new bool[_nodes.Count];
            var stack = new Stack<InternalNodeFast<T>>();
            stack.Push(startNode as InternalNodeFast<T>);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes[node.Key] = true;
                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes[parent.Key])
                    {
                        continue;
                    }

                    stack.Push(parent);
                }
            }
        }

        internal IEnumerable<INode<T>> GetDescendants(IInternalNode<T> startNode, Func<IInternalNode<T>, bool> predicate)
        {
            ComputeLevels();
            var visitedNodes = new bool[_nodes.Count];
            var stack = new Stack<InternalNodeFast<T>>();
            stack.Push(startNode as InternalNodeFast<T>);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes[node.Key] = true;

                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var child in node.Edges)
                {
                    if (visitedNodes[child.Key])
                    {
                        continue;
                    }

                    stack.Push(child);
                }
            }
        }

        private IEnumerable<INode<T>> GetNodesBetweenInternal(int levelFrom, int levelTo)
        {
            var predicate = new Func<IInternalNode<T>, bool>(_ => _.Level >= levelFrom && _.Level <= levelTo);
            return GetNodes(_nodes.Values.First(), predicate)
                .OrderBy(_ => _.Level);
        }

        private IEnumerable<INode<T>> InternalSort()
        {
            var inDegree = new Dictionary<T, int>(_nodes.Count);
            var queue = new Queue<IInternalNode<T>>();
            foreach (var node in _nodes.Values)
            {
                inDegree[node.Value] = node.Parents.Count;
                if (node.Parents.Count == 0)
                {
                    queue.Enqueue(node);
                }
            }

            var resultCount = 0;
            while (queue.Count != 0)
            {
                resultCount++;
                var node = queue.Dequeue();
                yield return node;

                foreach (var child in node.Edges)
                {
                    if (--inDegree[child.Value] == 0)
                    {
                        queue.Enqueue(child);
                    }
                }
            }

            if (_nodes.Count != resultCount)
            {
                throw new TopologicalSortException("Topological sort failed due to loops in the graph");
            }
        }
        #endregion

        #region TestHelpers
        IEnumerable<IInternalNode<T>[]> IInternalGraph<T>.Edges
        {
            get
            {
                foreach (var node in _nodes.Values)
                {
                    foreach (var child in node.Edges)
                    {
                        yield return new[] {node, child};
                    }
                }
            }
        }

        IEnumerable<IInternalNode<T>[]> IInternalGraph<T>.BackEdges
        {
            get
            {
                foreach (var node in _nodes.Values)
                {
                    foreach (var parent in node.Parents)
                    {
                        yield return new[] {node, parent}; // Attention: the order is opposite!
                    }
                }
            }
        }

        public IEnumerable<INode<T>> Nodes
        {
            get { return _nodes.Values; }
        }
        #endregion
    }
}