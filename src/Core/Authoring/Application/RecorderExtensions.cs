using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;

namespace Automate.Authoring.Application
{
    public static class RecorderExtensions
    {
        public static void CountPatternsListed(this IRecorder recorder)
        {
            recorder.Count("patterns.listed");
        }

        public static void CountPatternViewed(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.viewed",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void CountPatternSwitched(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.switched",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void CountPatternCreated(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.created",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void CountPatternUpdated(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.updated",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void CountAttributeAdded(this IRecorder recorder, PatternDefinition pattern, Attribute attribute)
        {
            recorder.Count("pattern.attribute.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountAttributeUpdated(this IRecorder recorder, PatternDefinition pattern,
            Attribute attribute)
        {
            recorder.Count("pattern.attribute.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountAttributeDeleted(this IRecorder recorder, PatternDefinition pattern,
            Attribute attribute)
        {
            recorder.Count("pattern.attribute.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountElementAdded(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.Count("pattern.element.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountElementUpdated(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.Count("pattern.element.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountElementDeleted(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.Count("pattern.element.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateAdded(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateWithCommandAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation command, CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate-with-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateEdited(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate.edited", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateDeleted(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplatesListed(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.Count("pattern.codetemplates.listed", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateCommandAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation command)
        {
            recorder.Count("pattern.codetemplate-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateCommandUpdated(this IRecorder recorder, PatternDefinition pattern,
            Automation command)
        {
            recorder.Count("pattern.codetemplate-command.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCliCommandAdded(this IRecorder recorder, PatternDefinition pattern, Automation command)
        {
            recorder.Count("pattern.cli-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCommandDeleted(this IRecorder recorder, PatternDefinition pattern, Automation command)
        {
            recorder.Count("pattern.command.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountLaunchPointAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.Count("pattern.launchpoint.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountLaunchPointUpdated(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.Count("pattern.launchpoint.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountLaunchPointDeleted(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.Count("pattern.launchpoint.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateTested(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate-tested", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateTestedWithImport(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate-tested.with-import", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void CountCodeTemplateTestedWithExport(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.Count("pattern.codetemplate-tested.with-export", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }
    }
}