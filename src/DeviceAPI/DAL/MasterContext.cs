﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace DeviceAPI;

public partial class MasterContext : DbContext
{
    public MasterContext()
    {
    }

    public MasterContext(DbContextOptions<MasterContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceEmployee> DeviceEmployees { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Person> People { get; set; }

    public virtual DbSet<Position> Positions { get; set; }
    
    public virtual DbSet<Role> Roles { get; set; }
    
    public virtual DbSet<Account> Accounts { get; set; }


    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Device>(entity =>
        {
            entity.ToTable("Device");

            entity.Property(e => e.AdditionalProperties)
                .HasMaxLength(8000)
                .IsUnicode(false)
                .HasDefaultValue("");
            entity.Property(e => e.IsEnabled).HasDefaultValue(true);
            entity.Property(e => e.Name)
                .HasMaxLength(150)
                .IsUnicode(false);

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .HasConstraintName("FK_Device_DeviceType");
        });

        modelBuilder.Entity<DeviceEmployee>(entity =>
        {
            entity.ToTable("DeviceEmployee");

            entity.Property(e => e.IssueDate).HasDefaultValueSql("(sysutcdatetime())");

            entity.HasOne(d => d.Device).WithMany(p => p.DeviceEmployees)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceEmployee_Device");

            entity.HasOne(d => d.Employee).WithMany(p => p.DeviceEmployees)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_DeviceEmployee_Employee");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.ToTable("DeviceType");

            entity.HasIndex(e => e.Name, "UQ__DeviceTy__737584F608FF18D3").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Employee>(entity =>
        {
            entity.ToTable("Employee");

            entity.Property(e => e.HireDate).HasDefaultValueSql("(sysutcdatetime())");
            entity.Property(e => e.Salary).HasColumnType("decimal(18, 2)");

            entity.HasOne(d => d.Person).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PersonId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Person");

            entity.HasOne(d => d.Position).WithMany(p => p.Employees)
                .HasForeignKey(d => d.PositionId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Employee_Position");
        });

        modelBuilder.Entity<Person>(entity =>
        {
            entity.ToTable("Person");

            entity.HasIndex(e => e.PassportNumber, "UQ__Person__45809E7118B199F6").IsUnique();

            entity.HasIndex(e => e.PhoneNumber, "UQ__Person__85FB4E3878D21353").IsUnique();

            entity.HasIndex(e => e.Email, "UQ__Person__A9D105346254865E").IsUnique();

            entity.Property(e => e.Email)
                .HasMaxLength(150)
                .IsUnicode(false);
            entity.Property(e => e.FirstName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.LastName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.MiddleName)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.PassportNumber)
                .HasMaxLength(30)
                .IsUnicode(false);
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(20)
                .IsUnicode(false);
        });

        modelBuilder.Entity<Position>(entity =>
        {
            entity.ToTable("Position");

            entity.HasIndex(e => e.Name, "UQ__Position__737584F649D70F90").IsUnique();

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });
        
        modelBuilder.Entity<Role>(entity =>
        {
            entity.ToTable("Role");

            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .IsUnicode(false);
        });
        
        modelBuilder.Entity<Account>(entity =>
        {
            entity.ToTable("Account");

            entity.Property(e => e.Username)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.Property(e => e.Password)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.Employee)
                .WithMany(p => p.Accounts)
                .HasForeignKey(d => d.EmployeeId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Employee");

            entity.HasOne(d => d.Role)
                .WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Account_Role");
        });

        

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
