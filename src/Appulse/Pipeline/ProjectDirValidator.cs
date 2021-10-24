using Microsoft.Build.Utilities;

namespace Codestellation.Appulse.Pipeline
{
    internal class ProjectDirValidator : IPipelineElement
    {
        public bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log)
        {
            if (!string.IsNullOrWhiteSpace(properties.ProjectDir))
            {
                return true;
            }

            log.LogError("ProjectDir is empty");
            return false;
        }
    }
}