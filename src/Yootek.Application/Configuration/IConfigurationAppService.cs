using System.Threading.Tasks;
using Yootek.Configuration.Dto;

namespace Yootek.Configuration
{
    public interface IConfigurationAppService
    {
        Task ChangeUiTheme(ChangeUiThemeInput input);
    }
}
