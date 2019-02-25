using System.Net;
using Moq;
using Xunit;

namespace Escc.Net.Tests
{
    public class HttpClientProviderTests
    {
        [Fact]
        public void TwoInstancesWithTheSameCredentialsReturnTheSameHttpClient()
        {
            var credentials = new Mock<IWebApiCredentialsProvider>();
            credentials.Setup(x => x.CreateCredentials()).Returns(new NetworkCredential("user1", "password1"));

            var clientProvider1 = new HttpClientProvider(null, credentials.Object);
            var clientProvider2 = new HttpClientProvider(null, credentials.Object);

            var client1 = clientProvider1.GetHttpClient();
            var client2 = clientProvider2.GetHttpClient();

            Assert.Equal(client1, client2);
        }

        [Fact]
        public void TwoInstancesWithDifferentCredentialsReturnDifferenceClients()
        {
            var credentials = new Mock<IWebApiCredentialsProvider>();
            credentials.Setup(x => x.CreateCredentials()).Returns(new NetworkCredential("user1", "password1"));

            var clientProvider1 = new HttpClientProvider(null, credentials.Object);
            var clientProvider2 = new HttpClientProvider(null, null);

            var client1 = clientProvider1.GetHttpClient();
            var client2 = clientProvider2.GetHttpClient();

            Assert.NotEqual(client1, client2);
        }
    }
}
