using System;
using System.Collections.Generic;
using Codestellation.Appulse.Pipeline;
using Microsoft.Build.Utilities;

namespace Codestellation.Appulse
{
    public class EnsureEditorConfigTask : Task, IMsBuildProperties
    {
        private readonly TaskContext _context;
        private readonly IReadOnlyCollection<IPipelineElement> _pipeline;
        public string ProjectDir { get; set; }

        public string SolutionDir { get; set; }
        public string AppulseReferenceEditorConfig { get; set; }

        public string AppulseLocalEditorConfig { get; set; }

        public bool AppulseEditorConfigAutoUpdate { get; set; }

        public EnsureEditorConfigTask()
        {
            _context = new TaskContext();

            _pipeline = new List<IPipelineElement>
            {
                new ProjectDirValidator(),
                new LocalEditorConfigValidator(),
                new ReferenceEditorConfigValidator(),
                new CopyPropertiesElement(),
                new ReferenceConfigLoader(),
                new LocalConfigLoader(),
                new ConfigComparer(),
                new ConfigUpdater(),
            };
        }

        public override bool Execute()
        {
            try
            {
                return ExecutePipeline();
            }
            catch (Exception e)
            {
                Log.LogError($"Task failed: ProjectDir='{ProjectDir}'. " + e.Message);
                return false;
            }
        }

        private bool ExecutePipeline()
        {
            foreach (IPipelineElement element in _pipeline)
            {
                if (!element.Process(this, _context, Log))
                {
                    return false;
                }
            }

            return true;
        }
    }
}