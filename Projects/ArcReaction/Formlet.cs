using System;
using ArcReaction;
using System.Collections.Generic;


namespace ArcReaction
{
    public static class FormletModule
    {
        delegate bool TryParse<T>(string s, out T t);

        static readonly TryParse<int>       try_parse_int   = int.TryParse;
        static readonly TryParse<long>      try_parse_long  = long.TryParse;
        static readonly TryParse<float>     try_parse_float = float.TryParse;
        static readonly TryParse<double>    try_parse_dbl   = double.TryParse;
        static readonly TryParse<decimal>   try_parse_dec   = decimal.TryParse;
        
        static readonly TryParse<Guid>      try_parse_guid  = Guid.TryParse;
        static readonly TryParse<DateTime>  try_parse_dt    = DateTime.TryParse;        
        
        private static Formlet<T> GetError<T>(string msg)
        {
            return new Formlet<T>.Failed(msg);
        }

        public struct MissingError
        {
            readonly string error;

            public MissingError(string name)
            {
                this.error = name;
            }

            public static implicit operator MissingError(string message)
            {
                return new MissingError(message);
            }

            public static implicit operator string(MissingError err)
            {
                return err.error;
            }
        }

        private static Formlet<Nullable<T>> GetNullable<T>(string s, TryParse<T> try_parse)
            where T : struct
        {
            T n = default(T);

            return try_parse(s, out n) ? (n) : default(T?);
        }

        private static Formlet<T> GetNonNull<T>(HttpContextEx ctx, string name, TryParse<T> try_parse)
            where T : struct
        {
            T value = default(T);

            return try_parse(ctx[name], out value) ? (Formlet<T>) value : (MissingError) name;
        }

        public static Formlet<int?> MaybeGetInt(string name)
        {
            return GetNullable<int>(name, int.TryParse);
        }

        public static Formlet<DateTime?> MaybeGetDateTime(string name)
        {
            return GetNullable<DateTime>(name, DateTime.TryParse);
        }

        public static Formlet<Guid?> MaybeGetGuid(string name)
        {
            return GetNullable<Guid>(name, Guid.TryParse);
        }

        public static Formlet<decimal?> MaybeGetDecimal(string name)
        {
            return GetNullable<decimal>(name, decimal.TryParse);
        }

        public static Formlet<float?> MaybeGetFloat(string name)
        {
            return GetNullable<float>(name, float.TryParse);
        }

        public static Formlet<int> GetInt32(this HttpContextEx ctx, string name)
        {
            return GetNonNull<int>(ctx, name, try_parse_int);
        }

        public static Formlet<DateTime> GetDateTime(this HttpContextEx ctx, string name)
        {
            return GetNonNull<DateTime>(ctx, name, try_parse_dt);
        }

        public static Formlet<double> GetDouble(this HttpContextEx ctx, string name)
        {
            return GetNonNull<double>(ctx, name, try_parse_dbl);
        }

        public static Formlet<decimal> GetDecimal(this HttpContextEx ctx, string name)
        {
            return GetNonNull<decimal>(ctx, name, try_parse_dec);
        }

        public static Formlet<float> GetFloat(this HttpContextEx ctx, string name)
        {
            return GetNonNull<float>(ctx, name, try_parse_float);
        }

        public static Formlet<Guid> GetGuid(this HttpContextEx ctx, string name)
        {
            return GetNonNull<Guid>(ctx, name, try_parse_guid);
        }

        public static Formlet<long> GetInt64(this HttpContextEx ctx, string name)
        {
            return GetNonNull<long>(ctx, name, try_parse_long);
        }

        public static Formlet<bool> GetBoolean(this HttpContextEx ctx, string name)
        {
            if (name.Equals("yes", StringComparison.OrdinalIgnoreCase) || name.Equals("true", StringComparison.OrdinalIgnoreCase))
                return (Formlet<bool>)true;

            if (name.Equals("no", StringComparison.OrdinalIgnoreCase) || name.Equals("false", StringComparison.OrdinalIgnoreCase))
                return (Formlet<bool>)false;

            return new Formlet<bool>.Failed("Cannot find non value named '" + name + "' that is a boolean");
        }

