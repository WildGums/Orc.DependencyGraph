// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderedEnumerable.cs" company="WildGums">
//   Copyright (c) 2008 - 2018 WildGums. All rights reserved.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------


namespace Orc.DependencyGraph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class OrderedEnumerable<T> : IOrderedEnumerable<T>
    {
        #region Fields
        private readonly Func<IEnumerable<T>> _enumerator;
        #endregion

        #region Constructors
        public OrderedEnumerable(Func<IEnumerable<T>> enumerator)
        {
            _enumerator = enumerator;
        }
        #endregion

        #region IOrderedEnumerable<T> Members
        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey> comparer, bool @descending)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<T> GetEnumerator()
        {
            var enumeratorInstance = _enumerator();
            return enumeratorInstance.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion
    }
}