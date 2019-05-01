﻿using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace Incrementalist.ProjectSystem
{
    public struct SlnFile
    {
        public SlnFile(FileType fileType, ProjectId projectId)
        {
            FileType = fileType;
            ProjectId = projectId;
        }

        public FileType FileType { get; }

        /// <summary>
        /// The ID of the project to which this file belongs.
        ///
        /// Used in the topological sorting of dependencies later.
        /// </summary>
        /// <remarks>
        /// Will be <c>null</c> when <see cref="FileType"/> is Solution file.
        /// </remarks>
        public ProjectId ProjectId { get; }
    }

    /// <summary>
    /// Analyzes MSBuild solutions using the Roslyn Workspaces API
    /// </summary>
    public static class SolutionAnalyzer
    {
        /// <summary>
        /// Produces a flat, unique list of all files in the solution, including .csproj and .sln files.
        /// </summary>
        /// <param name="sln">The Solution file.</param>
        /// <param name="workingFolder"></param>
        /// <returns>A flattened list of all files inside the solution.</returns>
        public static Dictionary<string, SlnFile> AllSolutionFiles(Solution sln, string workingFolder)
        {
            return sln.Projects.SelectMany(x => x.Documents)
                .GroupBy(x => x.FilePath, document => new SlnFile(document.SourceCodeKind == SourceCodeKind.Regular ? FileType.Code : FileType.Script, document.Project.Id))
                .ToDictionary(x => Path.GetFullPath(x.Key), x => x.First())
                .Concat(sln.Projects.ToDictionary(x => Path.GetFullPath(x.FilePath), x => new SlnFile(FileType.Project, x.Id)))
                .Concat(new Dictionary<string, SlnFile> { { Path.GetFullPath(sln.FilePath), new SlnFile(FileType.Solution, null) } })
                .ToDictionary(x => x.Key, x => x.Value);
        }
    }
}
