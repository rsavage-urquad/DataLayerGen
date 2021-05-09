namespace DataLayerGen.Classes
{
    /// <summary>
    /// ParserSplit Class - Utility class to Parse a String.
    /// </summary>
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

        /// <summary>
        /// Split() - Splits a source string based on a delimiter.
        /// </summary>
        /// <param name="source">Source string</param>
        /// <param name="delim">delimiter</param>
        /// <returns>String Before the delimiter.</returns>
        public string Split(string source, string delim)
        {
            int pos = source.IndexOf(delim);
            Before = source.Substring(0, pos);
            After = source.Substring(pos + delim.Length);
            return Before;
        }
    }
}
