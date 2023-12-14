namespace Orc.DependencyGraph
{
    using System;

    public class TopologicalSortException : Exception
    {
        public TopologicalSortException(string exceptionMessage)
            : base(exceptionMessage)
        {
        }
    }
}
