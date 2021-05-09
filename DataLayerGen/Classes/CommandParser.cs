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

        public void Parse()
        {
            ParserSplit ps = new ParserSplit();
            Prefix = ps.Split(InfoToParse, "{{");
            Command = ps.Split(ps.After, "|");
            Param1 = ps.Split(ps.After, "|");
            Param2 = ps.Split(ps.After, "}}");
            Suffix = ps.After;
        }

        #endregion Processing
    }


    #region ParserSplit

    public class ParserSplit
    {
        public string Before { get; set; }
        public string After { get; set; }

        /// <summary>
        /// ParserSplit() - Default Constructor
        /// </summary>
        public ParserSplit()
        {
            Before = "";
            After = "";
        }

        public string Split(string source, string delim)
        {
            int pos = source.IndexOf(delim);
            Before = source.Substring(0, pos);
            After = source.Substring(pos + delim.Length);
            return Before;
        }
    }

    #endregion ParserSplit

}
