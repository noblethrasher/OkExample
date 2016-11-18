using System.Collections.Generic;
using System.Linq;

namespace Prelude
{
    //public sealed class Bijection<X, Y>
    //    : IEnumerable<Bijection<X, Y>.Domain>
    //{
    //    internal class DomainMemo : Dictionary<X, Y> { }
    //    internal class CodomainMemo : Dictionary<Y, X> { }

    //    DomainMemo domain = new DomainMemo();
    //    CodomainMemo codomain = new CodomainMemo();

    //    public void Add(X x, Y y)
    //    {
    //        domain.Add(x, y);
    //        codomain.Add(y, x);
    //    }

    //    public struct Domain
    //    {
    //        readonly X x;

    //        public Domain(X x)
    //        {
    //            this.x = x;
    //        }

    //        public static implicit operator Domain(X x) => new Domain(x);
    //    }

    //    public struct Codomain
    //    {
    //        readonly Y y;

    //        public Codomain(Y y)
    //        {
    //            this.y = y;
    //        }

    //        public static implicit operator Codomain(Y y) => new Codomain(y);
    //    }

    //    public struct DomainEnumerator
    //    {
    //        int n;
    //        readonly IList<X> xs;

    //        internal DomainEnumerator(DomainMemo memo)
    //        {
    //            xs = (from kv in memo select kv.Key).ToList();
    //            n = -1;
    //        }

    //        public X Current => xs[n];
    //        public bool MoveNext() => n++ < xs.Count;
    //    }
    //}
}