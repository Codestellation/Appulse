using System;
using System.IO;
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
        private string _solutionDir;


        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            _sameEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_reference");
            File.WriteAllText(_sameEditorConfig, Resources.EditorConfigContent);
            _differentEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_different");
            File.WriteAllText(_differentEditorConfig, Resources.EditorConfigContent + "Symbols to change");

            _solutionDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("D"));
            Directory.CreateDirectory(_solutionDir);
            _localEditorConfig = Path.Combine(_solutionDir, ".editorconfig");
        }

        [SetUp]
        public void Setup()
        {
            _engine = new StubBuildEngine();
            File.WriteAllText(_localEditorConfig, Resources.EditorConfigContent);
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            File.Delete(_localEditorConfig);
            File.Delete(_sameEditorConfig);
            File.Delete(_differentEditorConfig);
            Directory.Delete(_solutionDir, true);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_reference_config_in_file()
        {
            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_sameEditorConfig).AbsoluteUri,
                ProjectDir = _solutionDir,
                AppulseEditorConfigAutoUpdate = false
            };

            Assert.That(task.Execute(), Is.True);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_reference_config_in_a_parent_folder_file()
        {
            var subDirectory = Path.Combine(_solutionDir, "project");
            Directory.CreateDirectory(subDirectory);

            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_sameEditorConfig).AbsoluteUri,
                ProjectDir = subDirectory,
                AppulseEditorConfigAutoUpdate = false
            };

            Assert.That(task.Execute(), Is.True);
        }

        [Test]
        public void Should_return_false_if_local_editor_config_differs()
        {
            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_differentEditorConfig).AbsoluteUri,
                ProjectDir = _solutionDir,
                AppulseEditorConfigAutoUpdate = false
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
                    AppulseReferenceEditorConfig = new Uri($"http://localhost:{Resources.Port}").ToString(),
                    ProjectDir = _solutionDir,
                    AppulseEditorConfigAutoUpdate = false
                };

                Assert.That(task.Execute(), Is.True);
            }
        }

        [Test]
        public void Should_auto_update_local_config()
        {
            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_differentEditorConfig).AbsoluteUri,
                ProjectDir = _solutionDir,
                AppulseEditorConfigAutoUpdate = true
            };

            Assert.That(task.Execute(), Is.True, string.Join(Environment.NewLine, _engine.Errors));

            string localContent = File.ReadAllText(_localEditorConfig);
            string referenceContent = File.ReadAllText(_differentEditorConfig);

            Assert.That(localContent, Is.EqualTo(referenceContent));
        }

        [Test]
        public void Should_download_config_to_solution_folder()
        {
            File.Delete(_localEditorConfig);

            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_differentEditorConfig).AbsoluteUri,
                ProjectDir = _solutionDir,
                SolutionDir = _solutionDir,
                AppulseEditorConfigAutoUpdate = true
            };

            Assert.That(task.Execute(), Is.True, string.Join(Environment.NewLine, _engine.Errors));
            FileAssert.Exists(_localEditorConfig);

            string localContent = File.ReadAllText(_localEditorConfig);
            string referenceContent = File.ReadAllText(_differentEditorConfig);

            Assert.That(localContent, Is.EqualTo(referenceContent));
        }
    }
}