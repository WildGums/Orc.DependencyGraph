// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IInternalGraph.cs" company="WildGums">
//   Copyright (c) 2008 - 2017 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.DependencyGraph.GraphD
{
    using System;
    using System.Collections.Generic;

    public interface IInternalGraph<T> : IGraph<T> 
        where T : IEquatable<T>
    {
        #region Properties
        IEnumerable<IInternalNode<T>[]> Edges { get; }
        IEnumerable<IInternalNode<T>[]> BackEdges { get; }
        #endregion

        #region Methods
        IInternalNode<T> GetOrCreateNode(T publicNode);
        void ToFile(string filePath);
        #endregion
    }
}