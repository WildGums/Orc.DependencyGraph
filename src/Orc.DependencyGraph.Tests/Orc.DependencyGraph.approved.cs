[assembly: System.Resources.NeutralResourcesLanguage("en-US")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Orc.DependencyGraph.PerformanceTest")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("Orc.DependencyGraph.Tests")]
[assembly: System.Runtime.Versioning.TargetFramework(".NETCoreApp,Version=v6.0", FrameworkDisplayName="")]
public static class ModuleInitializer
{
    public static void Initialize() { }
}
namespace Orc.DependencyGraph.GraphB
{
    public class GraphB<T> : Orc.Sort.TopologicalSort.TopologicalSort<T>, Orc.DependencyGraph.IGraph<T>
        where T : System.IEquatable<T>
    {
        protected System.Collections.Generic.IList<Orc.DependencyGraph.INode<T>> _graphList;
        protected System.Collections.Generic.IList<Orc.DependencyGraph.INode<T>> _graphSort;
        protected System.Collections.Generic.List<int> _levelList;
        public GraphB() { }
        public GraphB(bool usesPriority, bool usesTracking, int capacity) { }
        public int CountLevels { get; }
        public int CountNodes { get; }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> Nodes { get; }
        public void AddSequence(System.Collections.Generic.IEnumerable<T> sequence) { }
        public void AddSequences(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> sequences) { }
        public bool CanSort() { }
        public Orc.DependencyGraph.INode<T> Find(T node) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetLeafNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodes(int level) { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodesBetween(int levelFrom, int levelTo) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetNodesRelatedTo(T node) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetNodesRelatedTo(T node, int levelFrom, int levelTo) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetRootNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Sort() { }
        public class Node<N> : Orc.DependencyGraph.INode<N>
            where N : System.IEquatable<N>
        {
            public Node(Orc.DependencyGraph.GraphB.GraphB<N> graph, int index) { }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> Descendants { get; }
            public Orc.DependencyGraph.GraphB.GraphB<N> Graph { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> ImmediateDescendants { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> ImmediatePrecedents { get; }
            public int Level { get; }
            public Orc.DependencyGraph.INode<N> Next { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> Precedents { get; }
            public Orc.DependencyGraph.INode<N> Previous { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> TerminatingDescendants { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> TerminatingPrecedents { get; }
            public N Value { get; }
            public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<N>> GetNeighbours(int relativeLevelFrom, int relativeLevelTo) { }
            public override string ToString() { }
        }
    }
}
namespace Orc.DependencyGraph.GraphD
{
    public class GraphFast<T> : Orc.DependencyGraph.GraphD.IInternalGraph<T>, Orc.DependencyGraph.IGraph<T>
        where T : System.IEquatable<T>
    {
        public GraphFast() { }
        public GraphFast(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> initialSequences) { }
        public GraphFast(System.Collections.Generic.IEnumerable<T> initialSequence) { }
        public GraphFast(int capacity) { }
        public int CountLevels { get; }
        public int CountNodes { get; }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> Nodes { get; }
        public int ReferencePoint { get; }
        public void AddSequence(System.Collections.Generic.IEnumerable<T> sequence) { }
        public void AddSequences(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> sequences) { }
        public bool CanSort() { }
        public bool CanSort(System.Collections.Generic.IEnumerable<T> sequence) { }
        public Orc.DependencyGraph.INode<T> Find(T node) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetLeafNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodes(int level) { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodesBetween(int levelFrom, int levelTo) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetRootNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Sort() { }
    }
    public class Graph<T> : Orc.DependencyGraph.GraphD.IInternalGraph<T>, Orc.DependencyGraph.IGraph<T>
        where T : System.IEquatable<T>
    {
        public Graph() { }
        public Graph(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> initialSequences) { }
        public Graph(System.Collections.Generic.IEnumerable<T> initialSequence) { }
        public Graph(int capacity) { }
        public int CountLevels { get; }
        public int CountNodes { get; }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> Nodes { get; }
        public void AddSequence(System.Collections.Generic.IEnumerable<T> sequence) { }
        public void AddSequences(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> sequences) { }
        public bool CanSort() { }
        public bool CanSort(System.Collections.Generic.IEnumerable<T> sequence) { }
        public Orc.DependencyGraph.INode<T> Find(T node) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetLeafNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodes(int level) { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodesBetween(int levelFrom, int levelTo) { }
        public System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetRootNodes() { }
        public System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Sort() { }
    }
    public interface IInternalGraph<T> : Orc.DependencyGraph.IGraph<T>
        where T : System.IEquatable<T>
    {
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.GraphD.IInternalNode<T>[]> BackEdges { get; }
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.GraphD.IInternalNode<T>[]> Edges { get; }
        Orc.DependencyGraph.GraphD.IInternalNode<T> GetOrCreateNode(T publicNode);
        void ToFile(string filePath);
    }
    public interface IInternalNode<T> : Orc.DependencyGraph.INode<T>
        where T : System.IEquatable<T>
    {
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.GraphD.IInternalNode<T>> Edges { get; }
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.GraphD.IInternalNode<T>> Parents { get; }
        string PrintImmediatePrecedents { get; }
    }
}
namespace Orc.DependencyGraph
{
    public interface IGraph<T>
        where T : System.IEquatable<T>
    {
        int CountLevels { get; }
        int CountNodes { get; }
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> Nodes { get; }
        void AddSequence(System.Collections.Generic.IEnumerable<T> sequence);
        void AddSequences(System.Collections.Generic.IEnumerable<System.Collections.Generic.IEnumerable<T>> sequences);
        bool CanSort();
        bool CanSort(System.Collections.Generic.IEnumerable<T> sequence);
        Orc.DependencyGraph.INode<T> Find(T value);
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetLeafNodes();
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodes(int level);
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNodesBetween(int levelFrom, int levelTo);
        System.Collections.Generic.IEnumerable<Orc.DependencyGraph.INode<T>> GetRootNodes();
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Sort();
    }
    public interface INode<T>
        where T : System.IEquatable<T>
    {
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Descendants { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> ImmediateDescendants { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> ImmediatePrecedents { get; }
        int Level { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> Precedents { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> TerminatingDescendants { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> TerminatingPrecedents { get; }
        T Value { get; }
        System.Linq.IOrderedEnumerable<Orc.DependencyGraph.INode<T>> GetNeighbours(int relativeLevelFrom, int relativeLevelTo);
    }
    public class TopologicalSortException : System.Exception
    {
        public TopologicalSortException(string exceptionMessage) { }
    }
}