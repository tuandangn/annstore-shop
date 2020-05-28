using System.Threading.Tasks;
using Xunit;

namespace Annstore.Web.IntegratedTests
{
    public class BasicTests : IClassFixture<TestAnnstoreWebApplicationFactory>
    {
        private readonly TestAnnstoreWebApplicationFactory _factory;

        public BasicTests(TestAnnstoreWebApplicationFactory factory)
        {
            _factory = factory;
        }

        [Theory]
        [InlineData("/")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();

            var response = await client.GetAsync(url);

            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}
