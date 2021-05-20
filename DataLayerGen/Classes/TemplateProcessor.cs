using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DataLayerGen.Classes
{
    /// <summary>
    /// TemplateProcessor Class - Processes the Template.
    /// </summary>
    class TemplateProcessor
    {
        #region Properties

        private TemplateInfo TemplateInfo = new TemplateInfo();
        private List<ColumnData> ColDataList = new List<ColumnData>();
        private string SchemaName = "";
        private string TableName = "";
        private string SaveLocation = "";
        private string[] IdColumns = new string[0];
        private string NameColumn = "";
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
            NameColumn = nameCol;
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

        /// <summary>
        /// ProcessTemplateLines() - Process the Template Lines and add the results
        /// to the output collection.
        /// </summary>
        /// <param name="lines">Lines to process</param>
        /// <param name="output">Output collection to add processed lines to</param>
        private void ProcessTemplateLines(List<string> lines, List<string> output)
        {

            for (int currLine = 0; currLine < lines.Count; currLine++)
            {
                string line = lines[currLine];
                if (line.Contains("{{SectionIf"))
                {
                    int endline = ProcessSectionIf(lines, output, currLine);
                    currLine = endline;
                }
                else
                {
                    ProcessLine(line, output);
                }
            }
        }

        /// <summary>
        /// ProcessSectionIf() - Process a "SectionIf" directive by a) determining if the Section should be 
        /// included, b) identifying the lines involved in the Section and c) processing the Section lines.
        /// </summary>
        /// <param name="lines">Lines collection to process</param>
        /// <param name="output">Output (passed to "Processing" method</param>
        /// <param name="startLine">Line number of "lines" collection to start at.</param>
        /// <returns>Line that contains the "End of Section" identifier</returns>
        private int ProcessSectionIf(List<string> lines, List<string> output, int startLine)
        {
            bool gotEndOfSection = false;
            int currLine;
            List<string> sectionLines = new List<string>();
            string line = lines[startLine];

            // Determine if condition exists to include section
            CommandParser cmd = new CommandParser(line);
            cmd.Parse();
            bool shouldProcessSection = ((cmd.Param1.ToLower() == "activepresent") && ((ActiveColumn != "")));

            // Load Session Lines (need to locate End of Section even if not including the section.
            for (currLine = (startLine + 1); currLine < lines.Count; currLine++)
            {
                line = lines[currLine];

                if (line.Contains("{{/SectionIf"))
                {
                    gotEndOfSection = true;
                    break;
                }
                sectionLines.Add(line);
            }

            // No End of Section found!
            if (!gotEndOfSection)
            {
                throw new Exception("No {{/SectionIf}}");
            }

            // If applicable, call the ProcessTemplateLines() method to process the section lines 
            if (shouldProcessSection)
            {
                ProcessTemplateLines(sectionLines, output);
            }

            return currLine;
        }

        /// <summary>
        /// ProcessLine() - Process an input line to create the desired output
        /// </summary>
        /// <param name="line">Input Line</param>
        /// <param name="outputLines">Array of Output Lines (to potentially add the result to)</param>
        private void ProcessLine(string line, List<string> outputLines)
        {
            line = PerformSubstitution(line);
            line = PerformEach(line, outputLines);
            line = PerformIf(line);

            if (line != "{{Ignore}}")
            {
                outputLines.Add(line);
            }
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
            line = line.Replace("{{NameColName}}", NameColumn);
            line = line.Replace("{{NameColType}}", GetNameColSqlType());
            return line;
        }

        /// <summary>
        /// PerformEach() - Process an "Each" command in the line (it it exists).
        /// </summary>
        /// <param name="line">Line to process</param>
        /// <param name="outputLines">Output Lines List (to add items to)</param>
        /// <returns>Line (if "Each" is not present or "{{Ignore}}" if the "Each" was processed.</returns>
        private string PerformEach(string line, List<string> outputLines)
        {
            string workLine;
            bool isFirst;
            bool isLast;
            int itemNum = 0;

            if (line.Contains("{{Each|") == false)
            {
                return line;
            }

            CommandParser cmd = new CommandParser(line);
            cmd.Parse();

            List<ColumnData> workColList = getWorkColumnList(cmd.Param1);
            foreach (ColumnData item in workColList)
            {
                itemNum++;
                isFirst = (itemNum == 1);
                isLast = (itemNum == workColList.Count);

                workLine = cmd.Prefix + cmd.Param2 + cmd.Suffix;
                workLine = workLine.Replace("[[ColName]]", item.ColumnName);
                workLine = workLine.Replace("[[ColSqlType]]", item.SqlDataType);

                if (workLine.Contains("[First|"))
                {
                    ModifierParser mp = new ModifierParser(workLine, isFirst);
                    workLine = mp.Parse();
                }

                if (workLine.Contains("[Last|"))
                {
                    ModifierParser mp = new ModifierParser(workLine, isLast);
                    workLine = mp.Parse();
                }

                workLine = PerformIf(workLine);

                outputLines.Add(workLine);
            }

            return "{{Ignore}}";
        }

        /// <summary>
        /// PerformIf() - Process the any "If" commands in the line
        /// </summary>
        /// <param name="line">Line to be processed.</param>
        /// <returns>Output Line</returns>
        private string PerformIf(string line)
        {
            string resultLine = "{{Ignore}}";

            if (line.Contains("{{If|") == false)
            {
                return line;
            }

            CommandParser cmd = new CommandParser(line);
            cmd.Parse();

            switch (cmd.Param1.ToLower())
            {
                case "activepresent":
                    resultLine = (ActiveColumn != "") ? cmd.Prefix + cmd.Param2 + cmd.Suffix : cmd.Prefix;
                    resultLine = (string.IsNullOrWhiteSpace(resultLine)) ? "{{Ignore}}" : resultLine;
                    break;
                default:
                    break;
            }

            return resultLine;
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

        /// <summary>
        /// GetNameColSqlType() - Retrieves the SQL Type for the Name Column.
        /// </summary>
        /// <returns>SQL Type for the Name column</returns>
        private string GetNameColSqlType()
        {
            if (NameColumn == "")
            {
                return "";
            }

            ColumnData col = ColDataList.Find(cd => cd.ColumnName == NameColumn);
            if (col is null)
            {
                return "";
            }

            return col.SqlDataType;
         }

        /// <summary>
        /// getWorkColumnList() - Returns a list of columns meeting the requested criteria.
        /// </summary>
        /// <param name="colInfoRequest">Column Infor to retrieve ("ColList", "IdCols" and "ColListExceptIds")</param>
        /// <returns>List of relevant Column Data</returns>
        private List<ColumnData> getWorkColumnList(string colInfoRequest)
        {
            List<ColumnData> respList = new List<ColumnData>();
            bool isIDCol;

            foreach (ColumnData item in ColDataList)
            {
                isIDCol = Array.Exists(IdColumns, col => col == item.ColumnName);
                switch (colInfoRequest.ToLower())
                {
                    case "collist":
                        respList.Add(item);
                        break;
                    case "idcols":
                        if (isIDCol)
                        {
                            respList.Add(item);
                        }
                        break;
                    case "collistexceptids":
                        if (!isIDCol)
                        {
                            respList.Add(item);
                        }
                        break;
                    default:
                        throw new Exception("Unknown \"Each\" Param1");
                }

            }
            return respList;
        }

        #endregion Helpers

    }
}
