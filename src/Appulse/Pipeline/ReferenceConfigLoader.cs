using System;
using System.IO;
using System.Net;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ReferenceConfigLoader : IPipelineElement
    {
        private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(5);

        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            try
            {
                var uri = new Uri(properties.AppulseReferenceEditorConfig);
                //It can handle file scheme also
                var request = WebRequest.CreateDefault(uri);
                request.Timeout = (int)Timeout.TotalMilliseconds;
                using (var streamReader = new StreamReader(request.GetResponse().GetResponseStream()))
                {
                    state.ReferenceEditorConfigContent = streamReader.ReadToEnd();
                }

                log.LogMessage($"Reference .editorconfig was loaded successfully from '{uri}'");

                return true;
            }
            catch (WebException e)
            {
                log.LogError($"Cannot load reference .editorconfig: {e.Message}");
                return false;
            }
        }
    }
}