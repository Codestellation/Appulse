using Microsoft.Build.Utilities;

namespace Codestellation.Appulse
{
    internal interface IPipelineElement
    {
        bool Process(IMsBuildProperties properties, TaskContext state, TaskLoggingHelper log);
    }
}