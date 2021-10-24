using System;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ReferenceEditorConfigValidator : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            string propertyValue = properties.AppulseReferenceEditorConfig;
            string propertyName = nameof(properties.AppulseReferenceEditorConfig);

            if (string.IsNullOrWhiteSpace(propertyValue))
            {
                log.LogError($"Please set property {propertyName} to point to a file or web link with the source .editorconfig file. " +
                             $"Supported schemes are {string.Join(", ", TaskContext.SupportedSchemes.OrderBy(x => x))}");
                return false;
            }

            if (!Uri.IsWellFormedUriString(propertyValue, UriKind.RelativeOrAbsolute))
            {
                log.LogError($"Expected {propertyName} to be a well formed URI with path or http(s) scheme but found '{propertyValue}'");
                return false;
            }

            var uri = new Uri(propertyValue);
            if (!TaskContext.SupportedSchemes.Contains(uri.Scheme))
            {
                log.LogError($"Expected {propertyName} to be a well formed URI with path or http(s) scheme but found '{propertyValue}'");
                return false;
            }

            return true;
        }


    }
}