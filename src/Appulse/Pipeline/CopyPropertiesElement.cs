using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class CopyPropertiesElement : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            state.ReferenceEditorConfigContent = properties.AppulseReferenceEditorConfig;
            state.SolutionDir = properties.SolutionDir;
            return true;
        }
    }
}