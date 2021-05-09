using System;

namespace DataLayerGen.Classes
{
    /// <summary>
    /// ModifierParser() - Parses Modifier style commands such as "First" or "Last"
    /// </summary>
    public class ModifierParser
    {
        #region Properties

        public string InfoToParse { get; set; }
        public bool Condition { get; set; }
        public string Prefix { get; set; }
        public string Command { get; set; }
        public string TrueOutput { get; set; }
        public string FalseOutput { get; set; }
        public string Suffix { get; set; }

        #endregion Properties

        #region Constructor(s)

        /// <summary>
        /// ModifierParser() - Default Constructor
        /// </summary>
        /// <param name="infoToParse">Info to Parse</param>
        /// <param name="condition">Condition to return</param>
        public ModifierParser(string infoToParse, bool condition)
        {
            InfoToParse = infoToParse;
            Condition = condition;
            Command = "";
            TrueOutput = "";
            FalseOutput = "";
            Suffix = "";
        }

        #endregion Constructor(s)

        #region Processing

        /// <summary>
        /// Parse() - Parse the Modifier info
        /// </summary>
        public string Parse()
        {
            ParserSplit ps = new ParserSplit();
            Prefix = ps.Split(InfoToParse, "[");
            Command = ps.Split(ps.After, "|");
            TrueOutput = ps.Split(ps.After, ":");
            FalseOutput = ps.Split(ps.After, "]");
            Suffix = ps.After;

            string result = (Condition) ? TrueOutput : FalseOutput;
            return Prefix + result + Suffix;
        }

        #endregion Processing
    }
}
