using System;
using System.IO;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ConfigUpdater : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            bool filesAreEqual = string.IsNullOrWhiteSpace(state.EditorConfigDifferenceDetails);
            if (filesAreEqual)
            {
                return true;
            }

            bool updateForbidden = !properties.AppulseEditorConfigAutoUpdate;
            if (updateForbidden)
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