namespace Codestellation.Appulse
{
    internal class TaskState
    {
        public string EditorConfigDifferenceDetails { get; set; } = string.Empty;

        /// <summary>
        /// Is set from MsBuild property or located automatically
        /// </summary>
        public string LocalEditorConfig { get; set; }

        public string LocalEditorConfigContent { get; set; }
        public string ReferenceEditorConfigContent { get; set; } = string.Empty;
    }
}