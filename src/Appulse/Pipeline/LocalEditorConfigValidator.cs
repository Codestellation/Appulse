using System.IO;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class LocalEditorConfigValidator : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            string propertyValue = properties.AppulseLocalEditorConfig;
            string propertyName = nameof(properties.AppulseLocalEditorConfig);

            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                return true;
            }

            string directory = Path.GetDirectoryName(propertyValue);

            if (!Directory.Exists(directory))
            {
                log.LogError($"Path for {propertyName} set to '{propertyValue}' but the folder does not exist.");
                return false;
            }

            return true;
        }
    }
}