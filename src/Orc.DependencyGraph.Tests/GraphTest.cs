namespace Orc.DependencyGraph.Tests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using GraphB;
    using GraphD;
    using NUnit.Framework;

    [TestFixture(typeof(Graph<>))]
    [TestFixture(typeof(GraphFast<>))]
    [TestFixture(typeof(GraphB<>))]
    public class GraphTest
    {
        public GraphTest(Type targetGenericGraph)
        {
            TargetGraph = targetGenericGraph.MakeGenericType(typeof(int));
        }

        private Type TargetGraph { get; set; }

        [TestCase(31)]
        [TestCase(45)]
        [TestCase(11)]
        public void FindReturnsCorrectValue(int node)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);
            Assert.That(graph.Find(node)?.Value, Is.EqualTo(node));
        }

        [TestCase(0, new[] { 11, 12 })]
        [TestCase(1, new[] { 21, 22, 23, 24, 25, 26, 27 })]
        [TestCase(2, new[] { 31, 32 })]
        [TestCase(3, new[] { 41, 42, 43, 44, 45, 46 })]
        [TestCase(4, new[] { 51 })]
        [TestCase(5, new[] { 61, 62 })]
        public void GetNodesReturnsNodesOnTheLevel(int level, int[] expectedNodes)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedNodes, graph.GetNodes(level));
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph.GetNodes(level));
        }

        [TestCase(new[] { 11, 12 })]
        public void GetRootNodesReturnsNodesOnZeroLevel(int[] expectedNodes)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedNodes, graph.GetRootNodes());
        }

        [TestCase(new[] { 61, 62 })]
        public void GetLeafNodesReturnsNodesOnLastLevel(int[] expectedNodes)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedNodes, graph.GetLeafNodes());
        }

        [TestCase(0, 1, new[] { 11, 12, 21, 22, 23, 24, 25, 26, 27 })]
        [TestCase(1, 2, new[] { 21, 22, 23, 24, 25, 26, 27, 31, 32 })]
        [TestCase(0, 2, new[] { 11, 12, 21, 22, 23, 24, 25, 26, 27, 31, 32 })]
        [TestCase(4, 5, new[] { 51, 61, 62 })]
        public void GetNodesWithBetweenReturnsNodesOnTheLevelsInTheRange(int levelFrom, int levelTo, int[] expectedNodes)
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            GraphTestHelper.AssertCollectionsConsistsOfNodes(expectedNodes, graph.GetNodesBetween(levelFrom, levelTo));
            GraphTestHelper.AssertNodesAreOrderedByLevel(graph.GetNodesBetween(levelFrom, levelTo));
        }

        private static void AssertNodesAreInTopologicalOrder(IEnumerable<INode<int>> nodes)
        {
            var visitedNodes = new List<int>();
            foreach (var node in nodes)
            {
                if (node.ImmediatePrecedents.Any())
                {
                    foreach (var parent in node.ImmediatePrecedents)
                    {
                        if (!visitedNodes.Contains(parent.Value))
                        {
                            Assert.Fail($"Topological sort is not valid. {node.Value} is before {parent.Value}");
                        }
                    }
                }

                visitedNodes.Add(node.Value);
            }
        }

        [Test]
        public void AddAddsExistingSequence()
        {
            // {41, 51, 61, 100},
            // {42, 52, 62, 100},
            var graph = GraphTestHelper.CreateSimpleGraph(TargetGraph);
            Assert.That(graph.CountNodes, Is.EqualTo(7));
            graph.AddSequence(new[] { 51, 61 });
            graph.AddSequence(new[] { 42, 52 });
            Assert.That(graph.CountNodes, Is.EqualTo(7));

            GraphTestHelper.AssertConsistsOfSequences(graph, new[]
            {
                new[] {41, 51},
                new[] {51, 61},
                new[] {61, 100},
                new[] {42, 52},
                new[] {52, 62},
                new[] {62, 100},
                new[] {41, 51, 61, 100},
                new[] {42, 52, 62, 100},
            });
        }

        [Test]
        public void AddAddsSequence()
        {
            var target = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(target, Is.Not.Null);

            target.AddSequence(new[] { 0, 1 });
            target.AddSequence(new[] { 1, 2 });
            target.AddSequence(new[] { 2, 3 });
            target.AddSequence(new[] { 3, 4 });
            target.AddSequence(new[] { 4, 5 });
            target.AddSequence(new[] { 5, 6 });
            target.AddSequence(new[] { 6, 7 });
            target.AddSequence(new[] { 7, 8 });
            target.AddSequence(new[] { 8, 9 });
            Assert.That(target.CountNodes, Is.EqualTo(10));
            GraphTestHelper.AssertConsistsOfSequences(target, new[]
            {
                new[] {0, 1},
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 4},
                new[] {4, 5},
                new[] {5, 6},
                new[] {6, 7},
                new[] {7, 8},
                new[] {8, 9},
                new[] {0, 1, 2, 3, 4, 5, 6, 7, 8, 9},
            });
            GraphTestHelper.AssertConsistsOfBackSequences(target, new[]
            {
                new[] {1, 0},
                new[] {2, 1},
                new[] {3, 2},
                new[] {4, 3},
                new[] {5, 4},
                new[] {6, 5},
                new[] {7, 6},
                new[] {8, 7},
                new[] {9, 8},
                new[] {9, 8, 7, 6, 5, 4, 3, 2, 1, 0},
            });
        }

        [Test]
        public void AddRangeAddsSequences()
        {
            var graph = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(graph, Is.Not.Null);

            graph.AddSequences(new[]
            {
                new[] {41, 51, 61, 100},
                new[] {42, 52, 62, 100},
            });
            Assert.That(graph.CountNodes, Is.EqualTo(7));
            GraphTestHelper.AssertConsistsOfSequences(graph, new[]
            {
                new[] {41, 51},
                new[] {51, 61},
                new[] {61, 100},
                new[] {42, 52},
                new[] {52, 62},
                new[] {62, 100},
                new[] {41, 51, 61, 100},
                new[] {42, 52, 62, 100},
            });
        }

        [Test]
        public void CanSortReturnsWhetherGraphCanBeSorted1()
        {
            var graph = GraphTestHelper.CreateSimpleGraph(TargetGraph);
            Assert.That(graph.CanSort(), Is.True);
        }

        [Test]
        public void CanSortReturnsWhetherGraphCanBeSorted2()
        {
            // {41, 51, 61, 100},
            // {42, 52, 62, 100},
            var graph = GraphTestHelper.CreateSimpleGraph(TargetGraph);
            Assert.That(graph.CanSort(), Is.True);
            graph.AddSequences(new[]
            {
                new[] {100, 41},
            });
            Assert.That(graph.CanSort(), Is.False);
        }

        [Test]
        public void CanSortReturnsWhetherGraphWithTheSequenceCanBeSorted1()
        {
            var graph = GraphTestHelper.CreateSimpleGraph(TargetGraph);
            Assert.That(graph.CanSort(new[] { 43, 52, 63, 100 }), Is.True);
        }

        [Test]
        public void CanSortReturnsWhetherGraphWithTheSequenceCanBeSorted2()
        {
            var graph = GraphTestHelper.CreateSimpleGraph(TargetGraph);
            Assert.That(graph.CanSort(new[] { 100, 41 }), Is.False);
        }

        [Test]
        public void ConstructorWithInitialSequenceArgumentInitializesGraph()
        {
            var target = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(target, Is.Not.Null);

            target.AddSequence(new[] { 1, 2, 3, 4, 5 });
            Assert.That(target.CountNodes, Is.EqualTo(5));
            GraphTestHelper.AssertConsistsOfSequences(target, new[]
            {
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 4},
                new[] {4, 5},
                new[] {1, 2, 3, 4, 5},
            });
        }

        [Test]
        public void ConstructorWithInitialSequencesArgumentInitializesGraph()
        {
            var target = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(target, Is.Not.Null);

            target.AddSequences(new[]
            {
                new[] {1, 2, 3, 4, 5},
                new[] {11, 12, 13, 14, 5},
            });
            Assert.That(target.CountNodes, Is.EqualTo(9));
            GraphTestHelper.AssertConsistsOfSequences(target, new[]
            {
                new[] {1, 2},
                new[] {2, 3},
                new[] {3, 4},
                new[] {4, 5},
                new[] {11, 12},
                new[] {12, 13},
                new[] {13, 14},
                new[] {14, 5},
                new[] {11, 12, 13, 14, 5},
                new[] {1, 2, 3, 4, 5},
            });
        }

        [Test]
        public void CountLevelsReturnsNumberOfLevelsInGraph1()
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            Assert.That(graph.CountLevels, Is.EqualTo(6));
        }

        [Test]
        public void CountLevelsReturnsNumberOfLevelsInGraph2()
        {
            var graph = GraphTestHelper.CreateEmptyGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            graph.AddSequences(new[]
            {
                new[] {1, 2, 6},
                new[] {2, 3, 4, 5},
                new[] {9, 8, 7, 6},
            });
            Assert.That(graph.CountLevels, Is.EqualTo(6));
        }

        [Test]
        public void CountReturnsCountOfGraph()
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            Assert.That(graph.CountNodes, Is.EqualTo(20));
        }

        [Test]
        public void SingleNodeSequenceIsAllowed()
        {
            var graph = GraphTestHelper.CreateEmptyGraph(TargetGraph)!;

            Assert.That(graph, Is.Not.Null);

            graph.AddSequence(new[] { 1 });
            GraphTestHelper.AssertCollectionsConsistsOfNodes(new[] { 1 }, graph.GetRootNodes());
            GraphTestHelper.AssertCollectionsConsistsOfNodes(new[] { 1 }, graph.GetLeafNodes());
            GraphTestHelper.AssertConsistsOfSequences(graph, new[]
            {
                new[] {1},
            });
        }

        [Test]
        public void SortReturnsNodesInTopologicalOrder()
        {
            var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

            Assert.That(graph, Is.Not.Null);

            var nodes = graph.Sort();

            Assert.That(nodes.Count(), Is.EqualTo(20));
            AssertNodesAreInTopologicalOrder(nodes);
        }

        [Test]
        public void SortThrowsTopologicalSortExceptionExceptionWhenGraphContainsLoops()
        {
            Assert.Throws<TopologicalSortException>(() =>
            {
                var graph = GraphTestHelper.CreateExampleGraph(TargetGraph);

                Assert.That(graph, Is.Not.Null);

                graph.AddSequence(new[] { 61, 11 });
                graph.Sort().ToArray();
            });
        }
    }
}
