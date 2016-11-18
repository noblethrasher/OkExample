using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Prelude
{
    public static class StringUtils
    {
        public struct SubstringCheck
        {
            readonly string s;

            internal SubstringCheck(string s)
            {
                this.s = s;
            }

            public static implicit operator bool(SubstringCheck c)
            {
                return c.s != null;
            }
        }

        public static SubstringCheck ContainsAny(this string s, string s0, string s1)
        {
            if (s.Contains(s0))
                return new SubstringCheck(s0);

            if (s.Contains(s1))
                return new SubstringCheck(s1);

            return new SubstringCheck();
        }

        public static SubstringCheck ContainsAny(this string s, string s0, string s1, string s2)
        {
            if (s.Contains(s0))
                return new SubstringCheck(s0);

            if (s.Contains(s1))
                return new SubstringCheck(s1);

            if (s.Contains(s2))
                return new SubstringCheck(s2);

            return new SubstringCheck();
        }

        public static SubstringCheck ContainsAny(this string s, string s0, string s1, string s2, string s3)
        {
            if (s.Contains(s0))
                return new SubstringCheck(s0);

            if (s.Contains(s1))
                return new SubstringCheck(s1);

            if (s.Contains(s2))
                return new SubstringCheck(s2);

            if (s.Contains(s2))
                return new SubstringCheck(s3);

            return new SubstringCheck();
        }

        public static SubstringCheck ContainsAny(this string s, string x, params string[] xs)
        {
            if (s.Contains(x))
                return new SubstringCheck(x);

            for (var i=0; i < xs.Length; i++)
                if(s.Contains(xs[i]))
                    return new SubstringCheck(xs[i]);

            return new SubstringCheck();
        }
        
        public static int IndexOf(this string s, Func<char, bool> predicate)
        {
            if (s != null && s.Length == 0)
                for (var i = 0; i < s.Length; i++)
                    if (predicate(s[i]))
                        return i;

            return -1;
        }

        public static bool IsNullOrEmpty(this string s)
        {
            return string.IsNullOrEmpty(s);
        }

        public static string format(this string s, object obj)
        {
            return string.Format(s, obj);
        }

        public static string format(this string s, object obj1, object obj2)
        {
            return string.Format(s, obj1, obj2);
        }

        public static string format(this string s, object obj1, object obj2, object obj3)
        {
            return string.Format(s, obj1, obj2, obj3);
        }

        public static string format(this string s, params object[] objs)
        {
            return string.Format(s, objs);
        }

        public static Indexes IndexOfAll(this string s, char c)
        {
            if (s == null)
                return new Indexes();

            var xs = new List<int>();

            for (var i = 0; i < s.Length; i++)
                if (s[i] == c)
                    xs.Add(i);

            return new Indexes(xs.ToArray(), s);
        }

        public struct Indexes
        {
            readonly int[] indexes;
            readonly string s;

            internal Indexes(int[] indexes, string s)
            {
                this.indexes = indexes;
                this.s = s;
            }

            public int Length
            {
                get
                {
                    return indexes != null ? indexes.Length : -1;
                }
            }

            public Index this[int n]
            {
                get
                {
                    return new Index(indexes, n, s);
                }
            }

            public Index First
            {
                get
                {
                    if (indexes == null)
                        throw new InvalidOperationException();

                    return new Index(indexes, 0, s);
                }
            }

            public Index Last
            {
                get
                {
                    if (indexes == null)
                        throw new InvalidOperationException();

                    return new Index(indexes, indexes.Length - 1, s);
                }
            }

            public static implicit operator bool(Indexes index)
            {
                return index.indexes != null && index.indexes.Length > 0;
            }

            public struct Index
            {
                readonly int[] indexes;
                readonly string s;
                readonly int n;

                internal Index(int[] indexes, int n, string s)
                {
                    this.indexes = indexes;
                    this.n = n;
                    this.s = s;
                }

                public override string ToString()
                {
                    return indexes[n].ToString();
                }

                public static implicit operator int(Index n)
                {
                    return n.indexes[n.n];
                }

                public static Index operator -(Index n, int i)
                {
                    return new Index(n.indexes, (int)Math.Max(0, n.n - i), n.s);
                }

                public static Index operator +(Index n, int i)
                {
                    return new Index(n.indexes, (int)Math.Max(0, Math.Max(n.n + i, n.indexes.Length)), n.s);
                }

                public string Substring
                {
                    get
                    {
                        return s.Substring(indexes[n]);
                    }
                }
            }
        }
    }
    
    public struct null_or_nonblank_string : IEnumerable<char>
    {
        readonly string s;

        public null_or_nonblank_string(string s)
        {
            this.s = s;
        }

        public bool IsNullOrWhiteSpace
        {
            get
            {
                return string.IsNullOrWhiteSpace(s);
            }
        }

        public static implicit operator bool(null_or_nonblank_string s)
        {
            return !string.IsNullOrWhiteSpace(s.s);
        }

        public static implicit operator string(null_or_nonblank_string s)
        {
            return s.s;
        }

        public static implicit operator null_or_nonblank_string(string s)
        {
            return new null_or_nonblank_string(s);
        }

        public override string ToString()
        {
            return string.IsNullOrWhiteSpace(s) ? null : s;
        }

        public CharEnumerator GetEnumerator()
        {
            return s.GetEnumerator();
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
    
    public struct non_empty_string : IEnumerable<char>
    {
        readonly string s;

        public non_empty_string(string s, [CallerLineNumber] int line_number = 0, [CallerFilePath] string path = null, [CallerMemberNameAttribute] string method = null)
        {
            if (string.IsNullOrEmpty(s))
                throw new ArgumentException($"Unexpected empty string at {line_number} in file, '{path}' by method, '{method}'");            
            
            this.s = s;
        }

        public static implicit operator bool(non_empty_string s)
        {
            return !string.IsNullOrEmpty(s.s);
        }

        public static implicit operator string(non_empty_string s)
        {
            return s.s;
        }

        public static implicit operator non_empty_string(string s)
        {
            return new non_empty_string(s);
        }

        public override string ToString()
        {
            return s;
        }

        public CharEnumerator GetEnumerator()
        {
            return s.GetEnumerator();
        }

        IEnumerator<char> IEnumerable<char>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
    
}