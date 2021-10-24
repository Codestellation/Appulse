using System;
using System.IO;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ConfigUpdater : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            if (string.IsNullOrWhiteSpace(state.EditorConfigDifferenceDetails))
            {
                return true;
            }

            if (!properties.AppulseEditorConfigAutoUpdate)
            {
                return true;
            }

            try
            {
                File.WriteAllText(state.LocalEditorConfig, state.ReferenceEditorConfigContent);
                log.LogMessage($"Updated .editor config at '{state.LocalEditorConfig}'");
                return true;
            }
            catch (Exception e)
            {
                log.LogError($"Failed to update config at '{state.LocalEditorConfig}'. Reason: {e.Message}.");
                return false;
            }
        }
    }
}