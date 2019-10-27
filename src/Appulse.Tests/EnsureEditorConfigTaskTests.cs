using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Hosting;
using NUnit.Framework;

namespace Codestellation.Appulse.Tests
{
    [TestFixture]
    public class EnsureEditorConfigTaskTests
    {
        [Test]
        public void Should_return_true_if_local_editor_config_matches()
        {
            var engine = new StubBuildEngine();
            string referenceEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig");
            File.WriteAllText(referenceEditorConfig, Resources.EditorConfigContent, Encoding.UTF8);


            var task = new EnsureEditorConfigTask
            {
                BuildEngine = engine,
                ReferenceEditorConfig = new Uri(referenceEditorConfig).AbsoluteUri,
                SolutionDir = Directory.GetCurrentDirectory()
            };

            Assert.That(task.Execute(), Is.True);
        }

        [Test]
        public void Should_return_false_if_local_editor_config_differs()
        {
            var engine = new StubBuildEngine();

            string referenceEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_reference");
            File.WriteAllText(referenceEditorConfig, Resources.EditorConfigContent + "Symbols to make difference", Encoding.UTF8);

            string localEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig");
            File.WriteAllText(localEditorConfig, Resources.EditorConfigContent, Encoding.UTF8);


            var task = new EnsureEditorConfigTask
            {
                BuildEngine = engine,
                ReferenceEditorConfig = new Uri(referenceEditorConfig).AbsoluteUri,
                SolutionDir = Directory.GetCurrentDirectory()
            };

            Assert.That(task.Execute(), Is.False);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_remote()
        {
            var engine = new StubBuildEngine();
            string localEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig");
            File.WriteAllText(localEditorConfig, Resources.EditorConfigContent, Encoding.UTF8);

            using (IWebHost host = Resources.CreateWebServer())
            {
                host.Start();
                var task = new EnsureEditorConfigTask
                {
                    BuildEngine = engine,
                    ReferenceEditorConfig = new Uri($"http://localhost:{Resources.Port}").ToString(),
                    SolutionDir = Directory.GetCurrentDirectory()
                };

                Assert.That(task.Execute(), Is.True);
            }
        }
    }
}