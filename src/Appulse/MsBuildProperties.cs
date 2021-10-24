namespace Codestellation.Appulse
{
    internal class MsBuildProperties
    {
        public string ProjectDir { get; set; }
        public string AppulseReferenceEditorConfig { get; set; }
        public string AppulseLocalEditorConfig { get; set; }
        public bool AppulseEditorConfigAutoUpdate { get; set; } = true;
    }
}