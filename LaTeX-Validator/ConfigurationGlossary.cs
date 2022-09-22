﻿using System.Collections.Generic;
using System.Collections.Specialized;

namespace LaTeX_Validator
{
    internal class ConfigurationGlossary
    {
        #region Pfade

        public string? latexDirectoryPath { get; set; }
        public string? glossaryPath { get; set; }
        public string? preambleDirectoryPath { get; set; }
        public string? bibPath { get; set; }
        public StringCollection? ignoreFilesWithMissingGls { get; set; }

        #endregion

        #region Options

        public bool ignoreSectionLabels { get; set; }
        public bool ignoreSettingsFile { get; set; }
        public bool showIgnoredErrors { get; set; }

        #endregion

        public bool IsMissingSomething()
        {
            return string.IsNullOrEmpty(this.latexDirectoryPath) || string.IsNullOrEmpty(this.glossaryPath) ||
                   string.IsNullOrEmpty(this.preambleDirectoryPath) || string.IsNullOrEmpty(this.bibPath);
        }
    }
}
