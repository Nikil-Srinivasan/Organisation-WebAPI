﻿// <auto-generated />
using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Organisation_WebAPI.Data;

#nullable disable

namespace Organisation_WebAPI.Migrations
{
    [DbContext(typeof(OrganizationContext))]
    partial class OrganizationContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "7.0.7")
                .HasAnnotation("Relational:MaxIdentifierLength", 128);

            SqlServerModelBuilderExtensions.UseIdentityColumns(modelBuilder);

            modelBuilder.Entity("Organisation_WebAPI.Models.Customer", b =>
                {
                    b.Property<int>("CustomerID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("CustomerID"));

                    b.Property<string>("CustomerEmail")
                        .HasColumnType("nvarchar(max)");

                    b.Property<string>("CustomerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("CustomerPhoneNumber")
                        .HasColumnType("int");

                    b.Property<int>("ProductID")
                        .HasColumnType("int");

                    b.HasKey("CustomerID");

                    b.HasIndex("ProductID");

                    b.ToTable("Customers");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Department", b =>
                {
                    b.Property<int>("DepartmentID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("DepartmentID"));

                    b.Property<string>("DepartmentName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("DepartmentID");

                    b.ToTable("Departments");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Employee", b =>
                {
                    b.Property<int>("EmployeeID")
                        .HasColumnType("int");

                    b.Property<int>("DepartmentID")
                        .HasColumnType("int");

                    b.Property<int>("EmployeeAge")
                        .HasColumnType("int");

                    b.Property<string>("EmployeeName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("EmployeeSalary")
                        .HasColumnType("int");

                    b.Property<int?>("ManagerID")
                        .HasColumnType("int");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.HasKey("EmployeeID");

                    b.HasIndex("DepartmentID");

                    b.HasIndex("ManagerID");

                    b.HasIndex("UserID");

                    b.ToTable("Employees");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.EmployeeTask", b =>
                {
                    b.Property<int>("TaskID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("TaskID"));

                    b.Property<int>("EmployeeId")
                        .HasColumnType("int");

                    b.Property<DateTime?>("TaskCreatedDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TaskDescription")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTime?>("TaskDueDate")
                        .HasColumnType("datetime2");

                    b.Property<string>("TaskName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("TaskStatus")
                        .HasColumnType("int");

                    b.HasKey("TaskID");

                    b.HasIndex("EmployeeId");

                    b.ToTable("EmployeeTasks");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Manager", b =>
                {
                    b.Property<int>("ManagerId")
                        .HasColumnType("int");

                    b.Property<int>("ManagerAge")
                        .HasColumnType("int");

                    b.Property<string>("ManagerName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ManagerSalary")
                        .HasColumnType("int");

                    b.Property<int?>("ProductID")
                        .HasColumnType("int");

                    b.Property<int?>("UserID")
                        .HasColumnType("int");

                    b.Property<bool>("isAppointed")
                        .HasColumnType("bit");

                    b.HasKey("ManagerId");

                    b.HasIndex("ProductID");

                    b.HasIndex("UserID");

                    b.ToTable("Managers");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Product", b =>
                {
                    b.Property<int>("ProductID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("ProductID"));

                    b.Property<string>("ProductName")
                        .HasColumnType("nvarchar(max)");

                    b.Property<int>("ProductRevenue")
                        .HasColumnType("int");

                    b.HasKey("ProductID");

                    b.ToTable("Products");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.User", b =>
                {
                    b.Property<int>("UserID")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("int");

                    SqlServerPropertyBuilderExtensions.UseIdentityColumn(b.Property<int>("UserID"));

                    b.Property<string>("Email")
                        .HasColumnType("nvarchar(max)");

                    b.Property<bool>("IsVerified")
                        .HasColumnType("bit");

                    b.Property<string>("Otp")
                        .HasColumnType("nvarchar(max)");

                    b.Property<DateTimeOffset?>("OtpExpiration")
                        .HasColumnType("datetimeoffset");

                    b.Property<int>("OtpResendCount")
                        .HasColumnType("int");

                    b.Property<byte[]>("PasswordHash")
                        .HasColumnType("varbinary(max)");

                    b.Property<byte[]>("PasswordSalt")
                        .HasColumnType("varbinary(max)");

                    b.Property<int>("Role")
                        .HasColumnType("int");

                    b.Property<string>("UserName")
                        .HasColumnType("nvarchar(max)");

                    b.HasKey("UserID");

                    b.ToTable("Users");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Customer", b =>
                {
                    b.HasOne("Organisation_WebAPI.Models.Product", "Product")
                        .WithMany("Customers")
                        .HasForeignKey("ProductID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Product");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Employee", b =>
                {
                    b.HasOne("Organisation_WebAPI.Models.Department", "Department")
                        .WithMany("Employees")
                        .HasForeignKey("DepartmentID")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("Organisation_WebAPI.Models.Manager", "Manager")
                        .WithMany("Employees")
                        .HasForeignKey("ManagerID");

                    b.HasOne("Organisation_WebAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("Department");

                    b.Navigation("Manager");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.EmployeeTask", b =>
                {
                    b.HasOne("Organisation_WebAPI.Models.Employee", "Employee")
                        .WithMany()
                        .HasForeignKey("EmployeeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Employee");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Manager", b =>
                {
                    b.HasOne("Organisation_WebAPI.Models.Product", "Product")
                        .WithMany()
                        .HasForeignKey("ProductID");

                    b.HasOne("Organisation_WebAPI.Models.User", "User")
                        .WithMany()
                        .HasForeignKey("UserID");

                    b.Navigation("Product");

                    b.Navigation("User");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Department", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Manager", b =>
                {
                    b.Navigation("Employees");
                });

            modelBuilder.Entity("Organisation_WebAPI.Models.Product", b =>
                {
                    b.Navigation("Customers");
                });
#pragma warning restore 612, 618
        }
    }
}
