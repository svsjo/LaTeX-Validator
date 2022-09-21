using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace LaTeX_Validator
{
    internal class ConfigurationGlossary
    {
        public string latexDirectoryAbsolute { get; set; } = @"C:\Users\entep04\Desktop\Infos\EigeneProjektarbeiten\T2000\T2000_Latex";
        public string mainPathRelative { get; set; } = "main.tex";
        public string glossaryPathRelative { get; set; } = "glossaries.tex";
        public string acrLongDirectoryRelative { get; set; } = "03_Preamble";
    }
}
