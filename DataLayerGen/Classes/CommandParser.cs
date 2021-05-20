using System;

namespace DataLayerGen.Classes
{
    public class CommandParser
    {
        #region Properties

        public string InfoToParse { get; set; }
        public string Command { get; set; }
        public string Prefix { get; set; }
        public string Param1 { get; set; }
        public string Param2 { get; set; }
        public string Suffix { get; set; }

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        /// CommandParser() - Default Constructor
        /// </summary>
        /// <param name="infoToParse">Info to Parse</param>
        public CommandParser(string infoToParse)
        {
            InfoToParse = infoToParse;
            Command = "";
            Prefix = "";
            Param1 = "";
            Param2 = "";
            Suffix = "";
        }

        #endregion Constructor(s)

        #region Processing

        /// <summary>
        /// Parse() - Parses a Command Line, such as "If" or "Each".
        /// </summary>
        public void Parse()
        {
            ParserSplit ps = new ParserSplit();
            Prefix = ps.Split(InfoToParse, "{{");
            Command = ps.Split(ps.After, "|");

            // "SectionIf" has only Param1
            if (Command.ToLower() == "sectionif")
            {
                Param1 = ps.Split(ps.After, "}}");
                return;
            }

            Param1 = ps.Split(ps.After, "|");
            Param2 = ps.Split(ps.After, "}}");
            Suffix = ps.After;
        }

        #endregion Processing
    }

}
