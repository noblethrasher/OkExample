using System;
using System.Collections.Generic;
using System.Linq;

namespace Prelude
{
    struct Memo<TKey, TValue> : IEnumerable<KeyValuePair<TKey, TValue>>
    {
        readonly Dictionary<TKey, TValue> memo;

        public void Add(TKey key, TValue value)
        {
            memo.Add(key, value);
        }

        public TValue this[TKey key]
        {
            get
            {
                return memo[key];
            }
        }

        public Dictionary<TKey, TValue>.Enumerator GetEnumerator()
        {
            return memo.GetEnumerator();
        }

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }

    static class Foo
    {   
        public void Bar()
        {
            var memo = new Memo<Func<int, bool>, List<int>>()
            {
                { n => n % 02 == 0, new List<int>() },
                { n => n % 03 == 0, new List<int>() },
                { n => n % 05 == 0, new List<int>() },
                { n => n % 07 == 0, new List<int>() },
                { n => n % 11 == 0, new List<int>() },
                { n => n % 13 == 0, new List<int>() },
            };

            

            var m = 0;

            foreach (var kv in memo)
            {
                if (kv.Key(m))
                    kv.Value.Add(m);
            }
        }
    }

    public abstract class Token<T>
    {
        public abstract Consideration Consider(T elem);

        public virtual Consideration EndOfStream()
        {
            return this;
        }

        public struct Consideration : IEnumerable<Token<T>>
        {
            readonly IEnumerable<Token<T>> xs;
            readonly Token<T> x;
            
            public Consideration(IEnumerable<Token<T>> xs)
            {
                this.xs = xs;
                this.x = null;
            }

            public Consideration(Token<T> x)
            {
                this.xs = null;
                this.x = x;
            }

            struct ConsiderationEnumerator : IEnumerator<Token<T>>
            {
                readonly IEnumerator<Token<T>> xs;
                readonly Token<T> x;
                bool finished;
                Type type;

                enum Type
                {
                    Singleton,
                    NonSingleton
                }

                public ConsiderationEnumerator(IEnumerable<Token<T>> xs)
                {
                    this.xs = xs.GetEnumerator();
                    this.x = null;
                    type = Type.NonSingleton;
                    finished = false;
                }

                public ConsiderationEnumerator(Token<T> x)
                {
                    this.xs = null;
                    this.x = x;
                    type = Type.Singleton;
                    finished = false;
                }

                public Token<T> Current
                {
                    get
                    {
                        if (type == Type.Singleton)
                            return x;
                        else
                            return xs.Current;
                    }
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
                    if (!finished)
                    {
                        if (type == Type.Singleton)
                            return finished = true;

                        else
                            return xs.MoveNext();
                    }

                    return false;
                }

                public void Reset()
                {
                    return;
                }
            }

            ConsiderationEnumerator GetEnumerator()
            {
                if (xs != null)
                    return new ConsiderationEnumerator(xs);
                else
                    return new ConsiderationEnumerator(x);
            }

            IEnumerator<Token<T>> IEnumerable<Token<T>>.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                throw new NotImplementedException();
            }

            public static implicit operator Consideration(Token<T> token)
            {
                return new Consideration(token);
            }

            public static implicit operator Consideration(Token<T>[] tokens)
            {
                return new Consideration(tokens);
            }

            public static Consideration operator *(Consideration c, Token<T> t)
            {
                if (c.xs != null)
                    return new Consideration(c.xs.Union(new [] { t }));
                else
                    return new Consideration(new[] { c.x, t });
            }

        }
    }

    public struct Stacked<T> : IEnumerable<T>
    {
        readonly Stack<T> stack;

        public Stacked(T x)
        {
            stack = new Stack<T>();
            stack.Push(x);
        }

        public Stacked<T> Push(T x)
        {
            stack.Push(x);
            return this;
        }

        public Stacked<T> Push(IEnumerable<T> xs)
        {
            foreach (var x in xs)
                stack.Push(x);

            return this;
        }

        public T Pop()
        {
            return stack.Pop();
        }

        public T Peek()
        {
            return stack.Peek();
        }


        public int Count
        {
            get
            {
                return stack.Count;
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            return stack.Reverse().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public static implicit operator bool(Stacked<T> stacked)
        {
            return stacked.stack != null && stacked.Count > 0;
        }

        public static implicit operator Stacked<T> (T x)
        {
            return new Stacked<T>(x);
        }
    }

    public static partial class PreludeToLinq
    {
        public static IEnumerable<Token<T>> Scan<T>(this IEnumerable<T> xs, Token<T> start)
        {
            return Scan(xs, new Stacked<Token<T>>(start));
        }

        public static IEnumerable<Token<T>> Scan<T>(this IEnumerable<T> xs, Stacked<Token<T>> stacked)
        {
            foreach (var x in xs)
                stacked.Push(stacked.Pop().Consider(x));

            return stacked ? stacked.Push(stacked.Pop().EndOfStream()) : stacked;
        }

        public static IEnumerable<TaggedPartition<TaggedItem, TagDescriptor>> Partition<TaggedItem, TagDescriptor>(this IEnumerable<TaggedItem> xs, Func<TaggedItem, bool> predicate1, Func<TaggedItem, bool> predicate2)
        {
            return new Partition(xs, predicate1, predicate2, rest);
        }

        public sealed class TaggedPartition<TaggedItem, TaggedDesciptor> : Token<TaggedItem>, IEnumerable<TaggedItem>
        {
            readonly TaggedDesciptor descriptor;
            readonly IEnumerable<TaggedItem> xs;

            public TaggedPartition(TaggedDesciptor descriptor, IEnumerable<TaggedItem> xs)
            {
                this.descriptor = descriptor;
                this.xs = xs;
            }

            public TaggedDesciptor Descriptor
            {
                get
                {
                    return descriptor;
                }
            }

            public IEnumerable<TaggedItem> Items
            {
                get
                {
                    return xs;
                }
            }

            public IEnumerator<TaggedItem> GetEnumerator()
            {
                return xs.GetEnumerator();
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }

        public class Tag<TaggedItem, TagDesriptor> : Token<TaggedItem>
        {

            struct Foo
            {
                public readonly Func<TaggedItem, bool> predicate;
                public readonly TagDesriptor descriptor;

                public Foo(Func<TaggedItem, bool> predicate, TagDesriptor descriptor)
                {
                    this.predicate = predicate;
                    this.descriptor = descriptor;
                }

                public override int GetHashCode()
                {
                    return descriptor.GetHashCode();
                }

                public override bool Equals(object obj)
                {
                    return descriptor.Equals(obj);
                }
            }


            readonly Dictionary<Foo, List<TaggedItem>> memo;
            readonly List<TaggedItem> rest = new List<TaggedItem>();

            public override Token<TaggedItem>.Consideration Consider(TaggedItem elem)
            {
                var found = false;

                foreach(var kv in memo)
                {
                    if(kv.Key.predicate(elem))
                    {
                        found = true;
                        kv.Value.Add(elem);
                        break;
                    }
                }

                if(!found)
                    rest.Add(elem);

                return this;
            }

            public override Consideration EndOfStream()
            {
                return new Consideration((from x in memo select new TaggedPartition<TaggedItem, TagDesriptor>(x.Key.descriptor, x.Value)));
            }
        }
    }

    
}