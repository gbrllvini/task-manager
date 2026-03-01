using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Configurations;

public class TaskConfiguration : IEntityTypeConfiguration<TaskItem> {
    public void Configure(EntityTypeBuilder<TaskItem> builder) {
        builder.ToTable("tasks");

        builder.HasKey(t => t.Id);

        builder.Property(t => t.Id)
            .IsRequired();

        builder.Property(t => t.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(t => t.Title)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(t => t.Description)
            .HasMaxLength(500);

        builder.Property(t => t.Priority)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.Status)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(t => t.DueDate);

        builder.Property(t => t.CreatedAt)
            .IsRequired();

        builder.HasIndex(t => t.UserId);
    }
}