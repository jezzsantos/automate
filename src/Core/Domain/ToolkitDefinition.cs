﻿using System;
using System.Collections.Generic;
using System.Linq;
using Automate.Extensions;
using Semver;

namespace Automate.Domain
{
    public class ToolkitDefinition : IIdentifiableEntity, IPersistable
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
            RuntimeVersion = ToolkitConstants.GetRuntimeVersion();
            this.codeTemplateFiles = new List<CodeTemplateFile>();
        }

        private ToolkitDefinition(PersistableProperties properties,
            IPersistableFactory factory)
        {
            Id = properties.Rehydrate<string>(factory, nameof(Id));
            Version = properties.Rehydrate<string>(factory, nameof(Version));
            RuntimeVersion = properties.Rehydrate<string>(factory, nameof(RuntimeVersion));
            Pattern = properties.Rehydrate<PatternDefinition>(factory, nameof(Pattern));
            this.codeTemplateFiles =
                properties.Rehydrate<List<CodeTemplateFile>>(factory, nameof(CodeTemplateFiles));
        }

        public string Version { get; private set; }

        public string RuntimeVersion { get; private set; }

        public string PatternName => Pattern.Name;

        public PatternDefinition Pattern { get; private set; }

        public IReadOnlyList<CodeTemplateFile> CodeTemplateFiles => this.codeTemplateFiles;

        public PersistableProperties Dehydrate()
        {
            var properties = new PersistableProperties();
            properties.Dehydrate(nameof(Id), Id);
            properties.Dehydrate(nameof(Version), Version);
            properties.Dehydrate(nameof(RuntimeVersion), RuntimeVersion);
            properties.Dehydrate(nameof(Pattern), Pattern);
            properties.Dehydrate(nameof(CodeTemplateFiles), CodeTemplateFiles);

            return properties;
        }

        public static ToolkitDefinition Rehydrate(PersistableProperties properties,
            IPersistableFactory factory)
        {
            return new ToolkitDefinition(properties, factory);
        }

        public void AddCodeTemplateFiles(List<CodeTemplateFile> files)
        {
            files.ForEach(AddCodeTemplateFile);
        }

        public void Pack(Func<PatternDefinition, CodeTemplate, CodeTemplateContent> getTemplateContent)
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
                        return new CodeTemplateFile(contents.Content, template.Template.Id);
                    })
                    .ToList());
        }

        public void MigratePattern(ToolkitDefinition latestToolkit, DraftUpgradeResult result)
        {
            MigrateCodeTemplateFiles(latestToolkit, result);

            Pattern = latestToolkit.Pattern;
            Version = latestToolkit.Pattern.ToolkitVersion.Current;
        }

        public void VerifyRuntimeCompatability(IRuntimeMetadata metadata)
        {
            metadata.GuardAgainstNull(nameof(metadata));
            VerifyRuntimeCompatability(metadata, RuntimeVersion);
        }

        public string Id { get; }

        internal static void VerifyRuntimeCompatability(IRuntimeMetadata metadata, string toolkitRuntimeVersion)
        {
            metadata.GuardAgainstNull(nameof(metadata));

            var runtimeName = metadata.ProductName;
            var runtimeVersion = SemVersion.Parse(metadata.RuntimeVersion, SemVersionStyles.Any);
            var toolkitVersion = toolkitRuntimeVersion.HasValue()
                ? SemVersion.Parse(toolkitRuntimeVersion, SemVersionStyles.Any)
                : null;

            if (toolkitVersion.NotExists())
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitDefinition_CompatabilityToolkitNoVersion.Substitute(
                        ToolkitConstants.FirstVersionSupportingRuntimeVersion, runtimeVersion, runtimeName));
            }

            if (IsToolkitOutOfDate())
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitDefinition_CompatabilityToolkitOutOfDate.Substitute(toolkitVersion,
                        runtimeVersion, runtimeName));
            }

            if (IsRuntimeOutOfDate())
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitDefinition_CompatabilityRuntimeOutOfDate.Substitute(toolkitVersion,
                        runtimeVersion, runtimeName));
            }

            bool IsToolkitOutOfDate()
            {
                if (runtimeVersion.IsPrerelease && toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion.Minor > toolkitVersion.Minor;
                }
                if (!runtimeVersion.IsPrerelease && toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion > toolkitVersion.WithoutPrerelease();
                }
                if (runtimeVersion.IsPrerelease && !toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion.WithoutPrerelease() > toolkitVersion;
                }

                return runtimeVersion.Major > toolkitVersion.Major;
            }

            bool IsRuntimeOutOfDate()
            {
                if (runtimeVersion.IsPrerelease && toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion.Minor < toolkitVersion.Minor;
                }
                if (!runtimeVersion.IsPrerelease && toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion < toolkitVersion.WithoutPrerelease();
                }
                if (runtimeVersion.IsPrerelease && !toolkitVersion.IsPrerelease)
                {
                    return runtimeVersion.WithoutPrerelease() < toolkitVersion;
                }

                return runtimeVersion.Major < toolkitVersion.Major;
            }
        }

        private void AddCodeTemplateFile(CodeTemplateFile file)
        {
            var templateFile = this.codeTemplateFiles.FirstOrDefault(ctf => ctf.Id == file.Id);
            if (templateFile.Exists())
            {
                throw new AutomateException(
                    ExceptionMessages.ToolkitDefinition_CodeTemplateFileAlreadyExists.Substitute(file.Id));
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

        private void MigrateCodeTemplateFiles(ToolkitDefinition latestToolkit, DraftUpgradeResult result)
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
            SyncCodeFileTemplates(result, currentTemplateFiles, codeTemplates, latestTemplateFiles,
                latestCodeTemplates);
        }

        private void SyncCodeFileTemplates(DraftUpgradeResult result, List<CodeTemplateFile> currentTemplateFiles,
            List<(CodeTemplate Template, IPatternElement Parent)> currentCodeTemplates,
            List<CodeTemplateFile> latestTemplateFiles,
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
                            MigrationMessages.ToolkitDefinition_CodeTemplateFile_ContentUpgraded, template.Name,
                            template.Id);
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