﻿namespace Orc.DependencyGraph.GraphD
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;

    [DebuggerDisplay("{Value.ToString()}: [{(this as IInternalNode<T>).PrintImmediatePrecedents}]")]
    internal class InternalNodeFast<T> : IInternalNode<T>
        where T : IEquatable<T>
    {
        private readonly GraphFast<T> _graph;

        public int Key;

        internal InternalNodeFast(T publicNode, GraphFast<T> graph)
        {
            ArgumentNullException.ThrowIfNull(publicNode);
            ArgumentNullException.ThrowIfNull(graph);

            Edges = new List<InternalNodeFast<T>>();
            Parents = new List<InternalNodeFast<T>>();

            Value = publicNode;
            _graph = graph;
        }

        internal InternalNodeFast(T publicNode, GraphFast<T> graph, int key)
            : this(publicNode, graph)
        {
            Key = key;
        }

        public List<InternalNodeFast<T>> Edges { get; private set; }
        public List<InternalNodeFast<T>> Parents { get; private set; }

        IEnumerable<IInternalNode<T>> IInternalNode<T>.Edges
        {
            get { return Edges; }
        }

        IEnumerable<IInternalNode<T>> IInternalNode<T>.Parents
        {
            get { return Parents; }
        }

        public T Value { get; internal set; }

        public IGraph<T> Graph
        {
            get { return _graph; }
        }

        public int Level
        {
            get { return _graph.ReferencePoint + ReferenceRelativeLevel; }
        }

        public int ReferenceRelativeLevel { get; set; }

        public IOrderedEnumerable<INode<T>> Precedents
        {
            get
            {
                return new OrderedEnumerable<INode<T>>(
                    () => _graph.GetPrecedents(this, (_ => _.Level < Level)).OrderBy(_ => _.Level));
            }
        }

        public IOrderedEnumerable<INode<T>> Descendants
        {
            get
            {
                return new OrderedEnumerable<INode<T>>(
                    () => _graph.GetDescendants(this, (_ => _.Level > Level)).OrderBy(_ => _.Level));
            }
        }

        public IOrderedEnumerable<INode<T>> ImmediatePrecedents
        {
            get { return new OrderedEnumerable<INode<T>>(() => Parents.OrderBy(x => x.Level)); }
        }

        public IOrderedEnumerable<INode<T>> ImmediateDescendants
        {
            get { return new OrderedEnumerable<INode<T>>(() => Edges.OrderBy(x => x.Level)); }
        }

        public IOrderedEnumerable<INode<T>> TerminatingPrecedents
        {
            get
            {
                return new OrderedEnumerable<INode<T>>(
                    () => _graph.GetPrecedents(this, (_ => _.Level < Level && !_.Parents.Any())).OrderBy(_ => _.Level));
            }
        }

        public IOrderedEnumerable<INode<T>> TerminatingDescendants
        {
            get
            {
                return new OrderedEnumerable<INode<T>>(
                    () => _graph.GetDescendants(this, (_ => _.Level > Level && !_.Edges.Any())).OrderBy(_ => _.Level));
            }
        }

        string IInternalNode<T>.PrintImmediatePrecedents
        {
            get
            {
                if (!Edges.Any())
                {
                    return string.Empty;
                }

                var sb = new StringBuilder();
                foreach (var immediatePrecedents in Edges)
                {
                    sb.Append(immediatePrecedents.Value);
                    sb.Append(", ");
                }

                return sb.Remove(sb.Length - 2, 2).ToString();
            }
        }

        public IOrderedEnumerable<INode<T>> GetNeighbours(int relativeLevelFrom, int relativeLevelTo)
        {
            return new OrderedEnumerable<INode<T>>(() => GetNeighboursInternal(relativeLevelFrom, relativeLevelTo));
        }

        private IEnumerable<INode<T>> GetNeighboursInternal(int relativeLevelFrom, int relativeLevelTo)
        {
            if (relativeLevelFrom < 0 && relativeLevelTo < 0)
            {
                return _graph.GetPrecedents(this, (_ => _ != this && _.Level >= Level + relativeLevelFrom && _.Level <= Level + relativeLevelTo)).OrderBy(_ => _.Level);
            }

            if (relativeLevelFrom > 0 && relativeLevelTo > 0)
            {
                return _graph.GetDescendants(this, (_ => _ != this && _.Level >= Level + relativeLevelFrom && _.Level <= Level + relativeLevelTo)).OrderBy(_ => _.Level);
            }

            var precedents = _graph.GetPrecedents(this, (_ => _ != this && _.Level >= Level + relativeLevelFrom && _.Level <= Level + relativeLevelTo));
            var descendants = _graph.GetDescendants(this, (_ => _ != this && _.Level >= Level + relativeLevelFrom && _.Level <= Level + relativeLevelTo));

            return precedents.Union(descendants).OrderBy(_ => _.Level);
        }
    }
}
