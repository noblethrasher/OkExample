using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Configuration;
using System.Runtime.CompilerServices;
using System.Data;
using System.Dynamic;
using System.Linq.Expressions;
using System.Data.SqlTypes;

namespace Prelude
{       
    public struct NullCheck<T>
    {
        int ord;
        readonly IDataReader rdr;

        public NullCheck(IDataReader rdr, string name)
        {
            ord = (this.rdr = rdr).GetOrdinal(name);
        }

        public bool HasValue
        {
            get
            {
                return !rdr.IsDBNull(ord);
            }
        }

        public T Value
        {
            get
            {
                return (T)rdr.GetValue(ord);
            }
        }

        public static implicit operator bool(NullCheck<T> null_check)
        {
            return null_check.HasValue;
        }        
    }

    public static class DataReaderUtils
    {
        public static NullCheck<T> CheckNull<T>(this IDataReader rdr, string name)
        {
            return new NullCheck<T>(rdr, name);
        }

        public struct ColumnMetaData
        {
            readonly IDataReader rdr;
            readonly string name;

            public ColumnMetaData(IDataReader rdr, string name)
            {
                this.rdr = rdr;
                this.name = name;
            }

            public bool Exists
            {
                get
                {
                    var len = rdr.FieldCount;

                    for (var i = 0; i < len; i++)
                        if (rdr.GetName(i) == name || rdr.GetName(i).Equals(name, StringComparison.OrdinalIgnoreCase))
                            return true;

                    return false;
                }
            }

            public bool? HasValue
            {
                get
                {
                    try
                    {
                        return !rdr.IsDBNull(rdr.GetOrdinal(name));
                    }
                    #pragma warning disable

                    catch (IndexOutOfRangeException ex)
                    {
                        return null;
                    }


                    #pragma warning restore

                }
            }

            public static implicit operator bool(ColumnMetaData meta)
            {
                return meta.Exists && meta.HasValue.Value;
            }
        }

        public static ColumnMetaData HasColumn(this IDataReader rdr, string name)
        {
            return new ColumnMetaData(rdr, name);
        }

        public static string String(this IDataReader rdr, string name)
        {
            return rdr.GetString(rdr.GetOrdinal(name));
        }


        public static DateTime GetDateTime(this IDataReader rdr, string name)
        {
            return rdr.GetDateTime(rdr.GetOrdinal(name));
        }

        public static DateTime? MaybeDateTime(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetDateTime(ord);

            return null;
        }        
        
        
        public static bool GetBoolean(this IDataReader rdr, string name)
        {
            return rdr.GetBoolean(rdr.GetOrdinal(name));
        }

        public static bool? MaybeBoolean(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetBoolean(ord);

            return null;
        }
        
        
        public static int Int32(this IDataReader rdr, string name)
        {
            try
            {
                return rdr.GetInt32(rdr.GetOrdinal(name));
            }
            catch (InvalidCastException ex)
            {
                throw ex = new InvalidCastException($"Unable to cast the value contained in column '{name}' to System.Int32. The value appears to be of type '{rdr.GetValue(rdr.GetOrdinal(name)).GetType().FullName}'");
            }
            catch(SqlNullValueException ex)
            {
                throw ex = new SqlNullValueException($"The column '{name}' is unexpectedly null.");
            }
        }

        public static T GetEnum<T>(this IDataReader rdr, string name)
            where T:struct
        {
            return (T) rdr.GetValue(rdr.GetOrdinal(name));
        }

        public static byte[] GetBinary(this IDataReader rdr, string name, string length_column_name)
        {
            var ord = rdr.GetOrdinal(name);
            var buffer = new byte[rdr.MaybeInt64(length_column_name).Value];

            rdr.GetBytes(ord, 0, buffer, 0, buffer.Length);

            return buffer;
        }

        public static byte[] GetBinary(this IDataReader rdr, string name) => rdr.GetBinary(name, "length");

        public static int? MaybeInt32(this IDataReader rdr, string name)            
        {
            var ord = rdr.GetOrdinal(name);

            if(!rdr.IsDBNull(ord))
                return rdr.GetInt32(ord);

            return null;
        }


        public static short GetInt16(this IDataReader rdr, string name)
        {
            return rdr.GetInt16(rdr.GetOrdinal(name));
        }

        public static short? MaybeInt16(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetInt16(ord);

            return null;
        }


        public static long GetInt64(this IDataReader rdr, string name)
        {
            return rdr.GetInt64(rdr.GetOrdinal(name));
        }

        public static long? MaybeInt64(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetInt64(ord);

            return null;
        }


        public static float GetFloat(this IDataReader rdr, string name)
        {
            return rdr.GetFloat(rdr.GetOrdinal(name));
        }

        public static float? MaybeFloat(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetFloat(ord);

            return null;
        }


        public static decimal GetDecimal(this IDataReader rdr, string name)
        {
            return rdr.GetDecimal(rdr.GetOrdinal(name));
        }

        public static decimal? MaybeDecimal(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetDecimal(ord);

            return null;
        }


        public static double GetDouble(this IDataReader rdr, string name)
        {
            return rdr.GetDouble(rdr.GetOrdinal(name));
        }

        public static double? MaybeDouble(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetDouble(ord);

            return null;
        }


        public static Guid GetGuid(this IDataReader rdr, string name)
        {
            return rdr.GetGuid(rdr.GetOrdinal(name));
        }

        public static Guid? MaybeGetGuid(this IDataReader rdr, string name)
        {
            var ord = rdr.GetOrdinal(name);

            if (!rdr.IsDBNull(ord))
                return rdr.GetGuid(ord);

            return null;
        }
    }
}
