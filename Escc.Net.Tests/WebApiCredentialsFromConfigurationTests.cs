using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Escc.Net.Tests
{
    public class WebApiCredentialsFromConfigurationTests
    {
        [Fact]
        public void InstancesWithSameEmptyDataAreEqual()
        {
            var config1 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings() });
            var instance1 = new WebApiCredentialsFromConfiguration(config1);

            var config2 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings() });
            var instance2 = new WebApiCredentialsFromConfiguration(config2);

            var result = (instance1 == instance2);

            Assert.True(result);
        }

        [Fact]
        public void InstancesWithSameConfigDataAreEqual()
        {
            var config1 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user1", Password = "password1" } });
            var instance1 = new WebApiCredentialsFromConfiguration(config1);

            var config2 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user1", Password = "password1" } });
            var instance2 = new WebApiCredentialsFromConfiguration(config2);

            var result = (instance1 == instance2);

            Assert.True(result);
        }

        [Fact]
        public void InstancesWithDifferentUsernamesAreNotEqual()
        {
            var config1 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user1", Password = "password1" } });
            var instance1 = new WebApiCredentialsFromConfiguration(config1);

            var config2 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user2", Password = "password1" } });
            var instance2 = new WebApiCredentialsFromConfiguration(config2);

            var result = (instance1 == instance2);

            Assert.False(result);
        }

        [Fact]
        public void InstancesWithDifferentPasswordsAreNotEqual()
        {
            var config1 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user1", Password = "password1" } });
            var instance1 = new WebApiCredentialsFromConfiguration(config1);

            var config2 = Options.Create(new ConfigurationSettings { WebApi = new WebApiSettings { User = "user1", Password = "password2" } });
            var instance2 = new WebApiCredentialsFromConfiguration(config2);

            var result = (instance1 == instance2);

            Assert.False(result);
        }
    }
}
