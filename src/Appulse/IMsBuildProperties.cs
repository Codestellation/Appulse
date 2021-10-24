namespace Codestellation.Appulse
{
    internal interface IMsBuildProperties
    {
        string ProjectDir { get; }
        string SolutionDir { get; }
        string AppulseReferenceEditorConfig { get; }
        string AppulseLocalEditorConfig { get; }
        bool AppulseEditorConfigAutoUpdate { get; }
    }
}