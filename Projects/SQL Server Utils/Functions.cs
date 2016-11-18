using System;
using System.Data;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using Microsoft.SqlServer.Server;
using System.Collections;
using date = System.DateTime;
using System.Collections.Generic;
using System.Text;

public partial class UserDefinedFunctions
{
    static readonly object[] empty = new object[0];

    [SqlFunction(FillRowMethodName = "FillRow", TableDefinition ="name nvarchar(max)")]
    public static IEnumerable SplitString(SqlString s, SqlString delimiter)
    {
        if (s.IsNull)
            return empty;
        else
        {
            string[] _delimiter = new string[1];

            if (delimiter.IsNull)
                _delimiter[0] = ",";
            else
                _delimiter[0] = delimiter.Value;

            return s.Value.Split(_delimiter, StringSplitOptions.RemoveEmptyEntries);
        }
    }

    public static void FillRow(object row, out SqlString name)
    {
        name = new SqlString(row.ToString());
    }

    [SqlFunction(IsDeterministic = true)]
    public static SqlInt32 ShiftLeft(SqlInt32 n, SqlByte x)
    {
        if (n.IsNull || x.IsNull ||  x.Value > 32)
            return n;

        return n.Value << x.Value;
    }

    [SqlFunction(IsDeterministic = true)]
    public static SqlInt32 ShiftRight(SqlInt32 n, SqlByte x)
    {
        if (n.IsNull || x.IsNull || x.Value > 32)
            return n;

        return n.Value >> x.Value;
    }


    [SqlFunction(IsDeterministic = true)]
    public static SqlInt32 MaybeInt(SqlString s)
    {
        int n;

        if (s.IsNull || !int.TryParse(s.Value.ToString(), out n))
            return new SqlInt32();
        else
            return new SqlInt32(n);
    }

    [SqlFunction(IsDeterministic = true)]
    public static SqlDateTime MaybeDateTime(SqlString s)
    {
        date n;

        if (s.IsNull || !date.TryParse(s.Value.ToString(), out n))
            return new SqlDateTime();
        else
            return new SqlDateTime(n);
    }

    [SqlFunction(IsDeterministic = true)]
    public static SqlDecimal MaybeDecimal(SqlString s)
    {
        decimal n;

        if (s.IsNull || !decimal.TryParse(s.Value.ToString(), out n))
            return new SqlDecimal();
        else
            return new SqlDecimal(n);
    }
}
