using Abp.AspNetCore.Mvc.Controllers;
using Abp.IdentityFramework;
using Microsoft.AspNetCore.Identity;

namespace Yootek.Controllers
{
    public abstract class YootekControllerBase: AbpController
    {
        protected YootekControllerBase()
        {
            LocalizationSourceName = YootekConsts.LocalizationSourceName;
        }

        protected void CheckErrors(IdentityResult identityResult)
        {
            identityResult.CheckErrors(LocalizationManager);
        }
    }
}
