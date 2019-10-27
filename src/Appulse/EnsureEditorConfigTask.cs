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
        public string SolutionDir { get; set; }

        public string ReferenceEditorConfig { get; set; }

        public override bool Execute()
        {
            if (!VaildateInpute())
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

            Log.LogError($"Reference .editorconfig differs from the local. Update '{localLocation}' from '{ReferenceEditorConfig}')");
            return false;
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
            var current = new DirectoryInfo(SolutionDir);
            var searchedPathes = new List<string>();
            do
            {
                FileInfo editorConfigPath = current.EnumerateFiles(".editorconfig").FirstOrDefault();
                if (editorConfigPath != null)
                {
                    localContent = Normalize(File.ReadAllText(editorConfigPath.FullName));
                    location = editorConfigPath.FullName;
                    return true;
                }

                searchedPathes.Add(current.FullName);
                current = current.Parent;
            } while (current != null && Path.IsPathRooted(current.FullName) || current.EnumerateDirectories(".git").Any());

            Log.LogError($"Cannot find .editorconfig. Search path are '{string.Join(", ", searchedPathes)}");
            localContent = null;
            location = null;
            return false;
        }

        private static string Normalize(string source) => source.Replace("\r", string.Empty);

        private bool VaildateInpute()
        {
            if (string.IsNullOrWhiteSpace(SolutionDir))
            {
                Log.LogError("Solution dir is empty");
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