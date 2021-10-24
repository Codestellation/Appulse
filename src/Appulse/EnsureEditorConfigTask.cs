using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse
{
    public class EnsureEditorConfigTask : Task
    {
        private readonly AppulseTaskContext _context = new AppulseTaskContext();
        private MsBuildProperties MsBuildProperties => _context.MsBuildProperties;
        private TaskState State => _context.TaskState;

        public string ProjectDir
        {
            get => MsBuildProperties.ProjectDir;
            set => MsBuildProperties.ProjectDir = value;
        }

        public string AppulseReferenceEditorConfig
        {
            get => MsBuildProperties.AppulseReferenceEditorConfig;
            set => MsBuildProperties.AppulseReferenceEditorConfig = value;
        }

        public string AppulseLocalEditorConfig
        {
            get => MsBuildProperties.AppulseLocalEditorConfig;
            set => MsBuildProperties.AppulseLocalEditorConfig = value;
        }

        public bool AppulseEditorConfigAutoUpdate
        {
            get => MsBuildProperties.AppulseEditorConfigAutoUpdate;
            set => MsBuildProperties.AppulseEditorConfigAutoUpdate = value;
        }

        public override bool Execute()
        {
            try
            {
                State.ReferenceEditorConfigContent = MsBuildProperties.AppulseReferenceEditorConfig;
                return TryExecuteTask();
            }
            catch (Exception e)
            {
                Log.LogError($"Task failed: ProjectDir='{ProjectDir}'. " + e.Message);
                throw;
            }
        }

        private bool TryExecuteTask()
        {
            if (!ValidateInput())
            {
                return false;
            }

            if (!TryLoadReferenceEditorConfig())
            {
                return false;
            }


            if (TryLoadLocalEditorConfig() && CompareLineByLine())
            {
                return true;
            }


            if (MsBuildProperties.AppulseEditorConfigAutoUpdate)
            {
                TryUpdateLocalConfig();
                return true;
            }


            Log.LogError($"Reference .editorconfig differs from the local. {State.EditorConfigDifferenceDetails}. " +
                         $"Update '{MsBuildProperties.AppulseLocalEditorConfig}' from '{AppulseReferenceEditorConfig}')");
            return false;
        }

        private void TryUpdateLocalConfig()
        {
            try
            {
                File.WriteAllText(State.LocalEditorConfig, State.ReferenceEditorConfigContent);
            }
            catch (Exception e)
            {
                e.Data["location"] = AppulseLocalEditorConfig;
                throw;
            }
        }

        private bool CompareLineByLine()
        {
            var separators = new[]
            {
                "\r\n",
                "\r",
                "\n"
            };

            string[] localLines = State.LocalEditorConfigContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            string[] referenceLines = State.ReferenceEditorConfigContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (localLines.Length != referenceLines.Length)
            {
                State.EditorConfigDifferenceDetails = $"Local has {localLines.Length} lines reference has {referenceLines.Length} lines";
                return false;
            }


            for (var lineIndex = 0; lineIndex < localLines.Length; lineIndex++)
            {
                string local = localLines[lineIndex];
                string reference = referenceLines[lineIndex];
                if (string.Equals(local, reference, StringComparison.Ordinal))
                {
                    continue;
                }

                State.EditorConfigDifferenceDetails = $"Difference at line {lineIndex + 1}. Local is '{local}' reference is '{reference}'";
                return false;
            }

            return true;
        }

        private bool TryLoadReferenceEditorConfig()
        {
            try
            {
                var uri = new Uri(AppulseReferenceEditorConfig);
                //It can handle file scheme also
                var request = WebRequest.CreateDefault(uri);
                using (var streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    State.ReferenceEditorConfigContent = streamReader.ReadToEnd();
                }

                Log.LogMessage($"Reference .editorconfig was loaded successfully from '{uri}'");

                return true;
            }
            catch (WebException e)
            {
                Log.LogError($"Cannot load reference .editorconfig: {e.Message}");
                return false;
            }
        }


        private bool TryLoadLocalEditorConfig()
        {
            var current = new DirectoryInfo(ProjectDir);
            var searchPaths = new List<string>();
            try
            {
                do
                {
                    FileInfo editorConfigPath = current.EnumerateFiles(".editorconfig").FirstOrDefault();
                    if (editorConfigPath != null)
                    {
                        State.LocalEditorConfig = editorConfigPath.FullName;
                        State.LocalEditorConfigContent = File.ReadAllText(editorConfigPath.FullName);
                        return true;
                    }

                    searchPaths.Add(current.FullName);
                    current = current.Parent;
                } while (current != null && (Path.IsPathRooted(current.FullName) || current.EnumerateDirectories(".git").Any()));

                Log.LogError($"Cannot find .editorconfig. Search path are '{string.Join(", ", searchPaths)}");
                return false;
            }
            catch (Exception ex)
            {
                ex.Data["CurrentDir"] = current?.FullName;
                ex.Data["Location"] = MsBuildProperties.AppulseLocalEditorConfig;
                throw;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(ProjectDir))
            {
                Log.LogError("ProjectDir is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(AppulseReferenceEditorConfig))
            {
                Log.LogError($"{nameof(AppulseReferenceEditorConfig)} is not set. " +
                             $"Please set property {nameof(AppulseReferenceEditorConfig)} to point to a file or web link with the source .editorconfig file. " +
                             "Supported schemes are file:// and http://");
                return false;
            }

            if (!Uri.IsWellFormedUriString(AppulseReferenceEditorConfig, UriKind.RelativeOrAbsolute))
            {
                Log.LogError($"Expected {nameof(AppulseReferenceEditorConfig)} to be a well formed URI with path or http(s) scheme but found '{AppulseReferenceEditorConfig}'");
                return false;
            }

            var uri = new Uri(AppulseReferenceEditorConfig);
            if (!(string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)))
            {
                Log.LogError($"Expected {nameof(AppulseReferenceEditorConfig)} to be a well formed URI with path or http(s) scheme but found '{AppulseReferenceEditorConfig}'");
                return false;
            }

            return true;
        }
    }
}