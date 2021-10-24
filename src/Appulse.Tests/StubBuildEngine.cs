using System;
using System.Collections;
using System.Collections.Generic;
using Microsoft.Build.Framework;

namespace Codestellation.Appulse.Tests
{
    public class StubBuildEngine : IBuildEngine
    {
        private readonly List<string> _errors = new List<string>();
        public IReadOnlyCollection<string> Errors => _errors;
        public void LogErrorEvent(BuildErrorEventArgs e) => _errors.Add(e.Message);

        public void LogWarningEvent(BuildWarningEventArgs e) => Console.WriteLine(e.Message);

        public void LogMessageEvent(BuildMessageEventArgs e) => Console.WriteLine(e.Message);

        public void LogCustomEvent(CustomBuildEventArgs e) => Console.WriteLine(e.Message);

        public bool BuildProjectFile(string projectFileName, string[] targetNames, IDictionary globalProperties, IDictionary targetOutputs) => throw new NotImplementedException();

        public bool ContinueOnError => false;

        public int LineNumberOfTaskNode => 0;

        public int ColumnNumberOfTaskNode => 0;

        public string ProjectFileOfTaskNode => Guid.NewGuid().ToString();
    }
}