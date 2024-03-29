﻿namespace Orc.DependencyGraph
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public interface IGraph<T>
        where T : IEquatable<T>
    {
        /// <summary>
        /// Returns all nodes of the graph
        /// </summary>
        IEnumerable<INode<T>> Nodes { get; }

        /// <summary>
        /// Gets the number of nodes contained in the Graph
        /// </summary>
        int CountNodes { get; }

        /// <summary>
        /// Gets the number of levels contained in the Graph
        /// </summary>
        int CountLevels { get; }

        /// <summary>
        /// Finds a specific node within a graph by a value
        /// </summary>
        /// <param name="value">The value of node to search</param>
        /// <returns>The node associated with the specified value</returns>
        INode<T>? Find(T value);

        /// <summary>
        /// Adds a sequence to the graph
        /// </summary>
        /// <example>
        /// AddSequence(new[] { 1, 2, 3 } );
        /// </example>
        /// <param name="sequence">The sequence of nodes to be added to the graph.</param>
        /// <exception cref="ArgumentException">The length of sequence is shorter than 2.</exception>
        void AddSequence(IEnumerable<T> sequence);

        /// <summary>
        /// Adds multiple sequences to the graph.
        /// </summary>
        /// <example>
        /// AddSequence(
        ///     new[]
        ///     {
        ///         new[] { 1, 2, 3 },
        ///         new[] { 2, 4 }
        ///     });
        /// </example>
        /// <param name="sequences">The collection whose sequences should be added be added to the graph.</param>
        void AddSequences(IEnumerable<IEnumerable<T>> sequences);

        /// <summary>
        /// Determines whether graph can be sorted in topological order (i.e. whether graph is DAG)
        /// </summary>
        /// <returns>True, if graph can be sorted in topological order, otherwise false</returns>
        bool CanSort();

        /// <summary>
        /// Determines whether graph can be sorted in topological order (i.e. whether graph is DAG) after adding a given sequence.
        /// </summary>
        /// <param name="sequence">The sequence of nodes to be tested.</param>
        /// <returns>True, if after adding the given sequence graph can be sorted in topological order, otherwise false</returns>
        bool CanSort(IEnumerable<T> sequence);

        /// <summary>
        /// Returns nodes withing a given level in a topological order
        /// </summary>
        /// <param name="level">A value that defines a level of the nodes to search for</param>
        /// <returns>An IOrderedEnumerable whose elements are sorted in topological order</returns>
        IOrderedEnumerable<INode<T>> GetNodes(int level);

        /// <summary>
        /// Returns nodes between given levels in a topological order
        /// </summary>
        /// <param name="levelFrom"></param>
        /// <param name="levelTo"></param>
        /// <returns>An IOrderedEnumerable whose elements are sorted in topological order</returns>
        IOrderedEnumerable<INode<T>> GetNodesBetween(int levelFrom, int levelTo);

        /// <summary>
        /// Returns the nodes in topological order
        /// </summary>
        /// <returns>An IOrderedEnumerable whose elements are sorted in topological order</returns>
        /// <exception cref="TopologicalSortException">The graph cannot be sorted in topological order (i.e. contains loops)</exception>
        IOrderedEnumerable<INode<T>> Sort();

        /// <summary>
        /// Returns nodes on the 0 level of the Graph
        /// </summary>
        IEnumerable<INode<T>> GetRootNodes();

        /// <summary>
        /// Returns nodes on the last level of the Graph
        /// </summary>
        IEnumerable<INode<T>> GetLeafNodes();
    }
}
