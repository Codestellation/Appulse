using System;
using System.Collections.Generic;

namespace Codestellation.Appulse
{
    internal class TaskContext
    {
        public static readonly ISet<string> SupportedSchemes = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "file",
            "http",
            "https"
        };
        public string EditorConfigDifferenceDetails { get; set; } = string.Empty;

        /// <summary>
        /// Is set from MsBuild property or located automatically
        /// </summary>
        public string LocalEditorConfig { get; set; }

        public string LocalEditorConfigContent { get; set; }
        public string ReferenceEditorConfigContent { get; set; } = string.Empty;

        /// <summary>
        /// Imported from MsBuild or uppermost with any .sln file
        /// </summary>
        public string SolutionDir { get; set; }
    }
}