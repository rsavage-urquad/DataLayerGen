using DataLayerGen.Classes;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DataLayerGen.DataLayer
{
    /// <summary>
    /// TableDataDl Class - Data Layer for the Table Data
    /// </summary>
    public class TableDataDl : BaseData
    {
        #region Public Methods

        /// <summary>
        /// ListTableData() - Retrieve a list of the User Defined Tables
        /// </summary>
        /// <param name="cs">Connection String</param>
        /// <returns>List of TableData objects.</returns>
        public List<TableData> ListTableData(string cs)
        {
            List<TableData> tableDataList = new List<TableData>();
            string sqlCommand = "SELECT ";
            sqlCommand += "sh.name + '.' + o.name as ObjectName ";
            sqlCommand += "FROM sys.objects o ";
            sqlCommand += "INNER JOIN sys.schemas sh on o.schema_id = sh.schema_id ";
            sqlCommand += "WHERE o.type_desc = 'USER_TABLE' ";
            sqlCommand += "ORDER BY sh.name + '.' + o.name";

            SqlConnection cn = new SqlConnection(cs);
            using (cn)
            {
                try
                {
                    SqlCommand cmd = new SqlCommand(sqlCommand, cn);
                    cmd.CommandType = CommandType.Text;
                    cmd.CommandTimeout = 60;

                    DataTable table = GetResultSet(cmd);
                    foreach (DataRow dr in table.Rows)
                    {
                        TableData tableData = new TableData();
                        PopulateObject(dr, tableData);
                        tableDataList.Add(tableData);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
            }

            return tableDataList;
        }

        /// <summary>
        /// VerifyConnectionString() - Verifies that the Connection String is valid.
        /// </summary>
        /// <param name="cs">Connection String</param>
        /// <returns>True if Valid, otherwise false.</returns>
        public bool VerifyConnectionString(string cs)
        {
            bool retCode;
            using (SqlConnection conn = new SqlConnection(cs))
            {
                try
                {
                    conn.Open();
                    conn.Close();
                    retCode = true;
                }
                catch (Exception)
                {
                    retCode = false;
                }
            }

            return retCode;
        }

        #endregion Public Methods

        #region Private/Protected Methods

        /// <summary>
        /// PopulateObject() - Populates an Object from a DataRow
        /// </summary>
        /// <param name="dr">DataRow (Source) object</param>
        /// <param name="td">TableData (Destination) object</param>
        protected void PopulateObject(DataRow dr, TableData td)
        {
            td.TableName = dr.Field<string>("ObjectName");
        }

        #endregion Private/Protected Methods
    }
}
