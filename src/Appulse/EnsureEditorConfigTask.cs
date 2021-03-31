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

            if (string.Equals(localContent, referenceContent, StringComparison.Ordinal))
            {
                Log.LogMessage("Reference .editorconfig is the same as local one.");
                return true;
            }

            string details = CompareDetailed(localContent, referenceContent);

            Log.LogError($"Reference .editorconfig differs from the local. {details}. " +
                         $"Update '{localLocation}' from '{ReferenceEditorConfig}')");
            return false;
        }

        private string CompareDetailed(string localContent, string referenceContent)
        {
            string[] localLines = localContent.Split('\n');
            string[] referenceLines = referenceContent.Split('\n');

            if (localLines.Length != referenceLines.Length)
            {
                return $"Local has {localLines.Length} lines reference has {referenceLines.Length} lines";
            }


            for (var lineIndex = 0; lineIndex < localLines.Length; lineIndex++)
            {
                string local = localLines[lineIndex];
                string reference = referenceLines[lineIndex];
                if (string.Equals(local, reference, StringComparison.Ordinal))
                {
                    continue;
                }

                return $"Difference at line {lineIndex + 1}. Local is '{local}' reference is '{reference}'";
            }

            throw new InvalidOperationException("This should never happen");
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
                    referenceContent = Normalize(streamReader.ReadToEnd());
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
                        localContent = Normalize(File.ReadAllText(editorConfigPath.FullName));
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

        private static string Normalize(string source) => source.Replace("\r", string.Empty);

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