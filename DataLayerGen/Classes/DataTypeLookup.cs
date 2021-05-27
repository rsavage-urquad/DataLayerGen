using System;
using System.Collections.Generic;

namespace DataLayerGen.Classes
{
    /// <summary>
    /// DataTypeLookup() - Class representing the SQL Data Type to Code Data Type Cross Reference
    /// and processing logic.
    /// </summary>
    public static class DataTypeLookup
    {
        #region Properties

        private static Dictionary<string, CodeDataTypeInfo> _TypeXref;

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        /// DataTypeLookup() - Default Constructor
        /// </summary>
        static DataTypeLookup()
        {
            _TypeXref = new Dictionary<string, CodeDataTypeInfo>();

            _TypeXref.Add("char", new CodeDataTypeInfo("string",  false, "\"\""));
            _TypeXref.Add("varchar", new CodeDataTypeInfo("string", false, "\"\""));
            _TypeXref.Add("text", new CodeDataTypeInfo("string", false, "\"\""));
            _TypeXref.Add("nchar", new CodeDataTypeInfo("string", false, "\"\""));
            _TypeXref.Add("nvarchar", new CodeDataTypeInfo("string", false, "\"\""));
            _TypeXref.Add("ntext", new CodeDataTypeInfo("string", false, "\"\""));
            _TypeXref.Add("bit", new CodeDataTypeInfo("bool", true, "false"));
            _TypeXref.Add("tinyint", new CodeDataTypeInfo("int", true, "0"));
            _TypeXref.Add("smallint", new CodeDataTypeInfo("int", true, "0"));
            _TypeXref.Add("int", new CodeDataTypeInfo("int", true, "0"));
            _TypeXref.Add("bigint", new CodeDataTypeInfo("long", true, "0"));
            _TypeXref.Add("float", new CodeDataTypeInfo("double", true, "0.0"));
            _TypeXref.Add("real", new CodeDataTypeInfo("float", true, "0.0"));
            _TypeXref.Add("numeric", new CodeDataTypeInfo("decimal", true, "0.0"));
            _TypeXref.Add("decimal", new CodeDataTypeInfo("decimal", true, "0.0"));
            _TypeXref.Add("smallmoney", new CodeDataTypeInfo("decimal", true, "0.0"));
            _TypeXref.Add("money", new CodeDataTypeInfo("decimal", true, "0.0"));
            _TypeXref.Add("datetime", new CodeDataTypeInfo("DateTime", true, "DateTime.MinValue"));
            _TypeXref.Add("datetime2", new CodeDataTypeInfo("DateTime", true, "DateTime.MinValue"));
            _TypeXref.Add("smalldatetime", new CodeDataTypeInfo("DateTime", true, "DateTime.MinValue"));
            _TypeXref.Add("date", new CodeDataTypeInfo("DateTime", true, "DateTime.MinValue"));
            _TypeXref.Add("time", new CodeDataTypeInfo("DateTime", true, "DateTime.MinValue"));
        }

        #endregion Constructor(s)

        #region Processing

        /// <summary>
        /// GetCodeDataType() - Return the Code Data Type for the requested SQL Data Type.
        /// </summary>
        /// <param name="sqlDataType"SQL Data Type></param>
        /// <returns>Code Data Type</returns>
        public static string GetCodeDataType(ColumnData cd)
        {
            string workSqlDataType = cd.DataType.ToLower();
            bool isCodelDataTypeNullable = false;
            string result = "undefined";

            if (_TypeXref.ContainsKey(workSqlDataType))
            {
                result = _TypeXref[workSqlDataType].CodeDateType;
                isCodelDataTypeNullable = _TypeXref[workSqlDataType].CodeDataTypeIsNullable;
            }

            if ((isCodelDataTypeNullable) && (cd.IsNullable))
            {
                result += "?";
            }

            return result;
        }

        /// <summary>
        /// GetCodeDefaultValue() - Gets the Default value for the requested Data Column
        /// </summary>
        /// <param name="cd">Column Data information</param>
        /// <returns>Default Value</returns>
        public static string GetCodeDefaultValue(ColumnData cd)
        {
            string workSqlDataType = cd.DataType.ToLower();
            bool isCodelDataTypeNullable = false;
            string result = "undefined";

            if (_TypeXref.ContainsKey(workSqlDataType))
            {
                isCodelDataTypeNullable = _TypeXref[workSqlDataType].CodeDataTypeIsNullable;
                result = ((isCodelDataTypeNullable) && (cd.IsNullable)) ? "null" : _TypeXref[workSqlDataType].DefaultValue;
            }

            return result;
        }

        #endregion Processing
    }

    /// <summary>
    /// CodeDataTypeInfo() - Class representing Code Data Type Info
    /// </summary>
    public class CodeDataTypeInfo
    {
        #region CodeDataTypeInfo Properties

        public string CodeDateType { get; set; }
        public bool CodeDataTypeIsNullable { get; set; }
        public string DefaultValue { get; set; }

        #endregion CodeDataTypeInfo Properties

        #region CodeDataTypeInfo Constructor(s)

        /// <summary>
        /// CodeDataTypeInfo() - Default Constructor
        /// </summary>
        /// <param name="dataType">Code Data Type</param>
        /// <<param name="isNullable">Is Nullable</param>
        public CodeDataTypeInfo(string dataType, bool isNullable, string defaultValue)
        {
            CodeDateType = dataType;
            CodeDataTypeIsNullable = isNullable;
            DefaultValue = defaultValue;
        }

        #endregion CodeDataTypeInfo Constructor(s)
    }

}
