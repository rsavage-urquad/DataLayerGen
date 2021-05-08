namespace DataLayerGen.Classes
{
    /// <summary>
    /// TableData Class - Information pertaining to SQL User Defined Tables
    /// </summary>
    public class TableData
    {
        public string TableName { get; set; }

        /// <summary>
        /// ColumnData() - Default Constructor
        /// </summary>
        public TableData()
        {
            TableName = "";
        }
    }
}
