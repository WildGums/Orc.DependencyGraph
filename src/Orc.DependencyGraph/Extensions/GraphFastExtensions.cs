namespace Orc.DependencyGraph
{
    using System;
    using System.Collections.Generic;
    using Orc.DependencyGraph.GraphD;

    internal static class GraphFastExtensions
    {
        public static void PushUnvisited<T>(this Stack<InternalNodeFast<T>> stack, List<InternalNodeFast<T>> list, Func<InternalNodeFast<T>, bool> isVisited)
            where T : IEquatable<T>
        {
            foreach (var child in list)
            {
                if (isVisited(child))
                {
                    continue;
                }

                stack.Push(child);
            }
        }
    }
}
