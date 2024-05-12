using System.Threading.Tasks;
using Yootek.Models.TokenAuth;
using Yootek.Web.Controllers;
using Shouldly;
using Xunit;

namespace Yootek.Web.Tests.Controllers
{
    public class HomeController_Tests: YootekWebTestBase
    {
        [Fact]
        public async Task Index_Test()
        {
            await AuthenticateAsync(null, new AuthenticateModel
            {
                UserNameOrEmailAddress = "admin",
                Password = "123qwe"
            });

            //Act
            var response = await GetResponseAsStringAsync(
                GetUrl<HomeController>(nameof(HomeController.Index))
            );

            //Assert
            response.ShouldNotBeNullOrEmpty();
        }
    }
}