        public static Formlet<bool?> MaybeGetBoolean(this HttpContextEx ctx, string name)
        {
            bool? b = null;

            if (name.Equals("yes", StringComparison.OrdinalIgnoreCase) || name.Equals("true", StringComparison.OrdinalIgnoreCase))
                b = true;

            if (name.Equals("no", StringComparison.OrdinalIgnoreCase) || name.Equals("false", StringComparison.OrdinalIgnoreCase))
                b = false;

            return b;
        }
        
        public static Formlet<string> GetNonEmptyString(this HttpContextEx ctx, string name)
        {
            var s = ctx.Request[name];

            if (!string.IsNullOrEmpty(s))
                return s;

            else return new Formlet<string>.Failed("Cannot find non value named '" + name + "' that is a non-empty string");
        }
    }
    
    public class Formlet<T> : IEnumerable<T>
    {
        readonly T value;

        private Formlet(T value)
        {
            this.value = value;
        }

        private Formlet() { }

        public virtual bool HasValue
        {
            get { return true; }
        }

        public virtual T Value
        {
            get
            {
                return value;
            }
        }

        public virtual Formlet<V> Bind<U, V>(U value, Func<U, T, V> g)
        {
            return g(value, this.value);
        }

        public virtual Formlet<U> Select<U>(Func<T, U> f)
        {
            var k = f(this.value);
            return k;
        }

        public virtual Formlet<U> SelectMany<U>(Func<T, Formlet<U>> f)
        {
            return f(this.value);
        }

        public virtual Formlet<V> SelectMany<U, V>(Func<T, Formlet<U>> f, Func<T, U, V> g)
        {
            return f(this.value).Bind(value, g);
        }

        public static implicit operator Formlet<T>(T obj)
        {
            return new Formlet<T>(obj);
        }

        public static implicit operator Formlet<T>(FormletModule.MissingError err)
        {
            return new Formlet<T>.Failed("Cannot find value named '" + err + "' of type " + typeof(T).FullName);
        }

        public static implicit operator bool(Formlet<T> frmlt)
        {
            return frmlt.HasValue;
        }

        public static implicit operator T(Formlet<T> obj)
        {
            return obj.value;
        }

        public sealed class Failed : Formlet<T>
        {
            readonly string msg;

            public Failed(string msg)
            {
                this.msg = msg;
            }

            public override Formlet<V> Bind<U, V>(U value, Func<U, T, V> g)
            {
                return new Formlet<V>.Failed(msg);
            }

            public override Formlet<U> Select<U>(Func<T, U> f)
            {
                return new Formlet<U>.Failed(msg);
            }

            public override Formlet<V> SelectMany<U, V>(Func<T, Formlet<U>> f, Func<T, U, V> g)
            {
                return new Formlet<V>.Failed(msg);
            }

            public override Formlet<U> SelectMany<U>(Func<T, Formlet<U>> f)
            {
                return new Formlet<U>.Failed(msg);
            }

            public override bool HasValue
            {
                get
                {
                    return false;
                }
            }

            public override T Value
            {
                get
                {
                    throw new InvalidOperationException("Value is not assigned because: " + msg);
                }
            }

            public override Enumerator GetEnumerator()
            {
                return new Enumerator();
            }
        }

        public virtual Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<T> System.Collections.Generic.IEnumerable<T>.GetEnumerator()
        {
            return GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public struct Enumerator : IEnumerator<T>
        {
            readonly Formlet<T> value;
            bool done;

            internal Enumerator(Formlet<T> frmlt)
            {
                this.value = frmlt;
                done = (frmlt == null);
            }

            public T Current
            {
                get { return value.Value; }
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
                return value != null && (done = !done);
            }

            public void Reset()
            {
                return;
            }
        }
    }
}