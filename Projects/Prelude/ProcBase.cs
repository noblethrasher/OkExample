using System;
using System.Linq;
using System.Data.SqlClient;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Runtime.CompilerServices;
using System.Diagnostics.CodeAnalysis;

namespace Prelude
{
    public struct ColumnName
    {
        readonly string name;

        public ColumnName(string name)
        {
            this.name = name;
        }

        public static implicit operator ColumnName(string s)
        {
            return new ColumnName(s);
        }

        public static implicit operator string(ColumnName name)
        {
            return name.name;
        }
    }

    public sealed class SchemaAttribute : Attribute
    {
        public string Name { get; }

        public SchemaAttribute(string name)
        {
            Name = name;
        }
    }

    public abstract partial class Proc<T>
    {
        readonly Func<string> sql;
        readonly List<SqlParameter> parameters;

        protected Proc(Func<string> sql, IEnumerable<SqlParameter> parameters)
        {
            this.sql = sql;
            this.parameters = parameters != null ? new List<SqlParameter>(parameters) : new List<SqlParameter>();
        }

        protected Proc(string sql) : this(() => sql, null)
        {

        }

        protected Proc(string sql, params SqlParameter[] parameters)
            : this(() => sql, parameters)
        {

        }

        protected Proc(params SqlParameter[] parameters)
        {
            sql = () => this.GetType().Name;
            this.parameters = new List<SqlParameter>(parameters);
        }

        protected virtual string GetConnectionString()
        {
            return ConfigurationManager.ConnectionStrings["default"].ConnectionString;
        }

        protected void Add<K>(string name, Nullable<K> value)
            where K : struct
        {
            if (value != null)
                parameters.Add(new SqlParameter((EnsuredPrependedAt)name, value.Value));
        }

        protected void Add<K>(string name, K value)
            where K : struct
        {
            parameters.Add(new SqlParameter((EnsuredPrependedAt)name, value));
        }        

        protected void Add(string name, int n)
        {
            parameters.Add(new SqlParameter((EnsuredPrependedAt)name, n));
        }

        protected void Add(string name, string s)
        {
            parameters.Add(new SqlParameter((EnsuredPrependedAt)name, s));
        }

        protected void Add(string name, non_empty_string s) => Add(name, (string)s);

        protected void Add(string name, null_or_nonblank_string s)
        {
            if (s)
                Add(name, s.ToString());
        }

        protected void Add(SqlParameter p)
        {
            parameters.Add(p);
        }

        protected abstract T _Execute(SqlCommandEx cmd);

        [SuppressMessage("Microsoft.Security", "CA2100:Review SQL queries for security vulnerabilities")]
        public virtual T Execute(SqlConnectionEx sc)
        {
            SqlConnection conn = sc;

            try
            {
                var cmd = new SqlCommand(sql(), conn);
                
                cmd.CommandType = !cmd.CommandText.StartsWith("[") && cmd.CommandText.Any(c => char.IsWhiteSpace(c)) ? CommandType.Text : CommandType.StoredProcedure;

                if((cmd.CommandType & CommandType.StoredProcedure) != 0)
                {
                    var schema = (from attr in this.GetType().GetCustomAttributes(false) where attr.GetType() == typeof(SchemaAttribute) select attr as SchemaAttribute).FirstOrDefault();

                    if (schema != null)
                        cmd.CommandText = schema.Name + "." + cmd.CommandText;
                }


                if (parameters != null)
                    foreach (var p in parameters)
                        cmd.Parameters.Add(p);

                return _Execute(cmd);
            }
            finally
            {
                if (sc.Dispose)
                    conn.Dispose();
            }
        }

        public virtual T Execute()
        {
            return Execute(new SqlConnection(GetConnectionString()));
        }
    }

    public abstract partial class Proc<T>
    {
        public struct SqlConnectionEx
        {
            readonly SqlConnection sc;
            bool dispose;

            public bool Dispose
            {
                get
                {
                    return dispose;
                }
            }

            internal SqlConnectionEx(SqlConnection sc, bool dispose)
            {
                this.sc = sc;
                this.dispose = dispose;
            }

            public static implicit operator SqlConnectionEx(SqlConnection sc)
            {
                return new SqlConnectionEx(sc, true);
            }

            public static implicit operator SqlConnection(SqlConnectionEx sx)
            {
                var conn = sx.sc;

                if (conn.State == System.Data.ConnectionState.Closed)
                    conn.Open();

                return conn;
            }
        }

        public struct SqlCommandEx
        {
            readonly SqlCommand sc;

            public SqlCommandEx(SqlCommand sc)
            {
                this.sc = sc;
            }

            public SqlDataReader ExecuteReader()
            {
                return sc.ExecuteReader();
            }

            public int ExecuteNonQuery()
            {
                return sc.ExecuteNonQuery();
            }

            public IAsyncResult BeginExecuteReader() => sc.BeginExecuteReader();

            public static implicit operator SqlCommandEx(SqlCommand sc)
            {
                return new SqlCommandEx(sc);
            }
        }        
    }

    public abstract partial class Proc<T>
    {
        struct EnsuredPrependedAt
        {
            readonly string s;
            
            public EnsuredPrependedAt(string s)
            {
                if(!string.IsNullOrEmpty(s))
                    if (s[0] != '@')
                        s = '@' + s;

                this.s = s;
            }

            public static implicit operator string (EnsuredPrependedAt s)
            {
                return s.s;
            }

            public static implicit operator EnsuredPrependedAt(string s)
            {
                return new EnsuredPrependedAt(s);
            }

            public override string ToString()
            {
                return s;
            }
        }
    }
}