using System;
using System.Collections.Specialized;
using Xunit;

namespace Escc.Net.Configuration.Tests
{
    public class ConfigurationWebApiCredentialsProviderTests
    {
        [Fact]
        public void InstancesWithSameEmptyDataAreEqual()
        {
            var config1 = new NameValueCollection();
            config1.Add("User", "");
            config1.Add("Password", "");
            var instance1 = new ConfigurationWebApiCredentialsProvider(config1);

            var config2 = new NameValueCollection();
            config2.Add("User", "");
            config2.Add("Password", "");
            var instance2 = new ConfigurationWebApiCredentialsProvider(config2);

            var result = (instance1 == instance2);

            Assert.True(result);
        }

        [Fact]
        public void InstancesWithSameConfigDataAreEqual()
        {
            var config1 = new NameValueCollection();
            config1.Add("User", "user1");
            config1.Add("Password", "password1");
            var instance1 = new ConfigurationWebApiCredentialsProvider(config1);

            var config2 = new NameValueCollection();
            config2.Add("User", "user1");
            config2.Add("Password", "password1");
            var instance2 = new ConfigurationWebApiCredentialsProvider(config2);

            var result = (instance1 == instance2);

            Assert.True(result);
        }

        [Fact]
        public void InstancesWithDifferentUsernamesAreNotEqual()
        {
            var config1 = new NameValueCollection();
            config1.Add("User", "user1");
            config1.Add("Password", "password1");
            var instance1 = new ConfigurationWebApiCredentialsProvider(config1);

            var config2 = new NameValueCollection();
            config2.Add("User", "user2");
            config2.Add("Password", "password1");
            var instance2 = new ConfigurationWebApiCredentialsProvider(config2);

            var result = (instance1 == instance2);

            Assert.False(result);
        }

        [Fact]
        public void InstancesWithDifferentPasswordsAreNotEqual()
        {
            var config1 = new NameValueCollection();
            config1.Add("User", "user1");
            config1.Add("Password", "password1");
            var instance1 = new ConfigurationWebApiCredentialsProvider(config1);

            var config2 = new NameValueCollection();
            config2.Add("User", "user1");
            config2.Add("Password", "password2");
            var instance2 = new ConfigurationWebApiCredentialsProvider(config2);

            var result = (instance1 == instance2);

            Assert.False(result);
        }
    }
}
