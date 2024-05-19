using Abp.Zero.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Yootek.Authorization.Permissions;
using Yootek.Authorization.Roles;
using Yootek.Authorization.Users;
using Yootek.EntityDb;
using YOOTEK.EntityDb.Citizen;
using Yootek.MultiTenancy;
using Yootek.Organizations;
// using Yootek.Organizations.OrganizationStructure;
using Yootek.Storage;


namespace Yootek.EntityFrameworkCore
{
    public class YootekDbContext : AbpZeroDbContext<Tenant, Role, User, YootekDbContext>
    {
        #region DbSet
        // public virtual DbSet<SchedulerNotification> SchedulerNotifications { get; set; }
        

        // public virtual DbSet<AppOrganizationUnit> AppOrganizationUnits { get; set; }
        
        // public virtual DbSet<OrganizationStructureUnit> OrganizationStructureUnits { get; set; }
        // public virtual DbSet<OrganizationStructureDept> OrganizationStructureDepts { get; set; }
        // public virtual DbSet<OrganizationStructureDeptUser> OrganizationStructureDeptUsers { get; set; }
        // public virtual DbSet<DeptToUnit> DeptToUnits { get; set; }
        // public virtual DbSet<UnitToUnit> UnitToUnits { get; set; }
        // public virtual DbSet<DepartmentOrganizationUnit> DepartmentOrganizationUnits { get; set; }

        public virtual DbSet<Citizen> Citizens { get; set; }
        public virtual DbSet<Meter> Meters { get; set; }
        public virtual DbSet<MeterType> MeterTypes { get; set; }
        public virtual DbSet<MeterMonthly> MeterMonthlies { get; set; }

        #endregion

        public YootekDbContext(DbContextOptions<YootekDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            // base.OnModelCreating(builder);
            /*var mutableProperties = builder.Model.GetEntityTypes()
                .SelectMany(e => e.GetProperties().Where(p => p.ClrType == typeof(string)));*/

            base.OnModelCreating(builder);
            builder.HasDefaultSchema("public");
            /* Configure your own tables/entities inside the ConfigureMPQ method */
            //builder.Entity<RoomUserChat>()
            //    .HasKey(t => new { t.UserId, t.GroupChatId });


            //builder.Entity<RoomUserChat>()
            //    .HasOne(pt => pt.User)
            //    .WithMany(p => p.RoomUserChats)
            //    .HasForeignKey(pt => pt.UserId);

            //builder.Entity<RoomUserChat>()
            //    .HasOne(pt => pt.GroupChat)
            //    .WithMany(t => t.RoomUserChats)
            //    .HasForeignKey(pt => pt.GroupChatId);


            //builder.ConfigureMPQ();
        }
    }
}
