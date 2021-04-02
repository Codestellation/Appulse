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
        public string ProjectDir { get; set; }

        public string ReferenceEditorConfig { get; set; }

        public override bool Execute()
        {
            try
            {
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
            if (!VaildateInput())
            {
                return false;
            }

            if (!TryLoadLocalEditorConfig(out string localContent, out string localLocation))
            {
                return false;
            }

            if (!TryLoadReferenceEditorConfig(out string referenceContent))
            {
                return false;
            }

            if (CompareLineByLine(localContent, referenceContent, out string details))
            {
                return true;
            }
            {
                Log.LogMessage("Reference .editorconfig is the same as local one.");
                return true;
            }


            Log.LogError($"Reference .editorconfig differs from the local. {details}. " +
                         $"Update '{localLocation}' from '{ReferenceEditorConfig}')");
            return false;
        }

        private bool CompareLineByLine(string localContent, string referenceContent, out string errorMessage)
        {
            var separators = new[]
            {
                "\r\n",
                "\r",
                "\n"
            };

            string[] localLines = localContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            string[] referenceLines = referenceContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (localLines.Length != referenceLines.Length)
            {
                errorMessage = $"Local has {localLines.Length} lines reference has {referenceLines.Length} lines";
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

                errorMessage = $"Difference at line {lineIndex + 1}. Local is '{local}' reference is '{reference}'";
                return false;
            }

            errorMessage = string.Empty;
            return true;
        }

        private bool TryLoadReferenceEditorConfig(out string referenceContent)
        {
            try
            {
                var uri = new Uri(ReferenceEditorConfig);
                //It can handle file scheme also
                WebRequest request = WebRequest.CreateDefault(uri);
                using (var streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    referenceContent = streamReader.ReadToEnd();
                }

                Log.LogMessage($"Reference .editorconfig was loaded successfully from '{uri}'");

                return true;
            }
            catch (WebException e)
            {
                Log.LogError($"Cannot load reference .editorconfig: {e.Message}");
                referenceContent = null;
                return false;
            }
        }


        private bool TryLoadLocalEditorConfig(out string localContent, out string location)
        {
            var current = new DirectoryInfo(ProjectDir);
            var searchPaths = new List<string>();
            location = null;
            try
            {
                do
                {
                    FileInfo editorConfigPath = current.EnumerateFiles(".editorconfig").FirstOrDefault();
                    if (editorConfigPath != null)
                    {
                        localContent = File.ReadAllText(editorConfigPath.FullName);
                        location = editorConfigPath.FullName;
                        return true;
                    }

                    searchPaths.Add(current.FullName);
                    current = current.Parent;
                } while (current != null && Path.IsPathRooted(current.FullName) || current.EnumerateDirectories(".git").Any());

                Log.LogError($"Cannot find .editorconfig. Search path are '{string.Join(", ", searchPaths)}");
                localContent = null;
                location = null;
                return false;
            }
            catch (Exception ex)
            {
                ex.Data["CurrentDir"] = current?.FullName;
                ex.Data["Location"] = location;
                throw;
            }
        }

        private bool VaildateInput()
        {
            if (string.IsNullOrWhiteSpace(ProjectDir))
            {
                Log.LogError("ProjectDir is empty");
                return false;
            }

            if (string.IsNullOrWhiteSpace(ReferenceEditorConfig))
            {
                Log.LogError($"{nameof(ReferenceEditorConfig)} is not set. " +
                             $"Please set property {nameof(ReferenceEditorConfig)} to point to a file or web link with the source .editorconfig file. " +
                             "Supported schemes are file:// and http://");
                return false;
            }

            if (!Uri.IsWellFormedUriString(ReferenceEditorConfig, UriKind.RelativeOrAbsolute))
            {
                Log.LogError($"Expected {nameof(ReferenceEditorConfig)} to be a well formed URI with path or http(s) scheme but found '{ReferenceEditorConfig}'");
                return false;
            }

            var uri = new Uri(ReferenceEditorConfig);
            if (!(string.Equals(uri.Scheme, "file", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(uri.Scheme, "http", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(uri.Scheme, "https", StringComparison.OrdinalIgnoreCase)))
            {
                Log.LogError($"Expected {nameof(ReferenceEditorConfig)} to be a well formed URI with path or http(s) scheme but found '{ReferenceEditorConfig}'");
                return false;
            }

            return true;
        }
    }
}