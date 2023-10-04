using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Brio.Docs.Database.Extensions;
using Brio.Docs.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace Brio.Docs.Database
{
    public class DMContext : DbContext
    {
        private static readonly DateTime DEFAULT_DATE_TIME = new DateTime(2021, 2, 20, 13, 42, 42, 954, DateTimeKind.Utc);

        public DMContext(DbContextOptions<DMContext> opt)
            : base(opt)
        {
        }

        #region Models
        public DbSet<User> Users { get; set; }

        public DbSet<Project> Projects { get; set; }

        public DbSet<Item> Items { get; set; }

        public DbSet<Objective> Objectives { get; set; }

        public DbSet<ObjectiveType> ObjectiveTypes { get; set; }

        public DbSet<DynamicField> DynamicFields { get; set; }

        public DbSet<DynamicFieldInfo> DynamicFieldInfos { get; set; }

        public DbSet<BimElement> BimElements { get; set; }

        public DbSet<ConnectionInfo> ConnectionInfos { get; set; }

        public DbSet<EnumerationType> EnumerationTypes { get; set; }

        public DbSet<EnumerationValue> EnumerationValues { get; set; }

        public DbSet<Role> Roles { get; set; }

        public DbSet<ReportCount> ReportCounts { get; set; }

        public DbSet<ConnectionType> ConnectionTypes { get; set; }

        public DbSet<AppProperty> AppProperties { get; set; }

        public DbSet<AuthFieldName> AuthFieldNames { get; set; }

        public DbSet<AuthFieldValue> AuthFieldValues { get; set; }

        public DbSet<Synchronization> Synchronizations { get; set; }

        #endregion

        #region Bridges

        public DbSet<ObjectiveItem> ObjectiveItems { get; set; }

        public DbSet<UserProject> UserProjects { get; set; }

        public DbSet<BimElementObjective> BimElementObjectives { get; set; }

        public DbSet<UserRole> UserRoles { get; set; }

        public DbSet<ConnectionInfoEnumerationType> ConnectionInfoEnumerationTypes { get; set; }

        public DbSet<ConnectionInfoEnumerationValue> ConnectionInfoEnumerationValues { get; set; }

        #endregion

        public override int SaveChanges(bool acceptAllChangesOnSuccess)
        {
            UpdateEntities();
            return base.SaveChanges(acceptAllChangesOnSuccess);
        }

        public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
        {
            UpdateEntities();
            return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }

        public Task<int> SynchronizationSaveAsync(DateTime dateTime, CancellationToken cancellationToken = new CancellationToken())
        {
            UpdateEntities(dateTime);
            return base.SaveChangesAsync(true, cancellationToken);
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>()
                .HasOne(x => x.ConnectionInfo)
                .WithOne(x => x.User)
                .OnDelete(DeleteBehavior.SetNull);

            // Users should have unique logins
            modelBuilder.Entity<User>()
                .HasIndex(x => x.Login)
                .IsUnique(true);

            modelBuilder.Entity<ReportCount>()
                .HasKey(x => x.UserID);

            // Roles have unique names
            modelBuilder.Entity<Role>()
                .HasIndex(x => x.Name)
                .IsUnique(true);

            modelBuilder.Entity<ObjectiveType>()
                .HasIndex(
                    x => new
                    {
                        x.Name,
                        x.ConnectionTypeID,
                    })
                .IsUnique(true);
            modelBuilder.Entity<ObjectiveType>()
                .HasIndex(x => x.ExternalId)
                .IsUnique(true);
            modelBuilder.Entity<ObjectiveType>()
                .HasMany(x => x.Objectives)
                .WithOne(x => x.ObjectiveType)
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<ObjectiveType>()
                .HasMany(x => x.DefaultDynamicFields)
                .WithOne(x => x.ObjectiveType)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Location>()
               .HasOne(x => x.Objective)
               .WithOne(x => x.Location)
               .HasForeignKey<Location>(x => x.ObjectiveID)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
                 .HasIndex(x => x.Name)
                 .IsUnique();

            modelBuilder.Entity<UserRole>()
                .HasKey(x => new { x.UserID, x.RoleID });
            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.User)
                .WithMany(x => x.Roles)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserRole>()
                .HasOne(x => x.Role)
                .WithMany(x => x.Users)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserProject>()
                .HasKey(x => new { x.ProjectID, x.UserID });
            modelBuilder.Entity<UserProject>()
                .HasOne(x => x.User)
                .WithMany(x => x.Projects)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<UserProject>()
                .HasOne(x => x.Project)
                .WithMany(x => x.Users)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Item>()
                    .HasOne(x => x.Project)
                    .WithMany(x => x.Items)
                    .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ObjectiveItem>()
                .HasKey(x => new { x.ObjectiveID, x.ItemID });
            modelBuilder.Entity<ObjectiveItem>()
                .HasOne(x => x.Item)
                .WithMany(x => x.Objectives)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ObjectiveItem>()
                .HasOne(x => x.Objective)
                .WithMany(x => x.Items)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Objective>()
                .HasOne(x => x.Project)
                .WithMany(x => x.Objectives)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<Objective>()
                .HasOne(x => x.Author)
                .WithMany(x => x.Objectives)
                .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Objective>()
                .HasOne(x => x.ParentObjective)
                .WithMany(x => x.ChildrenObjectives)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicField>()
               .HasOne(x => x.Objective)
               .WithMany(x => x.DynamicFields)
               .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<DynamicField>()
                .HasOne(x => x.ParentField)
                .WithMany(x => x.ChildrenDynamicFields)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<DynamicFieldInfo>()
                .HasOne(x => x.ParentField)
                .WithMany(x => x.ChildrenDynamicFields)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<BimElement>()
                .HasKey(x => x.ID);

            modelBuilder.Entity<BimElementObjective>()
                .HasKey(x => new { x.BimElementID, x.ObjectiveID });
            modelBuilder.Entity<BimElementObjective>()
                .HasOne(x => x.BimElement)
                .WithMany(x => x.Objectives)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<BimElementObjective>()
                .HasOne(x => x.Objective)
                .WithMany(x => x.BimElements)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionInfoEnumerationType>()
              .HasKey(x => new { x.ConnectionInfoID, x.EnumerationTypeID });
            modelBuilder.Entity<ConnectionInfoEnumerationType>()
                .HasOne(x => x.ConnectionInfo)
                .WithMany(x => x.EnumerationTypes)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ConnectionInfoEnumerationType>()
                .HasOne(x => x.EnumerationType)
                .WithMany(x => x.ConnectionInfos)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionInfoEnumerationValue>()
                .HasKey(x => new { x.ConnectionInfoID, x.EnumerationValueID });
            modelBuilder.Entity<ConnectionInfoEnumerationValue>()
                .HasOne(x => x.ConnectionInfo)
                .WithMany(x => x.EnumerationValues)
                .OnDelete(DeleteBehavior.Cascade);
            modelBuilder.Entity<ConnectionInfoEnumerationValue>()
                .HasOne(x => x.EnumerationValue)
                .WithMany(x => x.ConnectionInfos)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EnumerationValue>()
                .HasOne(x => x.EnumerationType)
                .WithMany(x => x.EnumerationValues)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
                .HasMany(x => x.AppProperties)
                .WithOne(x => x.ConnectionType)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
                .HasMany(x => x.AuthFieldNames)
                .WithOne(x => x.ConnectionType)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
               .HasMany(x => x.ObjectiveTypes)
               .WithOne(x => x.ConnectionType)
               .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
                .HasMany(x => x.EnumerationTypes)
                .WithOne(x => x.ConnectionType)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionType>()
              .HasMany(x => x.ConnectionInfos)
              .WithOne(x => x.ConnectionType)
              .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ConnectionInfo>()
                .HasMany(x => x.AuthFieldValues)
                .WithOne(x => x.ConnectionInfo)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Project>()
                    .HasOne(x => x.SynchronizationMate)
                    .WithOne()
                    .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Objective>()
                    .HasOne(x => x.SynchronizationMate)
                    .WithOne()
                    .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<Item>()
                    .HasOne(x => x.SynchronizationMate)
                    .WithOne()
                    .OnDelete(DeleteBehavior.SetNull);
            modelBuilder.Entity<DynamicField>()
                    .HasOne(x => x.SynchronizationMate)
                    .WithOne()
                    .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Project>()
                    .Property(x => x.UpdatedAt)
                    .HasDefaultValue(DEFAULT_DATE_TIME);
            modelBuilder.Entity<Objective>()
                    .Property(x => x.UpdatedAt)
                    .HasDefaultValue(DEFAULT_DATE_TIME);
            modelBuilder.Entity<Item>()
                    .Property(x => x.UpdatedAt)
                    .HasDefaultValue(DEFAULT_DATE_TIME);
            modelBuilder.Entity<DynamicField>()
                    .Property(x => x.UpdatedAt)
                    .HasDefaultValue(DEFAULT_DATE_TIME);
        }

        private void UpdateEntities(DateTime dateTime = default)
        {
            UpdateDateTime(dateTime);
            UpdateObjective();
            UpdateItem();
        }

        private void UpdateDateTime(DateTime dateTime = default)
            => ChangeTracker.UpdateDateTime(dateTime);

        private void UpdateObjective()
        {
            foreach (var entityEntry in ChangeTracker
               .Entries()
               .Where(e => e.Entity is Objective && e.State is EntityState.Added or EntityState.Modified))
            {
                var objective = (Objective)entityEntry.Entity;
                objective.TitleToLower = objective.Title.ToLowerInvariant();
            }
        }

        private void UpdateItem()
        {
            foreach (var entityEntry in ChangeTracker
               .Entries()
               .Where(e => e.Entity is Item && e.State is EntityState.Added or EntityState.Modified))
            {
                var item = (Item)entityEntry.Entity;
                item.Name = Path.GetFileNameWithoutExtension(item.RelativePath);
            }
        }
    }
}
