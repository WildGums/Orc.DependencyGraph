// --------------------------------------------------------------------------------------------------------------------
// <copyright file="TopologicalSortException.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


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