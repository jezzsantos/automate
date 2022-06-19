﻿using Automate.Domain;

namespace Automate.Application
{
    public interface IPatternToolkitPackager
    {
        ToolkitPackage PackAndExport(PatternDefinition pattern, VersionInstruction instruction);

        ToolkitDefinition UnPack(IRuntimeMetadata metadata, IFile installer);
    }
}