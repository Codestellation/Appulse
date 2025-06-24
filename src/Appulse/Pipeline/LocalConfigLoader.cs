using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class LocalConfigLoader : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            var current = new DirectoryInfo(properties.ProjectDir);
            var searchPaths = new List<string>();
            try
            {
                do
                {
                    FileInfo editorConfigPath = current.EnumerateFiles(".editorconfig").FirstOrDefault();
                    if (editorConfigPath != null)
                    {
                        state.LocalEditorConfig = editorConfigPath.FullName;
                        state.LocalEditorConfigContent = File.ReadAllText(editorConfigPath.FullName);

                        return true;
                    }
                    
                    if (string.IsNullOrWhiteSpace(state.SolutionDir))
                    {
                        var solutionFiles = current.EnumerateFiles("*.sln").ToList();
                        if (solutionFiles.Any())
                        {
                            state.SolutionDir = current.FullName;
                        }
                    }
                    

                    searchPaths.Add(current.FullName);
                    current = current.Parent;
                } while (current != null && (Path.IsPathRooted(current.FullName) || current.EnumerateDirectories(".git").Any()));

                if (!properties.AppulseEditorConfigAutoUpdate)
                {
                    log.LogError($"Cannot find .editorconfig. Search path are '{string.Join(", ", searchPaths)}");
                    return false;
                }

                log.LogWarning(
                    $"Local.editorconfig was not found. Search path are '{string.Join(", ", searchPaths)}. Reference .editorconfig will be placed to '{state.SolutionDir}'.");

                state.LocalEditorConfig = Path.Combine(state.SolutionDir, ".editorconfig");
                state.LocalEditorConfigContent = string.Empty;
                return true;
            }
            catch (Exception ex)
            {
                ex.Data["CurrentDir"] = current?.FullName;
                ex.Data["Location"] = properties.AppulseLocalEditorConfig;
                throw;
            }
        }
    }
}