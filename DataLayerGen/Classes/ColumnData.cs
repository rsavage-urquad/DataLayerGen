namespace DataLayerGen.Classes
{
    /// <summary>
    /// ColumnData Class - Information pertaining to SQL Table Columns
    /// </summary>
    public class ColumnData
    {
        public int ColumnId { get; set; }
        public string ColumnName { get; set; }
        public string ObjectName { get; set; }
        public string ObjectType { get; set; }
        public string DataType { get; set; }
        public int Length { get; set; }
        public string SqlDataType { get; set; }
        public bool IsNullable { get; set; }
        public string MiscInfo { get; set; }

        /// <summary>
        /// ColumnData() - Default Constructor
        /// </summary>
        public ColumnData()
        {
            ColumnId = 0;
            ColumnName = "";
            ObjectName = "";
            ObjectType = "";
            DataType = "";
            Length = 0;
            SqlDataType = "";
            IsNullable = true;
            MiscInfo = "";
        }
    }
}
