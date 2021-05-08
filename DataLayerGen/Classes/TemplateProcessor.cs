using System;
using System.Collections.Generic;
using System.IO;

namespace DataLayerGen.Classes
{
    class TemplateProcessor
    {
        #region Properties

        private TemplateInfo TemplateInfo = new TemplateInfo();
        private List<ColumnData> ColDataList = new List<ColumnData>();
        private string SchemaName = "";
        private string TableName = "";
        private string SaveLocation = "";
        private string[] IdColumns = new string[0];
        private string NameColumns = "";
        private string ActiveColumn = "";
        private string ActiveValue = "";
        private bool IsActiveValueString = false;

        #endregion Properties

        #region Constructor

        /// <summary>
        /// TemplateProcessor() - Default Constructor
        /// </summary>
        /// <param name="templateInfo">Template to Process</param>
        /// <param name="cdList">Column Data List (for selected Table)</param>
        /// <param name="table">Schema and Table info</param>
        /// <param name="saveLoc">Save Location</param>
        /// <param name="idCols">Id Column(s)</param>
        /// <param name="nameCol">Name Column</param>
        /// <param name="activeCol">Active Column</param>
        /// <param name="activeVal">Active Value</param>
        public TemplateProcessor(TemplateInfo templateInfo, List<ColumnData> cdList, string table, string saveLoc, string idCols, 
                                 string nameCol, string activeCol, string activeVal)
        {
            TemplateInfo = templateInfo;
            ColDataList = cdList;
            SaveLocation = saveLoc;
            NameColumns = nameCol;
            ActiveColumn = activeCol;
            ActiveValue = activeVal;

            var tableNameInfo = table.Split('.');
            if (tableNameInfo.Length == 2)
            {
                SchemaName = tableNameInfo[0];
                TableName = tableNameInfo[1];
            }

            char[] charSeparators = new char[] { ';', ',' };
            IdColumns = idCols.Split(charSeparators);

            int temp;
            IsActiveValueString = !(int.TryParse(activeVal, out temp));
        }

        #endregion Constructor

        #region Processing

        /// <summary>
        /// ProcessTemplate() - Process the Template by retrieving the template's
        /// lines and processing them.
        /// </summary>
        /// <returns>Result response, blank is success.</returns>
        public string ProcessTemplate()
        {
            string result = "";
            List<string> outputLines = new List<string>();

            try
            {
                List<string> templateLines = LoadTemplateData(TemplateInfo.TemplateFilename);
                ProcessTemplateLines(templateLines, outputLines);
                if (outputLines.Count > 0)
                {
                    WriteOutput(outputLines, TemplateInfo.OutputName);
                }
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return result;
        }

        private void ProcessTemplateLines(List<string> lines, List<string> output)
        {
            foreach (string line in lines)
            {
                if (line.Contains("{{SectionIf"))
                {
                    ProcessLines(line, output);
                    /*
                    if (SectionIfTrue(line))
                    {
                        // If so grab the content
                        // Recursuive call this method to process the info
                    }
                    */
                }
                else
                {
                    ProcessLines(line, output);
                }
            }
        }

        private void ProcessLines(string line, List<string> outputLines)
        {
            line = PerformSubstitution(line);
            line = PerformEach(line);
            line = PerformIf(line);
            outputLines.Add(line);
        }

        /// <summary>
        /// PerformSubstitution() - Perform substitution for "Simple" tags. 
        /// </summary>
        /// <param name="line">Line to work on</param>
        /// <returns>Processed line.</returns>
        private string PerformSubstitution(string line)
        {
            line = line.Replace("{{Date}}", DateTime.Now.ToString("M/d/yyyy"));
            line = line.Replace("{{Schema}}", SchemaName);
            line = line.Replace("{{Table}}", TableName);
            line = line.Replace("{{ActiveCol}}", ActiveColumn);
            line = line.Replace("{{ActiveValue}}", (IsActiveValueString) ? $"'{ActiveValue}'" : ActiveValue);
            return line;
        }

        private string PerformEach(string line)
        {
            if (line.Contains("{{Each|") == false)
            {
                return line;
            }

            // TODO: Implement "Each" Functionality

            return line;
        }

        private string PerformIf(string line)
        {
            if (line.Contains("{{If|") == false)
            {
                return line;
            }

            // TODO: Implement "If" Functionality

            return line;
        }

        #endregion Processing

        #region Helpers

        /// <summary>
        /// LoadTemplateData() - Loads the Template File contents
        /// </summary>
        /// <param name="templateFilename">Template Filename</param>
        /// <returns>Template contents as a list of string</returns>
        private List<string> LoadTemplateData(string templateFilename)
        {
            return new List<string>(File.ReadAllLines($".\\Templates\\{templateFilename}"));
        }

        /// <summary>
        /// WriteOutput() - Writes the supplied output lines to a file
        /// </summary>
        /// <param name="outputLines">Output Lines</param>
        /// <param name="templateOutputFilename">Template Output Filename</param>
        private void WriteOutput(List<string> outputLines, string templateOutputFilename)
        {
            string outputFilename = templateOutputFilename;
            outputFilename = outputFilename.Replace("{{Schema}}", SchemaName);
            outputFilename = outputFilename.Replace("{{Table}}", TableName);

            using (TextWriter output = new StreamWriter($"{SaveLocation}\\{outputFilename}"))
            {
                foreach (string line in outputLines)
                {
                    output.WriteLine(line);
                }
            }
        }

        #endregion Helpers

    }
}
