using System.Collections.Generic;
using Automate.Authoring.Domain;
using Automate.Common;
using Automate.Common.Extensions;

namespace Automate.Authoring.Application
{
    public static class RecorderExtensions
    {
        public static void MeasurePatternsListed(this IRecorder recorder)
        {
            recorder.MeasureEvent("patterns.listed");
        }

        public static void MeasurePatternViewed(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.MeasureEvent("pattern.viewed",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void MeasurePatternSwitched(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.MeasureEvent("pattern.switched",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void MeasurePatternCreated(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.MeasureEvent("pattern.created",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void MeasurePatternUpdated(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.MeasureEvent("pattern.updated",
                new Dictionary<string, string>
                {
                    { "PatternId", pattern.Id.AnonymiseIdentifier() }
                });
        }

        public static void MeasureAttributeAdded(this IRecorder recorder, PatternDefinition pattern,
            Attribute attribute)
        {
            recorder.MeasureEvent("pattern.attribute.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureAttributeUpdated(this IRecorder recorder, PatternDefinition pattern,
            Attribute attribute)
        {
            recorder.MeasureEvent("pattern.attribute.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureAttributeDeleted(this IRecorder recorder, PatternDefinition pattern,
            Attribute attribute)
        {
            recorder.MeasureEvent("pattern.attribute.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "AttributeId", attribute.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureElementAdded(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.MeasureEvent("pattern.element.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureElementUpdated(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.MeasureEvent("pattern.element.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureElementDeleted(this IRecorder recorder, PatternDefinition pattern, Element element)
        {
            recorder.MeasureEvent("pattern.element.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "ElementId", element.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateAdded(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateWithCommandAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation command, CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate-with-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateEdited(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate.edited", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateDeleted(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplatesListed(this IRecorder recorder, PatternDefinition pattern)
        {
            recorder.MeasureEvent("pattern.codetemplates.listed", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateCommandAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation command)
        {
            recorder.MeasureEvent("pattern.codetemplate-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateCommandUpdated(this IRecorder recorder, PatternDefinition pattern,
            Automation command)
        {
            recorder.MeasureEvent("pattern.codetemplate-command.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCliCommandAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation command)
        {
            recorder.MeasureEvent("pattern.cli-command.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCommandDeleted(this IRecorder recorder, PatternDefinition pattern, Automation command)
        {
            recorder.MeasureEvent("pattern.command.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CommandId", command.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureLaunchPointAdded(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.MeasureEvent("pattern.launchpoint.added", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureLaunchPointUpdated(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.MeasureEvent("pattern.launchpoint.updated", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureLaunchPointDeleted(this IRecorder recorder, PatternDefinition pattern,
            Automation launchPoint)
        {
            recorder.MeasureEvent("pattern.launchpoint.deleted", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "LaunchPointId", launchPoint.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateTested(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate-tested", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateTestedWithImport(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate-tested.with-import", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }

        public static void MeasureCodeTemplateTestedWithExport(this IRecorder recorder, PatternDefinition pattern,
            CodeTemplate codeTemplate)
        {
            recorder.MeasureEvent("pattern.codetemplate-tested.with-export", new Dictionary<string, string>
            {
                { "PatternId", pattern.Id.AnonymiseIdentifier() },
                { "CodeTemplateId", codeTemplate.Id.AnonymiseIdentifier() }
            });
        }
    }
}