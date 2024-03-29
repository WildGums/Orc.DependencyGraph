﻿namespace Orc.DependencyGraph.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;
    using DependencyGraph;

    internal static class GraphTestHelper
    {
        public static IGraph<int> CreateExampleGraph(Type type)
        {
            var graph = CreateEmptyGraph(type);
            graph.AddSequences(new[]
            {
            new[] {51, 61},
            new[] {51, 62},

            new[] {41, 51},
            new[] {42, 51},
            new[] {43, 51},
            new[] {44, 51},
            new[] {45, 51},
            new[] {46, 51},

            new[] {31, 41},
            new[] {31, 42},
            new[] {31, 43},

            new[] {32, 46},

            new[] {21, 31},
            new[] {22, 31},
            new[] {23, 31},
            new[] {24, 31},

            new[] {25, 32},
            new[] {26, 32},

            new[] {11, 27, 32, 46, 51, 61}, // just to show the possibility
            new[] {12, 27, 32, 46, 51, 61}, // just to show the possibility
        });
            return graph;
        }

        public static IGraph<int> CreateExampleGraph_ForImmediateTests(Type type)
        {
            // this example contains difficult cases:
            // - children of node 31 are on different levels
            // - parents of node 51 are on different levels
            var graph = CreateExampleGraph(type);

            Assert.That(graph, Is.Not.Null);

            graph.AddSequences(new[]
            {
            new[] {31, 51},
        });
            return graph;
        }

        public static IGraph<int> CreateSimpleGraph(Type type)
        {
            var graph = CreateEmptyGraph(type);

            Assert.That(graph, Is.Not.Null);

            graph.AddSequences(new[]
            {
            new[] {41, 51, 61, 100},
            new[] {42, 52, 62, 100},
        });
            return graph;
        }

        public static void AssertConsistsOfSequences(IGraph<int> graph, int[][] sequences)
        {
            foreach (var sequence in sequences)
            {
                for (var index = 0; index < sequence.Length - 1; index++)
                {
                    var node = graph.Find(sequence[index]);
                    Assert.That(node?.ImmediateDescendants.Count(x => x.Value == sequence[index + 1]), Is.EqualTo(1));
                }
            }
        }

        public static void AssertConsistsOfBackSequences(IGraph<int> graph, int[][] sequences)
        {
            foreach (var sequence in sequences)
            {
                for (var index = 0; index < sequence.Length - 1; index++)
                {
                    var node = graph.Find(sequence[index]);
                    Assert.That(node?.ImmediatePrecedents.Count(x => x.Value == sequence[index + 1]), Is.EqualTo(1));
                }
            }
        }

        public static void AssertContainsSequence(int[][] sequences, INode<int>[] edge)
        {
            Assert.That(sequences.Count(_ => _[0] == edge[0].Value && _[1] == edge[1].Value), Is.EqualTo(1), $"Edge does not contain a sequence {edge[0].Value}-{edge[1].Value}");
        }

        public static void AssertContainsBackSequence(int[][] sequences, INode<int>[] edge)
        {
            Assert.That(sequences.Count(_ => _[0] == edge[0].Value && _[1] == edge[1].Value), Is.EqualTo(1), $"Edge does not contain a back sequence {edge[0].Value}-{edge[1].Value}");
        }

        public static void AssertCollectionsConsistsOfNodes(ICollection<int>? expectedNodes, IEnumerable<INode<int>>? nodes)
        {
            ArgumentNullException.ThrowIfNull(expectedNodes);
            ArgumentNullException.ThrowIfNull(nodes);

            var count = 0;

            foreach (var node in nodes)
            {
                Assert.That(expectedNodes.Contains(node.Value), Is.True, $"Expected result does not contain node {node.Value}");
                count++;
            }
            Assert.That(count, Is.EqualTo(expectedNodes.Count), $"Node count expected to be {expectedNodes.Count}, but was {count}");
        }

        public static void AssertNodesAreOrderedByLevel(IEnumerable<INode<int>>? orderedNodes)
        {
            ArgumentNullException.ThrowIfNull(orderedNodes);

            var lastLevel = int.MinValue;
            foreach (var node in orderedNodes)
            {
                if (node.Level >= lastLevel)
                {
                    lastLevel = node.Level;
                }
                else
                {
                    Assert.Fail("Order of nodes is not correct");
                }
            }
        }

        public static IGraph<int> CreateEmptyGraph(Type type)
        {
            var instance = Activator.CreateInstance(type) as IGraph<int>;

            Assert.That(instance, Is.Not.Null);

            return instance;
        }
    }
}
