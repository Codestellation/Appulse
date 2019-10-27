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
        private string _sameEditorConfig;
        private StubBuildEngine _engine;
        private string _localEditorConfig;
        private string _differentEditorConfig;

        [OneTimeSetUp]
        public void Setup()
        {
            _engine = new StubBuildEngine();

            _sameEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_reference");
            File.WriteAllText(_sameEditorConfig, Resources.EditorConfigContent, Encoding.UTF8);

            _differentEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_different");
            File.WriteAllText(_differentEditorConfig, Resources.EditorConfigContent + "Symbols to change", Encoding.UTF8);

            _localEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig");
            File.WriteAllText(_localEditorConfig, Resources.EditorConfigContent, Encoding.UTF8);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(_localEditorConfig);
            File.Delete(_sameEditorConfig);
            File.Delete(_differentEditorConfig);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_reference_config_in_file()
        {
            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                ReferenceEditorConfig = new Uri(_sameEditorConfig).AbsoluteUri,
                SolutionDir = Directory.GetCurrentDirectory()
            };

            Assert.That(task.Execute(), Is.True);
        }

        [Test]
        public void Should_return_false_if_local_editor_config_differs()
        {
            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                ReferenceEditorConfig = new Uri(_differentEditorConfig).AbsoluteUri,
                SolutionDir = Directory.GetCurrentDirectory()
            };

            Assert.That(task.Execute(), Is.False);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_reference_config_on_web()
        {
            using (IWebHost host = Resources.CreateWebServer())
            {
                host.Start();
                var task = new EnsureEditorConfigTask
                {
                    BuildEngine = _engine,
                    ReferenceEditorConfig = new Uri($"http://localhost:{Resources.Port}").ToString(),
                    SolutionDir = Directory.GetCurrentDirectory()
                };

                Assert.That(task.Execute(), Is.True);
            }
        }
    }
}