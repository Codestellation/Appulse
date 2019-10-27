using System;
using System.Collections;
using Microsoft.Build.Framework;

namespace Codestellation.Appulse.Tests
{
    public class StubBuildEngine : IBuildEngine
    {
        public void LogErrorEvent(BuildErrorEventArgs e) => Console.WriteLine(e.Message);

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