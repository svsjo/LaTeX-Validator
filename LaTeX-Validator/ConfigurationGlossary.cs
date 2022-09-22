namespace LaTeX_Validator
{
    internal class ConfigurationGlossary
    {
        #region Pfade

        public string? latexDirectoryPath { get; set; }
        public string? glossaryPath { get; set; }
        public string? preambleDirectoryPath { get; set; }

        #endregion

        #region Options

        public bool ignoreSectionLabels { get; set; } = true;

        #endregion
    }
}
