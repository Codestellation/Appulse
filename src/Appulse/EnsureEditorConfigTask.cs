using System;
using System.IO;
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

            if (!TryLoadLocalEditorConfig(out string localContent))
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

            Log.LogError("Reference .editorconfig differs from local .editorconfig");
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

        private bool TryLoadLocalEditorConfig(out string localContent)
        {
            string path1 = Path.Combine(SolutionDir, ".editorconfig");

            if (File.Exists(path1))
            {
                Log.LogMessage($"Found local editor config at '{path1}'");
                localContent = File.ReadAllText(path1);
                return true;
            }

            string path2 = Path.Combine(Directory.GetParent(SolutionDir).FullName, ".editorconfig");
            if (File.Exists(path2))
            {
                Log.LogMessage($"Found local editor config at '{path2}'");
                localContent = File.ReadAllText(path2);
                return true;
            }

            Log.LogError($"Cannot find editor config. Search path are '{path1}', '{path2}'");
            localContent = null;
            return false;
        }

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
                             $"Please set property {nameof(ReferenceEditorConfig)} to point to a file or web link with the source editor config file. Supported ");
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