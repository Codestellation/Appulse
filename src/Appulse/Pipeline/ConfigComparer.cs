using System;
using System.Linq;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ConfigComparer : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            if (!TryFindDifference(state, out string difference))
            {
                return true;
            }

            state.EditorConfigDifferenceDetails = difference;

            if (properties.AppulseEditorConfigAutoUpdate)
            {
                return true;
            }

            log.LogError($"Reference .editorconfig differs from the local. {state.EditorConfigDifferenceDetails}. " +
                         $"Update '{state.LocalEditorConfig}' from '{properties.AppulseReferenceEditorConfig}')");

            return false;
        }

        private static bool TryFindDifference(TaskContext state, out string difference)
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
                difference = $"Local has {localLines.Length} lines reference has {referenceLines.Length} lines";
                return true;
            }

            for (var lineIndex = 0; lineIndex < localLines.Length; lineIndex++)
            {
                string local = localLines[lineIndex];
                string reference = referenceLines[lineIndex];
                if (string.Equals(local, reference, StringComparison.Ordinal))
                {
                    continue;
                }

                difference = $"Difference at line {lineIndex + 1}. Local is '{local}' reference is '{reference}'";
                return true;
            }

            difference = null;
            return false;
        }
    }
}