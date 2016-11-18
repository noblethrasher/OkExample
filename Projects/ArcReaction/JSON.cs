using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Web;

namespace ArcReaction
{
    public abstract class JSON : Representation
    {
        [ThreadStatic]
        static int level = 0;

        List<JSONProperty> properties = new List<JSONProperty>();

        protected void Add(string name, int? n, bool show_null)
        {
            if (n == null)
            {
                if (show_null)
                    properties.Add(new SimpleJSONProperty(name, "null"));
            }
            else
                Add(name, n.Value);
        }

        protected void Add(string name, DateTime? n, bool show_null)
        {
            if (n == null)
            {
                if (show_null)
                    properties.Add(new SimpleJSONProperty(name, "null"));
            }
            else
                Add(name, n.Value);
        }

        protected void Add(string name, bool? n, bool show_null)
        {
            if (n == null)
            {
                if (show_null)
                    properties.Add(new SimpleJSONProperty(name, "null"));
            }
            else
                Add(name, n.Value);
        }

        protected void Add(string name, bool value)
        {
            properties.Add(new SimpleJSONProperty(name, value ? "true" : "false"));
        }

        protected void Add(string name, object obj)
        {
            if (obj == null)
            {
                properties.Add(new SimpleJSONProperty(name, "null"));
                return;
            }

            JSON json = obj as JSON;

            if (json != null)
            {
                properties.Add(new SimpleJSONProperty(name, json));
                return;
            }

            var type = obj.GetType();

            var implicit_json = type.GetMethod("op_Implicit", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public, null, new[] { type }, null);

            if (implicit_json != null)
            {
                properties.Add(new SimpleJSONProperty(name, (JSON)implicit_json.Invoke(null, new[] { obj })));
                return;
            }
            else
                properties.Add(new SimpleJSONProperty(name, Sanitize(obj.ToString())));
        }

        protected void Add(string name, DateTime value)
        {
            properties.Add(new SimpleJSONProperty(name, Sanitize(value)));
        }

        protected void Add(string name, string[] values)
        {
            if (values != null)
                properties.Add(new SimpleJSONProperty(name, "[" + string.Join(",", from v in values select Sanitize(v)) + "]"));
            else
                properties.Add(new SimpleJSONProperty(name, "[]"));
        }

        protected static string Sanitize(DateTime value)
        {
            return "{ \"type\": \"dateTime\", \"value\": \"" + value.ToString() + "\")";
        }

        protected static string Sanitize(string value)
        {
            var index = value.IndexOf('"');

            if (index < 0)
                return "\"" + value + "\"";
            else
            {
                index = value.IndexOf('\'');

                if (index < 0)
                    return "'" + value + "'";
                else
                    return "\"" + value.Replace("\"", "\\\"") + "\"";
            }
        }

        protected void Add(string name, string value, bool show_null = true)
        {
            if (value == null)
            {
                if (show_null)
                    properties.Add(new SimpleJSONProperty(name, "null"));
            }
            else
                properties.Add(new SimpleJSONProperty(name, Sanitize(value)));
        }

        public abstract class JSONProperty
        {
            protected readonly string name;
            protected readonly object value;

            protected JSONProperty(string name, object value)
            {
                this.name = name;
                this.value = value;
            }

            public override string ToString()
            {
                return new string('\t', level) + ("\"" + name + "\":" + value.ToString());
            }
        }

        public class SimpleJSONProperty : JSONProperty
        {
            public SimpleJSONProperty(string name, object value) : base(name, value)
            {
               
            }
        }

        public override string ToString()
        {
            level++;

            try
            {
                var sb = new StringBuilder();

                sb.AppendLine("{");

                sb.Append(string.Join(",\r\n", properties));

                sb.Append("}");

                return sb.ToString();
            }
            finally
            {
                level--;
            }
        }

        public override void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "application/json";
            context.Response.Write(ToString());
        }

        public static implicit operator JSON(JSON[] xs)
        {
            return new JSONList(xs);
        }

        public static implicit operator JSON(List<JSON> xs)
        {
            return new JSONList(xs);
        }

        public static JSON Create(IEnumerable<JSON> xs)
        {
            return new JSONList(xs);
        }

        sealed class JSONList : JSON
        {
            readonly IEnumerable<JSON> xs;
            
            public JSONList(IEnumerable<JSON> xs)
            {
                this.xs = xs;
            }

            public override string ToString()
            {
                var sb = new StringBuilder();

                sb.Append("[");

                var stk = new Stack<string>();

                foreach (var x in xs)
                {
                    if (stk.Count > 0)
                    {
                        var s = stk.Peek();

                        if (s == ",\r\n")
                            stk.Push("\t" + x.ToString());
                        else
                        {
                            stk.Push(",\r\n");
                            stk.Push("\t" + x.ToString());
                        }
                    }
                    else
                        stk.Push(x.ToString());
                }

                foreach (var s in stk.Reverse())
                    sb.Append(s);


                sb.AppendLine("]");

                return sb.ToString();
            }
        }
    }

    
}