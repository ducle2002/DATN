using System.Threading.Tasks;
using Abp.Authorization;
using Abp.Runtime.Session;
using Yootek.Configuration.Dto;

namespace Yootek.Configuration
{
    [AbpAuthorize]
    public class ConfigurationAppService : YootekAppServiceBase, IConfigurationAppService
    {
        public async Task ChangeUiTheme(ChangeUiThemeInput input)
        {
            await SettingManager.ChangeSettingForUserAsync(AbpSession.ToUserIdentifier(), AppSettingNames.UiTheme, input.Theme);
        }
    }
}
