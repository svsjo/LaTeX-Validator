// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AllExtractions.cs" company="www.arburg.com">
// Arburg GmbH + Co. KG all rights reserved.
// </copyright>
// <author>Jonas Weis</author>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.IO;
using System.Linq;
using LaTeX_Validator.Extensions;
using static LaTeX_Validator.DataClasses.DataTemplates;

namespace LaTeX_Validator.DataClasses;

internal class AllExtractions
{
    private readonly ConfigurationGlossary configuration;
    private readonly FileExtractor fileExtractor;
    public AllExtractions(ConfigurationGlossary pConfiguration, FileExtractor pFileExtractor)
    {
        this.configuration = pConfiguration;
        this.fileExtractor = pFileExtractor;
    }

    public List<string> allFiles { get; private set; } = new();
    public List<string> beforeFiles { get; private set; } = new();
    private List<string> contentFiles { get; set; } = new();
    public List<AcronymEntry> allAcronymEntries { get; private set; } = new();
    public List<string> allGlossaryEntries { get; private set; } = new();
    public List<CitationEntry> allCitationEntries { get; private set; } = new();
    public List<CitationUsage> allCitations { get; private set; } = new();
    public List<LabelDefinition> allLabelEntries { get; private set; } = new();
    public List<ReferenceUsage> allRefs { get; private set; } = new();
    public List<Area> allAreas { get; private set; } = new();
    public List<Sentence> allSenetences { get; private set; } = new();
    public List<string> missingGlsFiles { get; private set; } = new();

    public void ExtractAll()
    {
        this.ExtractFiles();
        this.ExtractEntries();
        this.ExtractUsages();
    }

    private void ExtractFiles()
    {
        this.allFiles = Directory.GetFiles(
            this.configuration.latexDirectoryPath!, "*.tex", SearchOption.AllDirectories).ToList();
        this.beforeFiles = Directory.GetFiles(
            this.configuration.preambleDirectoryPath!, "*.tex", SearchOption.AllDirectories).ToList();
        var tempFiles = Directory.GetFiles(
            this.configuration.latexDirectoryPath!, "*.tex", SearchOption.TopDirectoryOnly).ToList();
        this.contentFiles = this.allFiles.Except(tempFiles).ToList();
        this.missingGlsFiles = this.configuration.ignoreFilesWithMissingGls == null ? this.allFiles : this.allFiles
                                   .Except(this.configuration.ignoreFilesWithMissingGls.ToList())
                                   .ToList();
    }

    private void ExtractEntries()
    {
        this.allAcronymEntries = this.fileExtractor.GetAcronymEntries(this.configuration.glossaryPath!).ToList();
        this.allGlossaryEntries = this.fileExtractor.GetGlossaryEntries(this.configuration.glossaryPath!).ToList();
        this.allCitationEntries = this.fileExtractor.GetCitationEntries(this.configuration.bibPath!).ToList();
        this.allLabelEntries = this.fileExtractor.GetAllLabels(this.allFiles).ToList();
    }

    private void ExtractUsages()
    {
        this.allCitations = this.fileExtractor.GetAllCitations(this.allFiles).ToList();
        this.allRefs = this.fileExtractor.GetAllRefs(this.allFiles).ToList();
        this.allAreas = this.fileExtractor.GetAllCriticalAreas(this.allFiles).ToList();
        this.allSenetences = this.fileExtractor.GetAllSentences(this.contentFiles).ToList();
    }
}
