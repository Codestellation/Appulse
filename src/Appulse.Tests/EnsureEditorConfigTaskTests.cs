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

        [SetUp]
        public void Setup()
        {
            _engine = new StubBuildEngine();

            _sameEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_reference");
            File.WriteAllText(_sameEditorConfig, Resources.EditorConfigContent);

            _differentEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig_different");
            File.WriteAllText(_differentEditorConfig, Resources.EditorConfigContent + "Symbols to change");

            _localEditorConfig = Path.Combine(Directory.GetCurrentDirectory(), ".editorconfig");
            File.WriteAllText(_localEditorConfig, Resources.EditorConfigContent);
        }

        [TearDown]
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
                AppulseReferenceEditorConfig = new Uri(_sameEditorConfig).AbsoluteUri,
                ProjectDir = Directory.GetCurrentDirectory(),
                AppulseEditorConfigAutoUpdate = false
            };

            Assert.That(task.Execute(), Is.True);
        }

        [Test]
        public void Should_return_true_if_local_editor_config_matches_reference_config_in_a_parent_folder_file()
        {
            string parentPath = new FileInfo(_localEditorConfig).Directory.Parent.FullName;
            string destFileName = Path.Combine(parentPath, ".editorconfig");
            if (File.Exists(destFileName))
            {
                File.Delete(destFileName);
            }

            File.Move(_localEditorConfig, destFileName);


            var task = new EnsureEditorConfigTask
            {
                BuildEngine = _engine,
                AppulseReferenceEditorConfig = new Uri(_sameEditorConfig).AbsoluteUri,
                ProjectDir = Directory.GetCurrentDirectory(),
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
                ProjectDir = Directory.GetCurrentDirectory(),
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
                    ProjectDir = Directory.GetCurrentDirectory(),
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
                ProjectDir = Directory.GetCurrentDirectory(),
                AppulseEditorConfigAutoUpdate = true
            };

            Assert.That(task.Execute(), Is.True, string.Join(Environment.NewLine, _engine.Errors));

            string localContent = File.ReadAllText(_localEditorConfig);
            string referenceContent = File.ReadAllText(_differentEditorConfig);

            Assert.That(localContent, Is.EqualTo(referenceContent));
        }
    }
}