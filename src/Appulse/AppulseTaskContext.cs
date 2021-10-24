namespace Codestellation.Appulse
{
    internal class AppulseTaskContext
    {
        public MsBuildProperties MsBuildProperties { get; } = new MsBuildProperties();
        public TaskState TaskState { get; } = new TaskState();
    }
}