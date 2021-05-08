using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace DataLayerGen.DataLayer
{
    /// <summary>
    /// BaseData Class - Base class for the Data Layer.  Includes various Helper methods.
    /// </summary>
    public class BaseData
    {
        /// <summary>
        /// GetResultSet() - Used to process SQL Commands that return a Result Set.
        /// </summary>
        /// <param name="cmd">Database Command to process</param>
        /// <returns>Dataset object</returns>
        public DataTable GetResultSet(SqlCommand cmd)
        {
            DataTable dataTable = null;

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd))
            {
                try
                {
                    cmd.Connection.Open();
                    DataSet dataSet = new DataSet();
                    dataSet.EnforceConstraints = false;
                    sqlDataAdapter.Fill(dataSet);
                    if (dataSet.Tables.Count > 0)
                    {
                        dataTable = dataSet.Tables[0];
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }

            return dataTable;
        }


        /// <summary>
        /// GetMultipleResultSets() - Used to process SQL Commands that return multiple Result Sets.
        /// This is just an implementation of GetResultSet() for multiple result sets.   It is up to the 
        /// developer to know the order of the result tables and to handle them properly.
        /// </summary>
        /// <param name="cmd">Database Command to process</param>
        /// <returns>List of DataTable objects</returns>
        public List<DataTable> GetMultipleResultSets(SqlCommand cmd)
        {
            List<DataTable> dataTables = new List<DataTable>();

            using (SqlDataAdapter sqlDataAdapter = new SqlDataAdapter(cmd))
            {
                try
                {
                    cmd.Connection.Open();
                    DataSet dataSet = new DataSet();
                    dataSet.EnforceConstraints = false;
                    sqlDataAdapter.Fill(dataSet);
                    for (int idx = 0; idx < dataSet.Tables.Count; idx++)
                    {
                        dataTables.Add(dataSet.Tables[idx]);
                    }
                }
                catch (Exception ex)
                {
                    throw ex;
                }
                finally
                {
                    cmd.Connection.Close();
                }
            }

            return dataTables;
        }


        #region Helpers

        /// <summary>
        /// GetNullSafeString() - Checks if the string value is null, if so, 
        /// the string.Empty constant will be returned.
        /// </summary>
        /// <param name="value">Database Record</param>
        /// <param name="colIndex">Column Index</param>
        /// <returns>Actual String or "String.Empty" constant</returns>
        public string GetNullSafeString(string value)
        {
            return value ?? string.Empty;
        }

        /// <summary>
        /// GetNullSafeDateTime() - Checks the DateTime column to be accessed and if it is DBNull, 
        /// the DateTime.MinValue constant will be returned.
        /// </summary>
        /// <param name="rec">Database Record</param>
        /// <param name="colIndex">Column Index</param>
        /// <returns>Actual DateTime or MinValue (if null)</returns>
        public DateTime GetNullSafeDateTime(DbDataRecord rec, int colIndex)
        {
            return rec.IsDBNull(colIndex) ? DateTime.MinValue : rec.GetDateTime(colIndex);
        }

        /// <summary>
        /// GetNullSafeInt32() - Checks the Int32 column to be accessed and if it is DBNull, 
        /// 0 will be returned.
        /// </summary>
        /// <param name="rec">Database Record</param>
        /// <param name="colIndex">Column Index</param>
        /// <returns>Actual DateTime or MinValue (if null)</returns>
        public int GetNullSafeInt32(DbDataRecord rec, int colIndex)
        {
            return rec.IsDBNull(colIndex) ? 0 : rec.GetInt32(colIndex);
        }

        /// <summary>
        /// GetNullSafeDecimal() - Checks the Decimal column to be accessed and if it is DBNull, 
        /// 0.0M will be returned.
        /// </summary>
        /// <param name="rec">Database Record</param>
        /// <param name="colIndex">Column Index</param>
        /// <returns>Actual DateTime or MinValue (if null)</returns>
        public decimal GetNullSafeDecimal(DbDataRecord rec, int colIndex)
        {
            return rec.IsDBNull(colIndex) ? 0.0M : rec.GetDecimal(colIndex);
        }

        /// <summary>
        /// PopulateNullableParameter() - If the passed parameter is null, this method
        /// will return DBNull.Value, otherwise return the parameter.
        /// </summary>
        /// <param name="parameter">Parameter to be tested</param>
        /// <returns>Parameter or DBNull.Value</returns>
        public object PopulateNullableParameter(object parameter)
        {
            return parameter ?? DBNull.Value;
        }

        /// <summary>
        /// GetIntegerValue() - Attempt to parse the supplied value into an Integer
        /// </summary>
        /// <param name="val">Value to coerce</param>
        /// <param name="intVal">Ouptut integer value.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool GetIntegerValue(string val, out int intVal)
        {
            return int.TryParse(val, out intVal);
        }

        /// <summary>
        /// GetDecimalValue() - Attempt to parse the supplied value into an Decimal
        /// </summary>
        /// <param name="val">Value to coerce</param>
        /// <param name="decVal">Ouptut decimal value.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool GetDecimalValue(string val, out decimal decVal)
        {
            return decimal.TryParse(val, out decVal);
        }

        /// <summary>
        /// GetDateTimeValue() - Attempt to parse the supplied value into an DateTime
        /// </summary>
        /// <param name="val">Value to coerce</param>
        /// <param name="dtVal">Output DateTime value.</param>
        /// <returns>True if successful, otherwise false.</returns>
        public bool GetDateTimeValue(string val, out DateTime dtVal)
        {
            return DateTime.TryParse(val, out dtVal);
        }

        /// <summary>
        /// GetXml() - Converts the supplied object to an XML string
        /// </summary>
        /// <param name="paramItems">Information to convert to XML</param>
        /// <param name="type">Type of Object passed</param>
        /// <param name="rootName">Override returned Root Element with this Name</param>
        /// <returns>String of XML Data</returns>
        public string GetXml(object paramItems, Type type, string rootName)
        {
            string xml;

            // Serialize
            // 2016-06-06 RS: "XmlSerializer(<Type>, <XmlRootAttribute>)" Constructor causes Memory Leak (Per MDSN Documentation), using "XmlSerializer(<Type>)"
            XmlSerializer xmlSerializer = new XmlSerializer(type);

            using (var stringWriter = new StringWriter())
            {
                var xmlWriterSettings = new XmlWriterSettings();
                xmlWriterSettings.OmitXmlDeclaration = true;
                xmlWriterSettings.ConformanceLevel = ConformanceLevel.Document;
                using (var xmlWriter = XmlWriter.Create(stringWriter, xmlWriterSettings))
                {
                    xmlSerializer.Serialize(xmlWriter, paramItems);
                    xml = stringWriter.ToString();
                }
            }

            // 2016-06-06 RS: Replace Root Tag Name (if "elementName" supplied).
            if (rootName != null && rootName.Trim().Length > 0)
            {
                // 2016-09-16 RS: Changed method to determine Root Element name due to "Name Mangling" 
                //String typeName = type.Name;
                String typeName = GetGeneratedXMLRoot(xml);

                int targetLen = typeName.Length + 1;
                StringBuilder str = new StringBuilder();
                str.Append(xml.Substring(0, targetLen).Replace("<" + typeName, "<" + rootName));
                str.Append(xml.Substring(targetLen, (xml.Length - ((2 * targetLen) + 2))));
                str.Append(xml.Substring(xml.Length - (targetLen + 2)).Replace("</" + typeName, "</" + rootName));
                xml = str.ToString();
            }

            // Set return
            return xml;
        }

        /// <summary>
        /// GetGeneratedXMLRoot() - When using XMLWriter to geneate XML Strings, the Root Element can be
        /// "Name Mangled" (i.e. - May not be the Object type).  This is particularily true of Generics
        /// and some primative types (i.e. - Typeof "Int32" would be "int").  This method will extract and 
        /// return the Root Element name.
        /// </summary>
        /// <param name="xml">Generated XML String to process</param>
        /// <returns>Root Element Name</returns>
        protected string GetGeneratedXMLRoot(string xml)
        {
            string retValue;

            // Root element name will end with with space or ">", find end index and determine lesser.
            int firstSpace = xml.IndexOf(" ", StringComparison.Ordinal);
            int firstGt = xml.IndexOf(">", StringComparison.Ordinal);
            int rootEnd;

            // If token not found set to a high value (so it will not be used as "rootEnd").
            firstSpace = firstSpace < 0 ? 9999 : firstSpace;
            firstGt = firstGt < 0 ? 9999 : firstGt;

            if ((firstSpace == 9999) && (firstGt == 9999))
            {
                // This should not happen.  return Dummy Root Name
                return "--InvalidXML--";
            }

            // Get Lesser index
            rootEnd = firstSpace <= firstGt ? firstSpace : firstGt;

            // Get the string (omit the "<" character.
            retValue = xml.Substring(1, (rootEnd - 1));

            return retValue;
        }

        #endregion Helpers

    }
}
