using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LibGit2Sharp.Tests.TestHelpers;
using Xunit;

namespace LibGit2Sharp.Tests
{
    public class ConfigurationBackendFixture : BaseFixture
    {
        [Fact]
        public void CanCreateInMemoryRepositoryWithBackend()
        {
            var options = new RepositoryOptions
            {
                ConfigurationBackends = new Dictionary<ConfigurationLevel, ConfigurationBackend> { { ConfigurationLevel.Local, new MockConfigurationBackend() } }
            };
            using (var repo = new Repository(options))
            {
                // repo.Config.AddBackend(new MockConfigurationBackend(), ConfigurationLevel.Local);

                Assert.True(repo.Info.IsBare);
                Assert.Null(repo.Info.Path);
                Assert.Null(repo.Info.WorkingDirectory);

                Assert.Throws<BareRepositoryException>(() => { var idx = repo.Index; });
            }
        }

        [Fact]
        public void CanUnsetAnEntryFromTheLocalConfiguration()
        {
            string path = SandboxStandardTestRepo();

            var options = new RepositoryOptions
            {
                ConfigurationBackends = new Dictionary<ConfigurationLevel, ConfigurationBackend> { { ConfigurationLevel.Local, new MockConfigurationBackend() } }
            };

            using (var repo = new Repository(path, options))
            {
                Assert.Null(repo.Config.Get<bool>("unittests.boolsetting"));

                repo.Config.Set("unittests.boolsetting", true);
                Assert.True(repo.Config.Get<bool>("unittests.boolsetting").Value);

                repo.Config.Unset("unittests.boolsetting");

                Assert.Null(repo.Config.Get<bool>("unittests.boolsetting"));
            }
        }

        [Fact]
        public void CanUnsetAnEntryFromTheGlobalConfiguration()
        {
            string path = SandboxBareTestRepo();

            var options = new RepositoryOptions
            {
                ConfigurationBackends = new Dictionary<ConfigurationLevel, ConfigurationBackend> { { ConfigurationLevel.Global, new MockConfigurationBackend() } }
            };

            using (var repo = new Repository(path, options))
            {
                Assert.True(repo.Config.HasConfig(ConfigurationLevel.Global));
                Assert.Equal(42, repo.Config.Get<int>("Wow.Man-I-am-totally-global").Value);

                repo.Config.Unset("Wow.Man-I-am-totally-global");
                Assert.Equal(42, repo.Config.Get<int>("Wow.Man-I-am-totally-global").Value);

                repo.Config.Unset("Wow.Man-I-am-totally-global", ConfigurationLevel.Global);
                Assert.Null(repo.Config.Get<int>("Wow.Man-I-am-totally-global"));
            }
        }

        #region MockBackend

        private class MockConfigurationBackend : ConfigurationBackend<string, ConfigurationIterator<string>>, IDisposable
        {
            private ConfigurationLevel _currentConfigurationLevel;

            private readonly Dictionary<string, string> config;

            public MockConfigurationBackend()
            {
                this.isReadOnly = false;
                this.config = new Dictionary<string, string>();
            }

            public MockConfigurationBackend(Dictionary<string, string> config)
            {
                this.isReadOnly = true;
                this.config = config;
            }

            protected override ConfigBackendOperations SupportedOperations
            {
                get {
                    return ConfigBackendOperations.Get |
                           ConfigBackendOperations.Set |
                           ConfigBackendOperations.SetMultivar |
                           ConfigBackendOperations.Del |
                           ConfigBackendOperations.DelMultivar |
                           ConfigBackendOperations.Iterator |
                           ConfigBackendOperations.Snapshot | // Looks fairly required
                           ConfigBackendOperations.Lock |
                           ConfigBackendOperations.Unlock;
                }
            }

            public override int Open(ConfigurationLevel level)
            {
                _currentConfigurationLevel = level;
                return 0;
            }

            public override int Get(string key, out ConfigurationEntry<string> configurationEntry)
            {
                if (!this.config.ContainsKey(key))
                {
                    configurationEntry = null;
                    return (int)ReturnCode.GIT_ENOTFOUND;
                }
                configurationEntry = new MockConfigurationEntry(key, this.config[key], _currentConfigurationLevel);
                return (int)ReturnCode.GIT_OK;
            }

            public override int Set(string key, string value)
            {
                return 0;
            }

            public override int SetMultivar(string Name, string regexp, string value)
            {
                return 0;
            }

            public override int Del(string key)
            {
                return 0;
            }

            public override int DelMultivar(string Name, string regexp)
            {
                return 0;
            }

            public override int Iterator(out ConfigurationIterator<string> iterator)
            {
                iterator = new MockConfigurationIterator();
                return 0;
            }

            public override int Snapshot(out ConfigurationBackend<string, ConfigurationIterator<string>> configSnapshot)
            {
                var copy = new Dictionary<string, string>(this.config);
                configSnapshot = new MockConfigurationBackend(copy);
                return 0;
            }

            public override int Lock()
            {
                return 0;
            }

            public override int Unlock(out bool success)
            {
                success = true;
                return 0;
            }

            public void Dispose()
            {
               
            }
        }

        private class MockConfigurationIterator : ConfigurationIterator<string>
        {
            public override int Next(out ConfigurationEntry<string> configurationEntry)
            {
                throw new NotImplementedException();
            }
        }

        private class MockConfigurationEntry : ConfigurationEntry<string>
        {
            public MockConfigurationEntry(string key, string value, ConfigurationLevel level) : base(key, value, level)
            {

            }
        }

        #endregion
    }
}
