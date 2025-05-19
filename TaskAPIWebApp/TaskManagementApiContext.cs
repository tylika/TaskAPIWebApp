using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TaskAPIWebApp.Models;
using AppTask = TaskAPIWebApp.Models.Task; // Псевдонім для Task

namespace TaskAPIWebApp
{
    public partial class TaskManagementApiContext : DbContext
    {
        public TaskManagementApiContext(DbContextOptions<TaskManagementApiContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Comment> Comments { get; set; }
        public virtual DbSet<GroupMember> GroupMembers { get; set; }
        public virtual DbSet<AppTask> Tasks { get; set; } // Використовуємо псевдонім
        public virtual DbSet<TaskAttachment> TaskAttachments { get; set; }
        public virtual DbSet<TaskGroup> TaskGroups { get; set; }
        public virtual DbSet<TaskSubmission> TaskSubmissions { get; set; }
        public virtual DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Comment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Comments__3214EC07EECCFD81");

                entity.Property(e => e.CreatedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Task).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.TaskId)
                    .HasConstraintName("FK_Comments_Tasks");

                entity.HasOne(d => d.TaskSubmission).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.TaskSubmissionId)
                    .HasConstraintName("FK_Comments_TaskSubmissions");

                entity.HasOne(d => d.User).WithMany(p => p.Comments)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Comments_Users");
            });

            modelBuilder.Entity<GroupMember>(entity =>
            {
                entity.HasKey(e => new { e.UserId, e.TaskGroupId });

                entity.Property(e => e.JoinedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");
                entity.Property(e => e.Role)
                    .HasMaxLength(50)
                    .HasDefaultValue("Member");

                entity.HasOne(d => d.TaskGroup).WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.TaskGroupId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupMembers_TaskGroups");

                entity.HasOne(d => d.User).WithMany(p => p.GroupMembers)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_GroupMembers_Users");
            });

            modelBuilder.Entity<AppTask>(entity => // Використовуємо псевдонім
            {
                entity.HasKey(e => e.Id).HasName("PK__Tasks__3214EC07B86B8AC2");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Draft");

                entity.HasOne(d => d.TaskGroup).WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.TaskGroupId)
                    .HasConstraintName("FK_Tasks_TaskGroups");

                entity.HasOne(d => d.User).WithMany(p => p.Tasks)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_Tasks_Users");
            });

            modelBuilder.Entity<TaskAttachment>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TaskAtta__3214EC07949F78C3");

                entity.Property(e => e.FilePath).HasMaxLength(255);
                entity.Property(e => e.UploadedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Task).WithMany(p => p.TaskAttachments)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskAttachments_Tasks");
            });

            modelBuilder.Entity<TaskGroup>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TaskGrou__3214EC072BED5930");

                entity.Property(e => e.Name).HasMaxLength(255);

                entity.HasOne(d => d.User).WithMany(p => p.TaskGroups)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskGroups_Users");
            });

            modelBuilder.Entity<TaskSubmission>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__TaskSubm__3214EC076FD042DB");

                entity.Property(e => e.Status)
                    .HasMaxLength(50)
                    .HasDefaultValue("Pending");
                entity.Property(e => e.SubmittedAt)
                    .HasDefaultValueSql("(getdate())")
                    .HasColumnType("datetime");

                entity.HasOne(d => d.Task).WithMany(p => p.TaskSubmissions)
                    .HasForeignKey(d => d.TaskId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskSubmissions_Tasks");

                entity.HasOne(d => d.User).WithMany(p => p.TaskSubmissions)
                    .HasForeignKey(d => d.UserId)
                    .OnDelete(DeleteBehavior.ClientSetNull)
                    .HasConstraintName("FK_TaskSubmissions_Users");
            });

            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(e => e.Id).HasName("PK__Users__3214EC07B74163D8");

                entity.Property(e => e.Username).HasMaxLength(255);
            });

            OnModelCreatingPartial(modelBuilder);
        }

        partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
    }
}