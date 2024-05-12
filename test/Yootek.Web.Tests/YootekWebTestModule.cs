using Abp.AspNetCore;
using Abp.AspNetCore.TestBase;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Yootek.EntityFrameworkCore;
using Yootek.Web.Startup;
using Microsoft.AspNetCore.Mvc.ApplicationParts;

namespace Yootek.Web.Tests
{
    [DependsOn(
        typeof(YootekWebMvcModule),
        typeof(AbpAspNetCoreTestBaseModule)
    )]
    public class YootekWebTestModule : AbpModule
    {
        public YootekWebTestModule(YootekEntityFrameworkModule abpProjectNameEntityFrameworkModule)
        {
            abpProjectNameEntityFrameworkModule.SkipDbContextRegistration = true;
        } 
        
        public override void PreInitialize()
        {
            Configuration.UnitOfWork.IsTransactional = false; //EF Core InMemory DB does not support transactions.
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(YootekWebTestModule).GetAssembly());
        }
        
        public override void PostInitialize()
        {
            IocManager.Resolve<ApplicationPartManager>()
                .AddApplicationPartsIfNotAddedBefore(typeof(YootekWebMvcModule).Assembly);
        }
    }
}