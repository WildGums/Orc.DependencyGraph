// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Graph.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.DependencyGraph.GraphD
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Text;

    public class Graph<T>
        : IInternalGraph<T> where T : IEquatable<T>
    {
        #region Constants
        private const int DefaultCapacity = 4;
        #endregion

        #region Fields
        private readonly Dictionary<T, InternalNode<T>> _nodes;

        private int _countLevels;
        private bool _isDirty = false;
        #endregion

        #region Constructors
        public Graph(int capacity)
        {
            _nodes = new Dictionary<T, InternalNode<T>>(capacity);
        }

        public Graph()
            : this(DefaultCapacity)
        {
        }

        public Graph(IEnumerable<T> initialSequence)
            : this()
        {
            AddSequence(initialSequence);
        }

        public Graph(IEnumerable<IEnumerable<T>> initialSequences)
            : this()
        {
            AddSequences(initialSequences);
        }

        private Graph(Graph<T> initialGraph)
            : this(initialGraph.CountNodes)
        {
            var sequences = (initialGraph as IInternalGraph<T>).Edges.Select(_ => new[] { _[0].Value, _[1].Value });
            AddSequences(sequences);
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

        internal int ReferencePoint { get; private set; }
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
                .Select(publicNode => (INode<T>)GetOrCreateNode(publicNode))
                .ToList();

            for (var i = 0; i < nodes.Count - 1; i++)
            {
                var src = nodes[i];
                var dst = nodes[i + 1];
                AddEdge(src, dst);
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
            var tempGraph = new Graph<T>(this);
            tempGraph.AddSequence(sequence);
            return tempGraph.CanSort();
        }

        public IOrderedEnumerable<INode<T>> GetNodes(int level)
        {
            return new OrderedEnumerable<INode<T>>(
                () => GetNodes(_nodes.Values.First(), (_ => _.Level == level)));
        }

        public IOrderedEnumerable<INode<T>> GetNodesBetween(int levelFrom, int levelTo)
        {
            var predicate = new Func<IInternalNode<T>, bool>(_ => _.Level >= levelFrom && _.Level <= levelTo);
            return new OrderedEnumerable<INode<T>>(
                () => GetNodes(_nodes.Values.First(), predicate).OrderBy(_ => _.Level));
        }

        /// <summary>
        /// Returns nodes in topological order
        /// </summary>
        /// <returns></returns>
        public IOrderedEnumerable<INode<T>> Sort()
        {
            return new OrderedEnumerable<INode<T>>(SortInternal);
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
            var namesDictionary = new Dictionary<string, int>();

            var getId = new Func<T, int>(node =>
            {
                var nodeKey = node.ToString();
                if (namesDictionary.ContainsKey(nodeKey)) return namesDictionary[nodeKey];
                var newId = namesDictionary.Count;
                namesDictionary[nodeKey] = newId;
                return newId;
            });
            // export Edges
            var sb = new StringBuilder();
            sb.AppendLine("From,To");

            foreach (var edge in (this as IInternalGraph<T>).Edges)
            {
                sb.AppendLine(getId(edge[0].Value) + "," + getId(edge[1].Value));
            }

            sb.Length--;
            sb.Length--;

            File.WriteAllText(filePath, sb.ToString());

            // export Node names
            var sb1 = new StringBuilder();
            sb1.AppendLine("ID,Property,Value");

            foreach (var node in namesDictionary)
            {
                sb1.AppendLine(node.Value + ",Order," + node.Key);
            }

            sb1.Length--;
            sb1.Length--;

            var propertiesPath = Path.Combine(Directory.GetParent(filePath).FullName, "Properties.csv");
            File.WriteAllText(propertiesPath, sb1.ToString());
        }
        #endregion

        #region Methods
        private InternalNode<T> GetOrCreateNode(T publicNode)
        {
            InternalNode<T> node;
            if (!_nodes.TryGetValue(publicNode, out node))
            {
                node = new InternalNode<T>(publicNode, this);
                _nodes.Add(publicNode, node);
            }

            return node;
        }

        private void AddEdge(INode<T> source, INode<T> destination)
        {
            var src = (InternalNode<T>)source;
            var dst = (InternalNode<T>)destination;

            if (src.Edges.Count(_ => _ == dst) == 0)
            {
                src.Edges.Add(dst);
            }

            if (dst.Parents.Count(_ => _ == src) == 0)
            {
                dst.Parents.Add(src); // try to move inside the previous if
            }
        }

        private void ComputeLevels()
        {
            if (!_isDirty) return;

            // find the deepest node
            var orderedNodes = Sort();
            var deepestNode = new InternalNode<T>(default(T), null) { ReferenceRelativeLevel = int.MinValue };
            foreach (InternalNode<T> node in orderedNodes)
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

        private int VisitRelations(InternalNode<T> startNode)
        {
            int minLevel = int.MaxValue;
            var visitedNodes = new HashSet<InternalNode<T>>();
            var stack = new Stack<InternalNode<T>>();
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes.Add(node);
                if (node.ReferenceRelativeLevel < minLevel)
                {
                    minLevel = node.ReferenceRelativeLevel;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes.Contains(parent))
                    {
                        continue;
                    }

                    parent.ReferenceRelativeLevel = node.ReferenceRelativeLevel - 1;
                    stack.Push(parent);
                }
                foreach (var child in node.Edges)
                {
                    if (visitedNodes.Contains(child))
                    {
                        continue;
                    }

                    child.ReferenceRelativeLevel = node.ReferenceRelativeLevel + 1;
                    stack.Push(child);
                }
            }
            return minLevel;
        }

        private static int GetMaxLevel(List<InternalNode<T>> internalNodes)
        {
            if (internalNodes.Count == 0)
            {
                return -1;
            }

            return internalNodes.Max(_ => _.ReferenceRelativeLevel);
        }

        private IEnumerable<INode<T>> GetNodes(INode<T> startNode, Func<IInternalNode<T>, bool> predicate)
        {
            ComputeLevels();
            var visitedNodes = new HashSet<IInternalNode<T>>();
            var stack = new Stack<IInternalNode<T>>();
            stack.Push(startNode as IInternalNode<T>);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                if (!visitedNodes.Add(node))
                {
                    continue;
                }

                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes.Contains(parent))
                    {
                        continue;
                    }

                    stack.Push(parent);
                }
                foreach (var child in node.Edges)
                {
                    if (visitedNodes.Contains(child))
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
            var visitedNodes = new HashSet<IInternalNode<T>>();
            var stack = new Stack<IInternalNode<T>>();
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes.Add(node);

                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var parent in node.Parents)
                {
                    if (visitedNodes.Contains(parent))
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
            var visitedNodes = new HashSet<IInternalNode<T>>();
            var stack = new Stack<IInternalNode<T>>();
            stack.Push(startNode);
            while (stack.Count != 0)
            {
                var node = stack.Pop();
                visitedNodes.Add(node);
                if (predicate(node))
                {
                    yield return node;
                }

                foreach (var child in node.Edges)
                {
                    if (visitedNodes.Contains(child))
                    {
                        continue;
                    }

                    stack.Push(child);
                }
            }
        }

        private IEnumerable<INode<T>> SortInternal()
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
        // may be I need to move this to a partial class?

        IEnumerable<IInternalNode<T>[]> IInternalGraph<T>.Edges
        {
            get
            {
                foreach (var node in _nodes.Values)
                {
                    foreach (var child in node.Edges)
                    {
                        yield return new[] { node, child };
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
                        yield return new[] { node, parent }; // Attention: the order is opposite!
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