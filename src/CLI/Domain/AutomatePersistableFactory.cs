using System;

namespace Automate.CLI.Domain
{
    internal class AutomatePersistableFactory : IPersistableFactory
    {
        public IPersistable Rehydrate<TPersistable>(PersistableProperties properties)
            where TPersistable : IPersistable
        {
            return RehydrateInternal(typeof(TPersistable), properties);
        }

        public IPersistable Rehydrate(Type persistableType, PersistableProperties properties)
        {
            if (!persistableType.IsAssignableTo(typeof(IPersistable)))
            {
                throw new NotImplementedException();
            }

            return RehydrateInternal(persistableType, properties);
        }

        private IPersistable RehydrateInternal(Type persistableType, PersistableProperties properties)
        {
            if (persistableType == typeof(PatternDefinition))
            {
                return PatternDefinition.Rehydrate(properties, this);
            }
            if (persistableType == typeof(ToolkitDefinition))
            {
                return ToolkitDefinition.Rehydrate(properties, this);
            }
            if (persistableType == typeof(SolutionDefinition))
            {
                return SolutionDefinition.Rehydrate(properties, this);
            }
            if (persistableType == typeof(Element))
            {
                return Element.Rehydrate(properties, this);
            }
            if (persistableType == typeof(Attribute))
            {
                return Attribute.Rehydrate(properties, this);
            }
            if (persistableType == typeof(Automation))
            {
                return Automation.Rehydrate(properties, this);
            }
            if (persistableType == typeof(CodeTemplate))
            {
                return CodeTemplate.Rehydrate(properties, this);
            }
            if (persistableType == typeof(CodeTemplateFile))
            {
                return CodeTemplateFile.Rehydrate(properties, this);
            }
            if (persistableType == typeof(ArtifactLink))
            {
                return ArtifactLink.Rehydrate(properties, this);
            }
            if (persistableType == typeof(SolutionItem))
            {
                return SolutionItem.Rehydrate(properties, this);
            }
            if (persistableType == typeof(LocalState))
            {
                return LocalState.Rehydrate(properties, this);
            }
            if (persistableType == typeof(ToolkitVersion))
            {
                return ToolkitVersion.Rehydrate(properties, this);
            }
            if (persistableType == typeof(VersionChangeLog))
            {
                return VersionChangeLog.Rehydrate(properties, this);
            }

            throw new AutomateException($"Tried to Rehydrate an unknown persistable type '{persistableType}'");
        }
    }
}