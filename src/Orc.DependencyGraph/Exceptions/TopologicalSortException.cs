namespace Orc.DependencyGraph
{
    using System;

    public class TopologicalSortException : Exception
    {
        #region Constructors
        public TopologicalSortException(string exceptionMessage)
            : base(exceptionMessage)
        {
        }
        #endregion
    }
}
