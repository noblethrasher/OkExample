using System.Collections.Generic;

namespace ArcReaction
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Text;
    using System.Text.RegularExpressions;
    using System.Web;
    using Lines = IEnumerable<JSON.Line>;


    public sealed class ArrayLikeJSONObject : JSON
    {
        public ArrayLikeJSONObject(JSON[] xs)
        {
            var n = 0;

            foreach (var x in xs)
                Add(n++.ToString(), x);

            Add("length", xs.Length);
        }
    }

    public abstract class JSON : Representation
    {
        public delegate void Output(string s);
        readonly AttributeCollection attributes = new AttributeCollection();
        readonly Dictionary<string, AttributeName> AttributeMemo = new Dictionary<string, AttributeName>();

        protected void Add(string name, int n) => attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<int>(n)));
        protected void Add(string name, double d) => attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<double>(d)));
        protected void Add(string name, bool b) => attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<bool>(b)));
        protected void Add(string name, DateTime d) => attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<DateValue>(new DateValue(d))));
        protected void Add(string name, string s) => attributes.Add(new Attribute(new AttributeName(name, this), new StringValue(s)));
        protected void Add(string name, JSON json) => attributes.Add(new Attribute(new AttributeName(name, this), new JSONObjValue(json)));
        protected void Add(string name, JSONValue value) => attributes.Add(new Attribute(new AttributeName(name, this), value));


        protected void AddMany(string name, IEnumerable<JSONValue> xs) => attributes.Add(new Attribute(new AttributeName(name, this), new JSONArrayValue(xs)));

        protected void AddMany(string name, params JSONValue[] values) => AddMany(name, values as IEnumerable<JSONValue>);

        //protected void AddMany(AttributeName name, JSONValue value, params JSONValue[] values) => attributes.Add(new Attribute(name, new JSONArrayValue(value, values)));


        protected void MaybeAdd(string name, int? n, bool show_null = true) => MaybeAdd(name, new NullableValue<int>(n), show_null);
        protected void MaybeAdd(string name, DateTime? n, bool show_null = true)
        {
            if(n.HasValue || show_null)
            {
                attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<NullableValue<DateValue>>(new NullableValue<DateValue>(new DateValue(n)))));
            }
        }

        void MaybeAdd<T>(string name, NullableValue<T> value, bool show_null = true)
            where T : struct
        {
            if (value || show_null)
                attributes.Add(new Attribute(new AttributeName(name, this), new SimpleValue<NullableValue<T>>(value)));
        }

        public override void ProcessRequest(HttpContext ctx)
        {
            ctx.Response.ContentType = "application/json";

            ProcessRequest(ctx.Response.Write);
        }

        void ProcessRequest(Output output)
        {
            foreach (var line in Lines)
                foreach (var s in line)
                    output(s);
        }

        public int LineCount
        {
            get
            {
                var total = 2;

                foreach (var attr in attributes)
                    total += attr.LineCount;

                return total;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();

            ProcessRequest(s => sb.Append(s));

            return sb.ToString();
        }

        protected Lines Lines
        {
            get
            {
                yield return "{";

                foreach (var attr in attributes)
                    foreach (var line in attr)
                        yield return line + 1;

                yield return "}";
            }
        }

        protected struct AttributeName
        {
            readonly string name;

            static readonly string[] reserved_words =
                new[]
                {
                    "await",
                    "break",
                    "case",
                    "catch",
                    "class",
                    "const",
                    "continue",
                    "debugger",
                    "default",
                    "delete",
                    "do",
                    "else",
                    "enum",
                    "export",
                    "extends",
                    "false",
                    "finally",
                    "for",
                    "function",
                    "if",
                    "implements",
                    "import",
                    "in",
                    "instanceof",
                    "interface",
                    "let",
                    "new",
                    "null",
                    "package",
                    "private",
                    "protected",
                    "public",
                    "return",
                    "static",
                    "super",
                    "switch",
                    "this",
                    "throw",
                    "true",
                    "try",
                    "typeof",
                    "var",
                    "void",
                    "while",
                    "with",
                    "yield"
                };

            public AttributeName(string name, JSON json)
            {
                this.name = new StringValue(Safe(name)).ToString();

                try
                {
                    json.AttributeMemo.Add(this.name, this);
                }

#pragma warning disable

                catch (ArgumentException ex)
                {
                    throw new ArgumentException($"A value with the identifier, '{name}', is already present in the JSON object.");
                }

#pragma warning restore
            }

            static string Safe(string s)
            {
                if (string.IsNullOrEmpty(s))
                    return "$";

                foreach (var reserved in reserved_words)
                    if (reserved.Equals(s, StringComparison.Ordinal))
                        return "$" + s;
                
                //HACK -- just realized that JSON labels can be numbers (1/7/2015)
                {
                    int n;

                    if (int.TryParse(s, out n))
                        return n.ToString();
                }

                var first = s[0];

                if (first != '_' && first != '$' && !(first >= 'a' && first <= 'z') && !(first >= 'A' && first <= 'Z'))
                    s = "$" + s;

                for (var i = 1; i < s.Length; i++)
                {
                    var c = s[i];

                    if (!char.IsLetterOrDigit(c) && c != '_' && c != '$')
                        throw new ArgumentException("JSON Attribute name must start with $, _, or an ASCII letter and must contain only $, _, digits, or ASCII letter.");
                }

                return s;
            }



            public static implicit operator string (AttributeName attr_name) => attr_name.name;
        }

        protected struct Attribute
        {
            readonly AttributeName name;
            readonly JSONValue value;

            public Attribute(AttributeName name, JSONValue value)
            {
                this.name = name;
                this.value = value;
            }

            public int LineCount => value.TotalLineCount;

            public JSONValue.NameValueEnumerator GetEnumerator() => value.GetNameValueLines(name);
        }

        protected internal struct Line
        {
            readonly List<string> xs;
            readonly int indent;

            public Line(string x)
            {
                xs = new List<string>() { x };
                indent = 0;
            }

            public Line(string x, string y) : this(x) { xs.Add(y); }
            public Line(string x, string y, string z) : this(x, y) { xs.Add(z); }
            public Line(Line ln, string s)
            {
                this = ln;

                if (!string.IsNullOrEmpty(s))
                    xs.Add(s);
            }

            public Line(string s, Line ln)
            {
                this = ln;

                if (!string.IsNullOrEmpty(s))
                    xs.Insert(0, s);
            }

            public Line(Line ln, int n)
            {
                this = ln;
                indent += n;
            }

            public override string ToString() => string.Join("", xs);

            public static Line operator +(Line ln, string s) => new Line(ln, s);
            public static Line operator +(string s, Line ln) => new Line(s, ln);
            public static Line operator +(Line ln, int n) => new Line(ln, n);

            public static implicit operator string (Line line) => null;
            public static implicit operator Line(string s) => new Line(s);

            public Enumerator GetEnumerator() => new Enumerator(this);

            public struct Enumerator : IEnumerator<string>
            {
                readonly Line line;
                int index;

                public Enumerator(Line line)
                {
                    this.line = line;
                    index = -1 * line.indent - 1;
                }

                public string Current => index < 0 ? "\t" : index < line.xs.Count ? line.xs[index] : "";

                object IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext() => ++index <= line.xs.Count;

                public void Reset() { }
            }
        }

        protected struct DateValue
        {
            readonly string s;

            public DateValue(DateTime dt)
            {
                this.s = dt.ToUniversalTime().ToString();
            }

            public DateValue(DateTime? dt)
            {
                this.s = dt != null ? dt.Value.ToUniversalTime().ToString() : "null";
            }

            public override string ToString() => $@"""{s}""";
        }

        protected abstract class JSONValue
        {
            protected internal abstract NameValueEnumerator GetNameValueLines(AttributeName name);

            protected internal abstract IEnumerable<Line> GetValueLines();

            public abstract int TotalLineCount { get; }
            public abstract int PartialLineCount { get; }

            public struct NameValueEnumerator : IEnumerator<Line>
            {
                readonly IEnumerator<Line> xs;
                public int Max { get; }

                public Line Current => xs.Current;

                object IEnumerator.Current => Current;

                public NameValueEnumerator(IEnumerator<Line> xs, int max)
                {
                    Max = max;
                    this.xs = xs;
                }

                public void Dispose() { }

                public bool MoveNext() => xs.MoveNext();

                public void Reset() { }
            }

            public static implicit operator JSONValue(int n) => new SimpleValue<int>(n);
            public static implicit operator JSONValue(string s) => new StringValue(s);
            public static implicit operator JSONValue(DateTime d) => new SimpleValue<DateValue>(new DateValue(d));
            public static implicit operator JSONValue(JSON j) => new JSONObjValue(j);
        }

        protected sealed class JSONArrayValue : JSONValue
        {
            JSONValueCollection values;

            public JSONArrayValue(JSONValue value, params JSONValue[] values)
            {
                this.values = new JSONValueCollection(value, values);
            }

            public JSONArrayValue(IEnumerable<JSONValue> values)
            {
                this.values = new JSONValueCollection(values);
            }

            public JSONArrayValue(IEnumerable<JSON> jsons) : this(from json in jsons select new JSONObjValue(json)) { }


            public override int TotalLineCount => (values.Count > 0 ? 3 : 1) + PartialLineCount;

            public override int PartialLineCount
            {
                get
                {
                    var total = 0;

                    foreach (var value in values)
                        total += value.PartialLineCount;

                    return total;
                }
            }

            protected internal override NameValueEnumerator GetNameValueLines(AttributeName name)
            {
                if (values.Count == 0)
                {
                    return new NameValueEnumerator((new[] { new Line(name, " : ", "[]") } as IEnumerable<Line>).GetEnumerator(), 1);
                }
                else
                {
                    var xs = new[] { new Line(name, ":"), new Line("[") + 1 }.Union(from line in GetValueLines() select line + 2).Union(new[] { new Line("]") + 1 }).GetEnumerator();

                    var max = values.Sum(v => v.PartialLineCount) + 3;

                    return new NameValueEnumerator(xs, max);
                }
            }

            protected internal override Lines GetValueLines()
            {
                foreach (var value in values)
                    foreach (var line in value)
                        yield return line;
            }
        }

        protected struct NullableValue<T>
            where T : struct
        {
            readonly T? value;

            public NullableValue(T? value)
            {
                this.value = value;
            }

            public static implicit operator bool (NullableValue<T> value) => value.value != null;

            public override string ToString() => value != null ? value.ToString() : "null";
        }

        protected sealed class StringValue : JSONValue
        {
            readonly string s;

            public StringValue(string s)
            {
                //TODO: Optimize with StringBuilder

                var sb = new StringBuilder(s.Length);

                for (var i = 0; i < s.Length; i++)
                {
                    var c = s[i];


                    switch (c)
                    {
                        case '\n': { sb.Append("\\\\n"); break; }

                        case '\r': { sb.Append("\\\\r"); break; }

                        case '\t': { sb.Append("\\\\t"); break; }

                        case '\"': { sb.Append("\\\\\""); break; }

                        case '\'': { sb.Append("\\'"); break; }

                        case '\\': { sb.Append("\\\\\\\\"); break; }

                        default: { sb.Append(c); break; }
                    }
                }

                this.s = sb.ToString();
            }

            public StringValue(DateTime d) : this(d.ToUniversalTime().ToString()) { }

            public override int PartialLineCount => 1;

            public override int TotalLineCount => 1;

            public override string ToString() => $"\"{s.Replace("\"", "\\\"")}\"";

            protected internal override NameValueEnumerator GetNameValueLines(AttributeName name) => new NameValueEnumerator(GetEnumerator(name), 1);

            public IEnumerator<Line> GetEnumerator(AttributeName name)
            {
                yield return new Line(name, " : ", $@"{ToString()}");
            }

            protected internal override Lines GetValueLines()
            {
                yield return new Line(this.ToString());
            }

            public static implicit operator StringValue(string s) => new StringValue(s);
            public static implicit operator StringValue(DateTime dt) => new StringValue(dt);
        }

        protected sealed class SimpleValue<T> : JSONValue
            where T : struct
        {
            readonly T value;

            public SimpleValue(T value)
            {
                this.value = value;
            }

            public override int TotalLineCount => 1;
            public override int PartialLineCount => 1;

            protected internal override NameValueEnumerator GetNameValueLines(AttributeName name) => new NameValueEnumerator(GetEnumerator(name), 1);

            public IEnumerator<Line> GetEnumerator(AttributeName name)
            {
                yield return new Line(name, " : ", $@"{value.ToString().ToLower()}");
            }

            protected internal override Lines GetValueLines()
            {
                yield return new Line(value.ToString());
            }

            public static SimpleValue<K> Create<K>(K value) where K : struct => new SimpleValue<K>(value);
        }

        protected sealed class JSONObjValue : JSONValue
        {
            readonly JSON json;

            public JSONObjValue(JSON json)
            {
                this.json = json;
            }

            public override int TotalLineCount => PartialLineCount + 1;
            public override int PartialLineCount => json.LineCount;

            protected internal override NameValueEnumerator GetNameValueLines(AttributeName name)
            {
                return new NameValueEnumerator(GetEnumerator(name), TotalLineCount);
            }

            protected internal override Lines GetValueLines()
            {
                foreach (var line in json.Lines)
                    yield return line;
            }

            IEnumerator<Line> GetEnumerator(AttributeName name)
            {
                yield return new Line(name, ":");

                foreach (var line in json.Lines)
                    yield return line + 1;
            }

            public static JSONObjValue Create(JSON json) => new JSONObjValue(json);
        }

        protected sealed class AttributeCollection : Collection<Attribute, AttributeCollection.MaybeLast, AttributeCollection.Enumerator>
        {
            public struct MaybeLast : IEnumerable<Line>
            {
                readonly bool is_last;
                readonly Attribute attr;

                public MaybeLast(Attribute attr, bool last)
                {
                    this.attr = attr;
                    this.is_last = last;
                }

                public int LineCount => attr.LineCount;

                public struct Enumerator : IEnumerator<Line>
                {
                    readonly JSONValue.NameValueEnumerator enumerator;
                    readonly bool is_last;
                    int iter_count;

                    public Enumerator(MaybeLast maybe)
                    {
                        enumerator = maybe.attr.GetEnumerator();
                        is_last = maybe.is_last;
                        iter_count = 0;
                    }

                    public Line Current => enumerator.Current + (!is_last && iter_count == enumerator.Max ? "," : null);

                    object IEnumerator.Current => Current;

                    public void Dispose() { }

                    public bool MoveNext()
                    {
                        iter_count++;
                        return enumerator.MoveNext();
                    }

                    public void Reset() { }
                }

                public Enumerator GetEnumerator() => new Enumerator(this);

                IEnumerator<Line> Lines.GetEnumerator() => GetEnumerator();

                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            }

            public override Enumerator GetEnumerator() => new Enumerator(this);

            public struct Enumerator : IEnumerator<MaybeLast>
            {
                readonly AttributeCollection xs;
                int index;

                public Enumerator(AttributeCollection xs)
                {
                    this.xs = xs;
                    this.index = -1;
                }

                public MaybeLast Current => new MaybeLast(xs.xs[index], index == xs.xs.Count - 1);

                object IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext() => ++index < xs.xs.Count;

                public void Reset() { }
            }
        }

        protected sealed class JSONValueCollection : Collection<JSONValue, JSONValueCollection.MaybeLast, JSONValueCollection.Enumerator>
        {
            public JSONValueCollection(JSONValue x, params JSONValue[] xs)
            {
                this.xs.Add(x);

                this.xs.AddRange(xs);
            }

            public JSONValueCollection(params JSONValue[] xs) : this(xs as IEnumerable<JSONValue>) { }

            public JSONValueCollection(IEnumerable<JSONValue> xs)
            {
                if (xs != null)
                    this.xs.AddRange(xs);
            }

            public struct MaybeLast : IEnumerable<Line>
            {
                readonly JSONValue value;
                readonly bool is_last;

                public MaybeLast(JSONValue value, bool is_last)
                {
                    this.value = value;
                    this.is_last = is_last;
                }

                public int PartialLineCount => value.PartialLineCount;

                public Enumerator GetEnumerator() => new Enumerator(this);

                IEnumerator<Line> IEnumerable<Line>.GetEnumerator() => GetEnumerator();
                IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

                public struct Enumerator : IEnumerator<Line>
                {
                    readonly IEnumerator<Line> lines;
                    readonly int max;
                    readonly bool is_last;
                    int count;

                    public Enumerator(MaybeLast maybe)
                    {
                        lines = maybe.value.GetValueLines().GetEnumerator();
                        is_last = maybe.is_last;
                        max = maybe.value.PartialLineCount;
                        count = 0;
                    }

                    public Line Current => lines.Current + ((!is_last && max == count) ? "," : null);

                    object IEnumerator.Current => Current;

                    public void Dispose() { }

                    public bool MoveNext()
                    {
                        count++;

                        return lines.MoveNext();
                    }

                    public void Reset() { }
                }
            }

            public struct Enumerator : IEnumerator<MaybeLast>
            {
                readonly JSONValueCollection xs;
                int index;

                public Enumerator(JSONValueCollection xs)
                {
                    this.xs = xs;
                    index = -1;
                }

                public MaybeLast Current => new MaybeLast(xs.xs[index], index == xs.xs.Count - 1);

                object IEnumerator.Current => Current;

                public void Dispose() { }

                public bool MoveNext()
                {
                    ++index;
                    return index < xs.xs.Count;
                }

                public void Reset() { }
            }

            public override Enumerator GetEnumerator() => new Enumerator(this);
        }

        protected abstract class Collection<In, Out, Enumerator> : IEnumerable<Out>
            where Enumerator : IEnumerator<Out>
        {
            protected readonly List<In> xs = new List<In>();

            public Collection<In, Out, Enumerator> Add(In x)
            {
                xs.Add(x);
                return this;
            }

            public int Count => xs.Count;

            public abstract Enumerator GetEnumerator();

            IEnumerator<Out> IEnumerable<Out>.GetEnumerator() => GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        public static implicit operator JSON (JSON[] xs) => new ArrayLikeJSONObject(xs);
    }
}