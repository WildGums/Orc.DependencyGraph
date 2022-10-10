namespace Orc.DependencyGraph
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Linq;

    internal class OrderedEnumerable<T> : IOrderedEnumerable<T>
    {
        private readonly Func<IEnumerable<T>> _enumerator;

        public OrderedEnumerable(Func<IEnumerable<T>> enumerator)
        {
            ArgumentNullException.ThrowIfNull(enumerator);

            _enumerator = enumerator;
        }

        public IOrderedEnumerable<T> CreateOrderedEnumerable<TKey>(Func<T, TKey> keySelector, IComparer<TKey>? comparer, bool @descending)
        {
            ArgumentNullException.ThrowIfNull(keySelector);

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
    }
}
