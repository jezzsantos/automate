using System;
using System.Collections.Generic;
using System.Linq;
using Automate.CLI.Extensions;

namespace Automate.CLI.Domain
{
    internal class ToolkitDefinition : IIdentifiableEntity, IPersistable
    {
        private readonly List<CodeTemplateFile> codeTemplateFiles;

        public ToolkitDefinition(PatternDefinition pattern, Version version = null)
        {
            pattern.GuardAgainstNull(nameof(pattern));

            Id = pattern.Id;
            Pattern = pattern;
            Version = version.Exists()
                ? version.ToString(ToolkitVersion.VersionFieldCount)
                : pattern.ToolkitVersion.Current;
            this.codeTemplateFiles = new List<CodeTemplateFile>();
        }

        private ToolkitDefinition(PersistableProperties properties, IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Version = properties.Rehydrate<string>(factory, nameof(Version));
            Pattern = properties.Rehydrate<PatternDefinition>(factory, nameof(Pattern));
            this.codeTemplateFiles = properties.Rehydrate<List<CodeTemplateFile>>(factory, nameof(CodeTemplateFiles));
        }

        public string Version { get; private set; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; private set; }

        public IReadOnlyList<CodeTemplateFile> CodeTemplateFiles => this.codeTemplateFiles;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Version), Version);
            properties.Dehydrate(nameof(Pattern), Pattern);
            properties.Dehydrate(nameof(CodeTemplateFiles), CodeTemplateFiles);

            return properties;
        }

        public static ToolkitDefinition Rehydrate(PersistableProperties properties, IPersistableFactory factory)
        {
            return new ToolkitDefinition(properties, factory);
        }

        public void AddCodeTemplateFiles(List<CodeTemplateFile> files)
        {
            files.ForEach(AddCodeTemplateFile);
        }

        public void Pack(Func<PatternDefinition, CodeTemplate, byte[]> getTemplateContent)
        {
            var codeTemplates = Pattern.GetAllCodeTemplates();
            if (codeTemplates.HasNone())
            {
                return;
            }

            AddCodeTemplateFiles(
                codeTemplates
                    .Select(template =>
                    {
                        var contents = getTemplateContent(Pattern, template.Template);
                        return new CodeTemplateFile(contents, template.Template.Id);
                    })
                    .ToList());
        }

        public void MigratePattern(ToolkitDefinition latestToolkit, SolutionUpgradeResult result)
        {
            //TODO: complete the migration (https://github.com/jezzsantos/automate/issues/32)
            MigrateCodeTemplates(latestToolkit, result);

            // results = Model.Migrate(latestToolkit); //navigate solutionitems and compare old and new schema. Auto-fix, reset or delete each solutionitem.
            // result.Append(results);
            // write the new tookit.pattern over the top of existing and save the solution
            Pattern = latestToolkit.Pattern;
            Version = latestToolkit.Pattern.ToolkitVersion.Current;
        }

        public string Id { get; }

        private void AddCodeTemplateFile(CodeTemplateFile file)
        {
            var templateFile = this.codeTemplateFiles.FirstOrDefault(ctf => ctf.Id == file.Id);
            if (templateFile.Exists())
            {
                throw new AutomateException(ExceptionMessages.ToolkitDefinition_CodeTemplateFileAlreadyExists.Format(file.Id));
            }

            this.codeTemplateFiles.Add(file);
        }

        private void RemoveCodeTemplateFile(string id)
        {
            id.GuardAgainstNullOrEmpty(nameof(id));

            var templateFile = this.codeTemplateFiles.FirstOrDefault(ctf => ctf.Id == id);
            if (templateFile.Exists())
            {
                this.codeTemplateFiles.Remove(templateFile);
            }
        }

        private void MigrateCodeTemplates(ToolkitDefinition latestToolkit, SolutionUpgradeResult result)
        {
            latestToolkit.GuardAgainstNull(nameof(latestToolkit));

            var codeTemplates = Pattern.GetAllCodeTemplates();
            if (codeTemplates.HasNone())
            {
                return;
            }

            var currentTemplateFiles = CodeTemplateFiles.ToList();
            var latestTemplateFiles = latestToolkit.CodeTemplateFiles.ToList();
            var latestCodeTemplates = latestToolkit.Pattern.GetAllCodeTemplates();
            SyncCodeFileTemplates(result, currentTemplateFiles, codeTemplates, latestTemplateFiles, latestCodeTemplates);
        }

        private void SyncCodeFileTemplates(SolutionUpgradeResult result, List<CodeTemplateFile> currentTemplateFiles, List<(CodeTemplate Template, IPatternElement Parent)> currentCodeTemplates, List<CodeTemplateFile> latestTemplateFiles,
            List<(CodeTemplate Template, IPatternElement Parent)> latestCodeTemplates)
        {
            var templateFilesToDelete = new List<string>();
            var templateFilesToAdd = new List<CodeTemplateFile>();

            currentTemplateFiles.ForEach(currentTemplate =>
            {
                var latestTemplate = latestTemplateFiles.FirstOrDefault(latest => latest.Id == currentTemplate.Id);
                if (latestTemplate.NotExists())
                {
                    templateFilesToDelete.Add(currentTemplate.Id);
                }
                else
                {
                    if (!latestTemplate.Contents.SequenceEqual(currentTemplate.Contents))
                    {
                        var (template, _) = currentCodeTemplates.Single(temp => temp.Template.Id == currentTemplate.Id);

                        currentTemplate.SetContent(latestTemplate.Contents);
                        result.Add(MigrationChangeType.NonBreaking,
                            MigrationMessages.ToolkitDefinition_CodeTemplateFile_ContentUpgraded, template.Name, template.Id);
                    }
                }
            });

            latestTemplateFiles.ForEach(latestTemplate =>
            {
                var currentTemplate = currentTemplateFiles.FirstOrDefault(current => current.Id == latestTemplate.Id);
                if (currentTemplate.NotExists())
                {
                    templateFilesToAdd.Add(latestTemplate);
                }
            });

            templateFilesToDelete.ForEach(id =>
            {
                var (template, _) = currentCodeTemplates.Single(temp => temp.Template.Id == id);
                RemoveCodeTemplateFile(id);
                result.Add(MigrationChangeType.Breaking,
                    MigrationMessages.ToolkitDefinition_CodeTemplateFile_Deleted, template.Name, id);
            });
            templateFilesToAdd.ForEach(file =>
            {
                var (template, _) = latestCodeTemplates.Single(temp => temp.Template.Id == file.Id);
                AddCodeTemplateFile(file);
                result.Add(MigrationChangeType.NonBreaking,
                    MigrationMessages.ToolkitDefinition_CodeTemplateFile_Added, template.Name, template.Id);
            });
        }
    }
}