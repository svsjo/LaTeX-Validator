namespace LaTeX_Validator
{
    internal class ConfigurationGlossary
    {
        public string latexDirectoryAbsolute { get; set; } = @"C:\Users\entep04\Desktop\Infos\EigeneProjektarbeiten\T2000\T2000_Latex";
        public string mainPathRelative { get; set; } = "main.tex";
        public string glossaryName { get; set; } = "glossaries.tex";
        public string beforeDirectoryRelative { get; set; } = "03_Preamble";
        public bool ignoreSectionLabels { get; set; } = true;
    }
}
