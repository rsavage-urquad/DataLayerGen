namespace DataLayerGen.Classes
{
    public class TemplateInfo
    {
        public string TemplateId { get; set; }
        public string Title { get; set; }
        public string TemplateFilename { get; set; }
        public string OutputName { get; set; }

        public TemplateInfo()
        {
            TemplateId = "";
            Title = "";
            TemplateFilename = "";
            OutputName = "";
        }
    }
}
