using System;
using System.Collections.Generic;

namespace Prelude
{
    public sealed class AdhocEqualityComparer<T> : IEqualityComparer<T>
    {
        readonly Func<T, T, bool> equals;
        readonly Func<T, int> get_hashcode;

        public AdhocEqualityComparer(Func<T, T, bool> equals) : this(equals, null) { }

        public AdhocEqualityComparer(Func<T, T, bool> equals, Func<T, int> get_hashcode)
        {
            this.get_hashcode = get_hashcode;
            this.equals = equals;
        }

        public bool Equals(T x, T y)
        {
            return equals(x, y);
        }

        public int GetHashCode(T obj)
        {
            if (get_hashcode != null)
                return get_hashcode(obj);

            return obj.GetHashCode();
        }

        public static AdhocEqualityComparer<T> Create(Func<T, T, bool> equals)
        {
            return new AdhocEqualityComparer<T>(equals);
        }
    }
    
    public sealed class AdhocEnumerable<T> : IEnumerable<T>
    {
        readonly Func<IEnumerator<T>> get_enumerator;

        public AdhocEnumerable(Func<IEnumerator<T>> get_enumerator)
        {
            this.get_enumerator = get_enumerator;
        }

        public IEnumerator<T> GetEnumerator()
        {
            return get_enumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    public sealed class AdhocEnumerator<T> : IEnumerator<T>
    {
        readonly Func<T> current;
        readonly Func<bool> move_next;
        
        readonly Action reset, dispose;

        public AdhocEnumerator(Func<bool> move_next, Func<T> current, Action reset, Action dispose)
        {
            this.current = current;
            this.move_next = move_next;
            this.reset = reset;
            this.dispose = dispose;
        }

        public AdhocEnumerator(Func<bool> move_next, Func<T> current) : this(move_next, current, null, null)
        { }

        public T Current
        {
            get { return current(); }
        }

        public void Dispose()
        {
            if (dispose != null)
                dispose();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return Current; }
        }

        public bool MoveNext()
        {
            return move_next();
        }

        public void Reset()
        {
            if (reset != null)
                reset();
        }
    }

    public sealed class AdhocEquatable<T> : IEquatable<T>
    {
        readonly Func<T, bool> equals;

        public AdhocEquatable(Func<T, bool> equals)
        {
            this.equals = equals;
        }

        public bool Equals(T other)
        {
            return equals(other);
        }
    }

}