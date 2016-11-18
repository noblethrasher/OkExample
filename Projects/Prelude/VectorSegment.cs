using System;
using System.Collections.Generic;
using System.IO;

namespace Prelude
{
    public static class VectorSegmentUtil
    {
        public static int Read(this VectorSegment<byte> vs, Stream s)
        {
            return s.Read(vs);
        }

        public static VectorSegment<T> ToVectorSegment<T>(this T[] xs, int offset, int length)
        {
            return new VectorSegment<T>(xs, offset, length);
        }

        public static int Read(this Stream s, VectorSegment<byte> vs)
        {
            return s.Read(vs.array, vs.offset, vs.length);
        }

        public static int ToInt32(this VectorSegment<byte> vs)
        {
            if (vs.Length != 4)
                throw new InvalidOperationException("Vector segment length must be exactly 4");

            int n = 0, i = 32;

            foreach (var b in vs)
                n += b << (i -= 8);

            return n;
        }

        public static short ToInt16(this VectorSegment<byte> vs)
        {
            if (vs.Length != 2)
                throw new InvalidOperationException("Vector segment length must be exactly 2");

            short n = 0, i = 16;

            foreach (var b in vs)
                n += (short)(b << (i -= 8));

            return n;
        }
    }

    public struct VectorSegment<T> : IEnumerable<T>
    {
        internal T[] array;
        internal readonly int offset, length;

        public VectorSegment(T[] array, int offset, int length)
        {
            this.offset = offset;
            this.length = length;
            this.array = array;
        }

        public int Length
        {
            get
            {
                return length;
            }
        }

        public T this[int n]
        {
            get
            {
                return array[offset + n];
            }
            set
            {
                array[offset + n] = value;
            }
        }

        /// <summary>
        /// Copies the elements in this vector segment into a new array.
        /// </summary>
        /// <param name="vs"></param>
        /// <returns></returns>
        public static explicit operator T[] (VectorSegment<T> vs)
        {
            return vs.Copy();
        }

        public static implicit operator ArraySegment<T>(VectorSegment<T> vs)
        {
            return new ArraySegment<T>(vs.array, vs.offset, vs.length);
        }

        public T[] Copy()
        {
            return CopyTo(new T[length]);
        }

        public T[] CopyTo(T[] buffer)
        {
            Array.Copy(array, offset, buffer, 0, length);
            return buffer;
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly VectorSegment<T> vs;
            int current;

            public Enumerator(VectorSegment<T> vs)
            {
                this.vs = vs;
                this.current = -1;
            }

            public T Current
            {
                get { return vs.array[vs.offset + current]; }
            }

            public void Dispose()
            {
                return;
            }

            object System.Collections.IEnumerator.Current
            {
                get { return Current; }
            }

            public bool MoveNext()
            {
                return ++current < vs.length;
            }

            public void Reset()
            {
                current = -1;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return new Enumerator(this);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

}