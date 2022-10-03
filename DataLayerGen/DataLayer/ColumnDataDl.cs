using DataLayerGen.Classes;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;

namespace DataLayerGen.DataLayer
{
    /// <summary>
    /// ColumnDataDl Class - Data Layer for the ColumnData Table
    /// </summary>
    public class ColumnDataDl : BaseData
    {
        #region Public Methods

        /// <summary>
        /// ListColumnData() - Retrieve a list of the Column Data
        /// </summary>
        /// <param name="cs">Connection String</param>
        /// <param name="schemaName">Schema Name</param>
        /// <param name="tableName">Table to retrieve info for</param>
        /// <returns>List of ColumnData objects for the requested table.</returns>
        public List<ColumnData> ListColumnData(string cs, string schemaName, string tableName)
        {
            List<ColumnData> colDataList = new List<ColumnData>();
            string sqlCommand = "SELECT ";
            sqlCommand += "s.column_id as ColumnId, ";
            sqlCommand += "s.name as ColumnName, ";
            sqlCommand += "sh.name + '.' + o.name as ObjectName, ";
            sqlCommand += "o.type_desc as ObjectType, ";
            sqlCommand += "t.name as DataType, ";
            sqlCommand += "CAST(s.max_length as int) as Length, ";
            sqlCommand += "CASE ";
            sqlCommand += "    WHEN t.name IN('char','varchar') THEN t.name + '(' + CASE WHEN s.max_length < 0 then 'MAX' ELSE CONVERT(varchar(10),s.max_length) END + ')' ";
            sqlCommand += "    WHEN t.name IN('nvarchar','nchar') THEN t.name + '(' + CASE WHEN s.max_length < 0 then 'MAX' ELSE CONVERT(varchar(10),s.max_length / 2) END + ')' ";
            sqlCommand += "    WHEN t.name IN('numeric') THEN t.name + '(' + CONVERT(varchar(10), s.precision) + ',' + CONVERT(varchar(10), s.scale) + ')' ";
            sqlCommand += "    ELSE t.name ";
            sqlCommand += "END as SqlDataType, ";
            sqlCommand += "s.is_nullable IsNullable, ";
            sqlCommand += "( ";
            sqlCommand += "    CASE ";
            sqlCommand += "        WHEN ic.column_id IS NULL THEN '' ";
            sqlCommand += "        ELSE ' identity(' + ISNULL(CONVERT(varchar(10), ic.seed_value), '') + ',' + ISNULL(CONVERT(varchar(10), ic.increment_value), '') + ')=' + ISNULL(CONVERT(varchar(10), ic.last_value), 'null') ";
            sqlCommand += "    END + ";
            sqlCommand += "    CASE ";
            sqlCommand += "        WHEN sc.column_id IS NULL THEN '' ";
            sqlCommand += "        ELSE ' computed(' + ISNULL(sc.definition, '') + ')' ";
            sqlCommand += "    END + ";
            sqlCommand += "    CASE ";
            sqlCommand += "        WHEN cc.object_id IS NULL THEN '' ";
            sqlCommand += "        ELSE ' check(' + ISNULL(cc.definition, '') + ')' ";
            sqlCommand += "    END ";
            sqlCommand += ") as MiscInfo ";
            sqlCommand += "FROM sys.columns s ";
            sqlCommand += "INNER JOIN sys.types t ON s.system_type_id = t.user_type_id and t.is_user_defined = 0 ";
            sqlCommand += "INNER JOIN sys.objects o ON s.object_id = o.object_id ";
            sqlCommand += "INNER JOIN sys.schemas sh on o.schema_id = sh.schema_id ";
            sqlCommand += "LEFT OUTER JOIN sys.identity_columns ic ON s.object_id = ic.object_id AND s.column_id = ic.column_id ";
            sqlCommand += "LEFT OUTER JOIN sys.computed_columns sc ON s.object_id = sc.object_id AND s.column_id = sc.column_id ";
            sqlCommand += "LEFT OUTER JOIN sys.check_constraints cc ON s.object_id = cc.parent_object_id AND s.column_id = cc.parent_column_id ";
            sqlCommand += "WHERE sh.name = @SchemaName ";
            sqlCommand += "AND o.name = @TableName ";
            sqlCommand += "AND o.type_desc = 'USER_TABLE' ";
            sqlCommand += "ORDER BY sh.name + '.' + o.name, s.column_id";

            SqlConnection cn = new SqlConnection(cs);
            using (cn)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 60;
                    cmd.Parameters.Add(new SqlParameter("SchemaName", schemaName));
                    cmd.Parameters.Add(new SqlParameter("TableName", tableName));

                    DataTable table = GetResultSet(cmd);
                    foreach (DataRow dr in table.Rows)
                    {
                        ColumnData colData = new ColumnData();
                        PopulateObject(dr, colData);
                        colDataList.Add(colData);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return colDataList;
        }

        #endregion Public Methods

        #region Private/Protected Methods

        /// <summary>
        /// PopulateObject() - Populates an Object from a DataRow
        /// </summary>
        /// <param name="dr">DataRow (Source) object</param>
        /// <param name="cd">ColumnData (Destination) object</param>
        protected void PopulateObject(DataRow dr, ColumnData cd)
        {
            cd.ColumnId = dr.Field<int>("ColumnId");
            cd.ColumnName = dr.Field<string>("ColumnName");
            cd.ObjectName = dr.Field<string>("ObjectName");
            cd.ObjectType = dr.Field<string>("ObjectType");
            cd.DataType = dr.Field<string>("DataType");
            cd.Length = (int)dr.Field<int>("Length");
            cd.SqlDataType = dr.Field<string>("SqlDataType");
            cd.IsNullable = dr.Field<bool>("IsNullable");
            cd.MiscInfo = dr.Field<string>("MiscInfo");
        }

        #endregion Private/Protected Methods
    }
}
