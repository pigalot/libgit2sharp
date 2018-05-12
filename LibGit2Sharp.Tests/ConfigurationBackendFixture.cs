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
            using (var repo = new Repository())
            {
                repo.Config.AddBackend(new MockConfigurationBackend(), ConfigurationLevel.Local);

                Assert.True(repo.Info.IsBare);
                Assert.Null(repo.Info.Path);
                Assert.Null(repo.Info.WorkingDirectory);

                Assert.Throws<BareRepositoryException>(() => { var idx = repo.Index; });
            }
        }

        #region MockBackend

        private class MockConfigurationBackend : ConfigurationBackend<string, MockConfigurationIterator>
        {
            protected override ConfigBackendOperations SupportedOperations
            {
                get { return ConfigBackendOperations.Open; }
            }

            public override int Open(ConfigurationLevel level)
            {
                return 0;
            }

            public override int Get(string key, out ConfigurationEntry<string> configurationEntry)
            {
                throw new NotImplementedException();
            }

            public override int Set(string key, string value)
            {
                throw new NotImplementedException();
            }

            public override int SetMultivar(string Name, string regexp, string value)
            {
                throw new NotImplementedException();
            }

            public override int Del(string key)
            {
                throw new NotImplementedException();
            }

            public override int DelMultivar(string Name, string regexp)
            {
                throw new NotImplementedException();
            }

            public override int Iterator(out MockConfigurationIterator iterator)
            {
                throw new NotImplementedException();
            }

            public override int Snapshot(out ConfigurationBackend<string, MockConfigurationIterator> configSnapshot)
            {
                throw new NotImplementedException();
            }

            public override int Lock()
            {
                throw new NotImplementedException();
            }

            public override int Unlock(out bool success)
            {
                throw new NotImplementedException();
            }
        }

        private class MockConfigurationIterator : ConfigurationIterator<string>
        {
            public override int Next(out ConfigurationEntry<string> configurationEntry)
            {
                throw new NotImplementedException();
            }
        }

        #endregion
    }
}
