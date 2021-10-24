using System;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ConfigComparer : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            var separators = new[]
            {
                "\r\n",
                "\r",
                "\n"
            };

            string[] localLines = state.LocalEditorConfigContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();
            string[] referenceLines = state.ReferenceEditorConfigContent.Split(separators, StringSplitOptions.RemoveEmptyEntries).Select(x => x.Trim()).ToArray();

            if (localLines.Length != referenceLines.Length)
            {
                state.EditorConfigDifferenceDetails = $"Local has {localLines.Length} lines reference has {referenceLines.Length} lines";
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

                state.EditorConfigDifferenceDetails = $"Difference at line {lineIndex + 1}. Local is '{local}' reference is '{reference}'";
                //Won't fail task if auto update is set
                return properties.AppulseEditorConfigAutoUpdate;
            }

            log.LogError($"Reference .editorconfig differs from the local. {state.EditorConfigDifferenceDetails}. " +
                         $"Update '{state.LocalEditorConfig}' from '{properties.AppulseReferenceEditorConfig}')");

            return true;
        }
    }
}