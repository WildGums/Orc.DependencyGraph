﻿namespace Orc.DependencyGraph.Tests
{
    using System;
    using System.Linq;
    using GraphB;
    using GraphD;
    using NUnit.Framework;

    [TestFixture(typeof(Graph<>))]
    [TestFixture(typeof(GraphFast<>))]
    [TestFixture(typeof(GraphB<>))]
    public class NodeTest
    {
        public NodeTest(Type targetGenericGraph)
        {
            TargetGraph = targetGenericGraph.MakeGenericType(typeof(int));
        }

        private Type TargetGraph { get; set; }

        [TestCase(11, 0)]
        [TestCase(25, 1)]
        [TestCase(31, 2)]
        [TestCase(44, 3)]
        [TestCase(51, 4)]
        [TestCase(61, 5)]
        public void LevelCalculatesCorrectly(int node, int expectedLevel)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);
            _ = graph.CountLevels; // force graph to rebuild. fix this later
            var target = graph.Find(node);
            Assert.That(target?.Level, Is.EqualTo(expectedLevel), $"Level of node {target?.Value} expected to be {expectedLevel}, but is {target?.Level}");
        }

        [TestCase(31, new[] { 21, 22, 23, 24, 41, 42, 43, 51 })]
        [TestCase(32, new[] { 25, 26, 27, 46, 51 })]
        public void GetNeighboursReturnsNeighboursOfTheNodeInTheRangeOfLevels(int node, int[] expectedNeighbours)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedNeighbours, graph?.Find(node)?.GetNeighbours(-1, 2));
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph?.Find(node)?.GetNeighbours(-1, 2));
        }

        [TestCase(32, new[] { 11, 12, 25, 26, 27 })]
        [TestCase(42, new[] { 21, 22, 23, 24, 31 })]
        public void PrecedentsReturnsPrecedentsOfTheNode(int node, int[] expectedPrecedents)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedPrecedents, graph?.Find(node)?.Precedents);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph?.Find(node)?.Precedents);
        }

        [TestCase(31, new[] { 41, 42, 43, 51, 61, 62 })]
        [TestCase(32, new[] { 46, 51, 61, 62 })]
        public void DescendantsReturnsDescendantsOfTheNode(int node, int[] expectedDescendants)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedDescendants, graph?.Find(node)?.Descendants);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph?.Find(node)?.Descendants);
        }

        [TestCase(26, new int[] { })]
        [TestCase(61, new[] { 51 })]
        [TestCase(27, new[] { 11, 12 })]
        [TestCase(32, new[] { 25, 26, 27 })]
        [TestCase(51, new[] { 41, 42, 43, 44, 45, 46, 31 })]
        public void ImmediatePrecedentsReturnsParentsOfTheNode(int node, int[] expectedImmediatePrecedents)
        {
            var graph = GraphTestHelper.CreateExampleGraph_ForImmediateTests(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedImmediatePrecedents, graph.Find(node)?.ImmediatePrecedents);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph.Find(node)?.ImmediatePrecedents);
        }

        [TestCase(62, new int[] { })]
        [TestCase(27, new[] { 32 })]
        [TestCase(31, new[] { 41, 42, 43, 51 })]
        [TestCase(51, new[] { 61, 62 })]
        public void ImmediateDescendantsReturnsChildrenOfTheNode(int node, int[] expectedImmediateDescendants)
        {
            var graph = GraphTestHelper.CreateExampleGraph_ForImmediateTests(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedImmediateDescendants, graph.Find(node)?.ImmediateDescendants);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph.Find(node)?.ImmediateDescendants);
        }

        [TestCase(43, new[] { 21, 22, 23, 24 })]
        [TestCase(51, new[] { 11, 12, 21, 22, 23, 24, 25, 26, 44, 45 })]
        [TestCase(11, new int[] { })]
        public void TerminatingPrecedentsReturnsPrecedentsOfTheNodeOnTheZeroLevel(int node, int[] expectedTerminatingPrecedents)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedTerminatingPrecedents, graph?.Find(node)?.TerminatingPrecedents);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph?.Find(node)?.TerminatingPrecedents);
        }

        [TestCase(31, new[] { 61, 62 })]
        [TestCase(11, new[] { 61, 62 })]
        [TestCase(62, new int[] { })]
        public void TerminatingDescendantsReturnsDescendantsOfTheNodeOnTheLastLevel(int node, int[] expectedTerminatingDescendants)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);
            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedTerminatingDescendants, graph?.Find(node)?.TerminatingDescendants);
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph?.Find(node)?.TerminatingDescendants);
        }

        [Test]
        public void InitialLevelCalculatesCorrectly()
        {
            var graph = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(graph, Is.Not.Null);

            graph.AddSequences(new[]
            {
                new[] {2, 4, 3, 9},
                new[] {1, 3},
                new[] {4, 5, 7, 8, 3},
                new[] {4, 6, 8},
                new[] {2, 9},
            });

            var countLevels = graph.CountLevels;
            for (var level = 0; level < countLevels; level++)
            {
                var nodesOnLevel = graph.GetNodes(level).Count();
                Assert.That(nodesOnLevel > 0, Is.True);
            }
        }
    }
}
