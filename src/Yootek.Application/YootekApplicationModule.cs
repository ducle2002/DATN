﻿using Abp.AutoMapper;
using Abp.Modules;
using Abp.Reflection.Extensions;
using Yootek.Authorization;

namespace Yootek
{
    [DependsOn(
        typeof(YootekCoreModule), 
        typeof(AbpAutoMapperModule))]
    public class YootekApplicationModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Authorization.Providers.Add<YootekAuthorizationProvider>();
        }

        public override void Initialize()
        {
            var thisAssembly = typeof(YootekApplicationModule).GetAssembly();

            IocManager.RegisterAssemblyByConvention(thisAssembly);

            Configuration.Modules.AbpAutoMapper().Configurators.Add(
                // Scan the assembly for classes which inherit from AutoMapper.Profile
                cfg => cfg.AddMaps(thisAssembly)
            );
        }
    }
}
