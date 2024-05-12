using AutoMapper;
using Yootek.Authorization.Permissions.Dto;

using Yootek.Organizations;
using Yootek.Services;
using System.Collections;
using System.Collections.Generic;

using Yootek.Authorization.Users;

using Permission = Abp.Authorization.Permission;
using System.Reflection;
using System;
using AutoMapper.Internal;
using AutoMapper.Configuration;
using System.Linq;

namespace Yootek
{
    internal static class CustomDtoMapper
    {
        private static volatile bool _mappedBefore;
        private static readonly object SyncObj = new object();

        public static void CreateMappings(IMapperConfigurationExpression mapper)
        {
            lock (SyncObj)
            {
                if (_mappedBefore)
                {
                    return;
                }

                CreateMappingsInternal(mapper);

                _mappedBefore = true;
            }
        }

        private static void CreateMappingsInternal(IMapperConfigurationExpression mapper)
        {
            //Permission
            mapper.CreateMap<Permission, FlatPermissionDto>();
            mapper.CreateMap<Permission, FlatPermissionWithLevelDto>();

            
        }

        #region method helpers 
        private static bool IsNotNullOrDefault<T>(T srcMember)
        {
            if (srcMember is IEnumerable list)
            {
                return list.GetEnumerator().MoveNext();
            }
            return srcMember != null && !EqualityComparer<T>.Default.Equals(srcMember, default);
        }
        #endregion
    }

    public static class AutoMapperExtensions
    {
        private static readonly PropertyInfo TypeMapActionsProperty = typeof(TypeMapConfiguration).GetProperty("TypeMapActions", BindingFlags.NonPublic | BindingFlags.Instance);

        // not needed in AutoMapper 12.0.1
        private static readonly PropertyInfo DestinationTypeDetailsProperty = typeof(TypeMap).GetProperty("DestinationTypeDetails", BindingFlags.NonPublic | BindingFlags.Instance);

        public static void ForAllOtherMembers<TSource, TDestination>(this IMappingExpression<TSource, TDestination> expression, Action<IMemberConfigurationExpression<TSource, TDestination, object>> memberOptions)
        {
            var typeMapConfiguration = (TypeMapConfiguration)expression;

            var typeMapActions = (List<Action<TypeMap>>)TypeMapActionsProperty.GetValue(typeMapConfiguration);

            typeMapActions.Add(typeMap =>
            {
                var destinationTypeDetails = (TypeDetails)DestinationTypeDetailsProperty.GetValue(typeMap);

                foreach (var accessor in destinationTypeDetails.WriteAccessors.Where(m => typeMapConfiguration.GetDestinationMemberConfiguration(m) == null))
                {
                    expression.ForMember(accessor.Name, memberOptions);
                }
            });
        }
    }
}