using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TechRepair_CRM.Models.Db;

namespace TechRepair_CRM.Data;

public partial class RepairServiceDbContext : DbContext
{
    public RepairServiceDbContext(DbContextOptions<RepairServiceDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Client> Clients { get; set; }

    public virtual DbSet<Device> Devices { get; set; }

    public virtual DbSet<DeviceType> DeviceTypes { get; set; }

    public virtual DbSet<OrderService> OrderServices { get; set; }

    public virtual DbSet<OrderServicePart> OrderServiceParts { get; set; }

    public virtual DbSet<OrderStatus> OrderStatuses { get; set; }

    public virtual DbSet<OrderStatusHistory> OrderStatusHistories { get; set; }

    public virtual DbSet<Part> Parts { get; set; }

    public virtual DbSet<Payment> Payments { get; set; }

    public virtual DbSet<RepairOrder> RepairOrders { get; set; }

    public virtual DbSet<Service> Services { get; set; }

    public virtual DbSet<Technician> Technicians { get; set; }

    public virtual DbSet<VwClientOrderHistory> VwClientOrderHistories { get; set; }

    public virtual DbSet<VwOrderCostBreakdown> VwOrderCostBreakdowns { get; set; }

    public virtual DbSet<VwOrderFullInfo> VwOrderFullInfos { get; set; }

    public virtual DbSet<VwOrderPayment> VwOrderPayments { get; set; }

    public virtual DbSet<VwOrderServicePartDetail> VwOrderServicePartDetails { get; set; }

    public virtual DbSet<VwOrderStatusTimestamp> VwOrderStatusTimestamps { get; set; }

    public virtual DbSet<VwPartUsageStatistic> VwPartUsageStatistics { get; set; }

    public virtual DbSet<VwRepairDuration> VwRepairDurations { get; set; }

    public virtual DbSet<VwServiceStatistic> VwServiceStatistics { get; set; }

    public virtual DbSet<VwTechnicianWorkload> VwTechnicianWorkloads { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Client>(entity =>
        {
            entity.HasKey(e => e.ClientId).HasName("client_pkey");

            entity.ToTable("client");

            entity.HasIndex(e => e.Phone, "client_phone_key").IsUnique();

            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.RegistrationDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("registration_date");
        });

        modelBuilder.Entity<Device>(entity =>
        {
            entity.HasKey(e => e.DeviceId).HasName("device_pkey");

            entity.ToTable("device");

            entity.HasIndex(e => e.SerialNumber, "device_serial_number_key").IsUnique();

            entity.HasIndex(e => e.ClientId, "idx_device_client_id");

            entity.HasIndex(e => e.DeviceTypeId, "idx_device_device_type_id");

            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.DeviceTypeId).HasColumnName("device_type_id");
            entity.Property(e => e.EquipmentDescription).HasColumnName("equipment_description");
            entity.Property(e => e.ExternalCondition).HasColumnName("external_condition");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");

            entity.HasOne(d => d.Client).WithMany(p => p.Devices)
                .HasForeignKey(d => d.ClientId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_device_client");

            entity.HasOne(d => d.DeviceType).WithMany(p => p.Devices)
                .HasForeignKey(d => d.DeviceTypeId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_device_device_type");
        });

        modelBuilder.Entity<DeviceType>(entity =>
        {
            entity.HasKey(e => e.DeviceTypeId).HasName("device_type_pkey");

            entity.ToTable("device_type");

            entity.HasIndex(e => e.TypeName, "device_type_type_name_key").IsUnique();

            entity.Property(e => e.DeviceTypeId).HasColumnName("device_type_id");
            entity.Property(e => e.TypeName)
                .HasMaxLength(100)
                .HasColumnName("type_name");
        });

        modelBuilder.Entity<OrderService>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ServiceId }).HasName("order_service_pkey");

            entity.ToTable("order_service");

            entity.HasIndex(e => e.CompletedAt, "idx_order_service_completed_at");

            entity.HasIndex(e => e.ServiceId, "idx_order_service_service_id");

            entity.HasIndex(e => new { e.TechnicianId, e.CompletedAt }, "idx_order_service_technician_completed_at");

            entity.HasIndex(e => e.TechnicianId, "idx_order_service_technician_id");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PriceAtMoment)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_moment");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TechnicianId).HasColumnName("technician_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderServices)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_service_order");

            entity.HasOne(d => d.Service).WithMany(p => p.OrderServices)
                .HasForeignKey(d => d.ServiceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_order_service_service");

            entity.HasOne(d => d.Technician).WithMany(p => p.OrderServices)
                .HasForeignKey(d => d.TechnicianId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_order_service_technician");
        });

        modelBuilder.Entity<OrderServicePart>(entity =>
        {
            entity.HasKey(e => new { e.OrderId, e.ServiceId, e.PartId }).HasName("order_service_part_pkey");

            entity.ToTable("order_service_part");

            entity.HasIndex(e => new { e.OrderId, e.ServiceId }, "idx_order_service_part_order_service");

            entity.HasIndex(e => e.PartId, "idx_order_service_part_part_id");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.PriceAtMoment)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_moment");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Part).WithMany(p => p.OrderServiceParts)
                .HasForeignKey(d => d.PartId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_order_service_part_part");

            entity.HasOne(d => d.OrderService).WithMany(p => p.OrderServiceParts)
                .HasForeignKey(d => new { d.OrderId, d.ServiceId })
                .HasConstraintName("fk_order_service_part_order_service");
        });

        modelBuilder.Entity<OrderStatus>(entity =>
        {
            entity.HasKey(e => e.StatusId).HasName("order_status_pkey");

            entity.ToTable("order_status");

            entity.HasIndex(e => e.StatusName, "order_status_status_name_key").IsUnique();

            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.StatusName)
                .HasMaxLength(50)
                .HasColumnName("status_name");
        });

        modelBuilder.Entity<OrderStatusHistory>(entity =>
        {
            entity.HasKey(e => e.HistoryId).HasName("order_status_history_pkey");

            entity.ToTable("order_status_history");

            entity.HasIndex(e => e.ChangedAt, "idx_order_status_history_changed_at");

            entity.HasIndex(e => new { e.OrderId, e.ChangedAt }, "idx_order_status_history_order_changed_at");

            entity.HasIndex(e => e.OrderId, "idx_order_status_history_order_id");

            entity.HasIndex(e => e.StatusId, "idx_order_status_history_status_id");

            entity.Property(e => e.HistoryId).HasColumnName("history_id");
            entity.Property(e => e.ChangedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("changed_at");
            entity.Property(e => e.Comment).HasColumnName("comment");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.StatusId).HasColumnName("status_id");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("fk_order_status_history_order");

            entity.HasOne(d => d.Status).WithMany(p => p.OrderStatusHistories)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_order_status_history_status");
        });

        modelBuilder.Entity<Part>(entity =>
        {
            entity.HasKey(e => e.PartId).HasName("part_pkey");

            entity.ToTable("part");

            entity.HasIndex(e => e.PartNumber, "part_part_number_key").IsUnique();

            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.DefaultPrice)
                .HasPrecision(10, 2)
                .HasColumnName("default_price");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.PartName)
                .HasMaxLength(100)
                .HasColumnName("part_name");
            entity.Property(e => e.PartNumber)
                .HasMaxLength(50)
                .HasColumnName("part_number");
        });

        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.PaymentId).HasName("payment_pkey");

            entity.ToTable("payment");

            entity.HasIndex(e => e.PaymentMethod, "idx_payment_method");

            entity.HasIndex(e => new { e.OrderId, e.PaymentDate }, "idx_payment_order_date");

            entity.HasIndex(e => e.OrderId, "idx_payment_order_id");

            entity.HasIndex(e => e.PaymentDate, "idx_payment_payment_date");

            entity.HasIndex(e => e.TransactionNumber, "ux_payment_transaction_number")
                .IsUnique()
                .HasFilter("(transaction_number IS NOT NULL)");

            entity.Property(e => e.PaymentId).HasColumnName("payment_id");
            entity.Property(e => e.Amount)
                .HasPrecision(10, 2)
                .HasColumnName("amount");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.PaymentDate)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .HasColumnType("timestamp without time zone")
                .HasColumnName("payment_date");
            entity.Property(e => e.PaymentMethod)
                .HasMaxLength(20)
                .HasColumnName("payment_method");
            entity.Property(e => e.TransactionNumber)
                .HasMaxLength(100)
                .HasColumnName("transaction_number");

            entity.HasOne(d => d.Order).WithMany(p => p.Payments)
                .HasForeignKey(d => d.OrderId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_payment_order");
        });

        modelBuilder.Entity<RepairOrder>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("repair_order_pkey");

            entity.ToTable("repair_order");

            entity.HasIndex(e => e.DeviceId, "idx_repair_order_device_id");

            entity.HasIndex(e => e.StatusId, "idx_repair_order_status_id");

            entity.HasIndex(e => e.OrderNumber, "repair_order_order_number_key").IsUnique();

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.DiagnosticResult).HasColumnName("diagnostic_result");
            entity.Property(e => e.EstimatedCost)
                .HasPrecision(10, 2)
                .HasColumnName("estimated_cost");
            entity.Property(e => e.IsWarrantyRepair)
                .HasDefaultValue(false)
                .HasColumnName("is_warranty_repair");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.ProblemDescription).HasColumnName("problem_description");
            entity.Property(e => e.StatusId).HasColumnName("status_id");
            entity.Property(e => e.TotalCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_cost");
            entity.Property(e => e.WarrantyMonths).HasColumnName("warranty_months");

            entity.HasOne(d => d.Device).WithMany(p => p.RepairOrders)
                .HasForeignKey(d => d.DeviceId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_repair_order_device");

            entity.HasOne(d => d.Status).WithMany(p => p.RepairOrders)
                .HasForeignKey(d => d.StatusId)
                .OnDelete(DeleteBehavior.Restrict)
                .HasConstraintName("fk_repair_order_status");
        });

        modelBuilder.Entity<Service>(entity =>
        {
            entity.HasKey(e => e.ServiceId).HasName("service_pkey");

            entity.ToTable("service");

            entity.HasIndex(e => e.ServiceName, "service_service_name_key").IsUnique();

            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.BasePrice)
                .HasPrecision(10, 2)
                .HasColumnName("base_price");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstimatedDuration).HasColumnName("estimated_duration");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
        });

        modelBuilder.Entity<Technician>(entity =>
        {
            entity.HasKey(e => e.TechnicianId).HasName("technician_pkey");

            entity.ToTable("technician");

            entity.HasIndex(e => e.Email, "technician_email_key").IsUnique();

            entity.HasIndex(e => e.Phone, "technician_phone_key").IsUnique();

            entity.Property(e => e.TechnicianId).HasColumnName("technician_id");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
        });

        modelBuilder.Entity<VwClientOrderHistory>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_client_order_history");

            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.CanceledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("canceled_at");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(100)
                .HasColumnName("device_type");
            entity.Property(e => e.Email)
                .HasMaxLength(254)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsWarrantyRepair).HasColumnName("is_warranty_repair");
            entity.Property(e => e.IssuedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("order_status");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(10, 2)
                .HasColumnName("paid_amount");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.ProblemDescription).HasColumnName("problem_description");
            entity.Property(e => e.RemainingAmount)
                .HasPrecision(10, 2)
                .HasColumnName("remaining_amount");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");
            entity.Property(e => e.TotalCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_cost");
        });

        modelBuilder.Entity<VwOrderCostBreakdown>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_order_cost_breakdown");

            entity.Property(e => e.CalculatedTotal)
                .HasPrecision(10, 2)
                .HasColumnName("calculated_total");
            entity.Property(e => e.Difference)
                .HasPrecision(10, 2)
                .HasColumnName("difference");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.PartSum)
                .HasPrecision(10, 2)
                .HasColumnName("part_sum");
            entity.Property(e => e.ServiceSum)
                .HasPrecision(10, 2)
                .HasColumnName("service_sum");
            entity.Property(e => e.StoredTotal)
                .HasPrecision(10, 2)
                .HasColumnName("stored_total");
        });

        modelBuilder.Entity<VwOrderFullInfo>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_order_full_info");

            entity.Property(e => e.AcceptedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("accepted_at");
            entity.Property(e => e.Brand)
                .HasMaxLength(50)
                .HasColumnName("brand");
            entity.Property(e => e.CalculatedTotal)
                .HasPrecision(10, 2)
                .HasColumnName("calculated_total");
            entity.Property(e => e.CanceledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("canceled_at");
            entity.Property(e => e.ClientEmail)
                .HasMaxLength(254)
                .HasColumnName("client_email");
            entity.Property(e => e.ClientFirstName)
                .HasMaxLength(50)
                .HasColumnName("client_first_name");
            entity.Property(e => e.ClientId).HasColumnName("client_id");
            entity.Property(e => e.ClientLastName)
                .HasMaxLength(50)
                .HasColumnName("client_last_name");
            entity.Property(e => e.ClientPhone)
                .HasMaxLength(20)
                .HasColumnName("client_phone");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.DeviceId).HasColumnName("device_id");
            entity.Property(e => e.DeviceType)
                .HasMaxLength(100)
                .HasColumnName("device_type");
            entity.Property(e => e.DiagnosticResult).HasColumnName("diagnostic_result");
            entity.Property(e => e.EquipmentDescription).HasColumnName("equipment_description");
            entity.Property(e => e.EstimatedCost)
                .HasPrecision(10, 2)
                .HasColumnName("estimated_cost");
            entity.Property(e => e.ExternalCondition).HasColumnName("external_condition");
            entity.Property(e => e.IsWarrantyRepair).HasColumnName("is_warranty_repair");
            entity.Property(e => e.IssuedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.LastStatusChangedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_status_changed_at");
            entity.Property(e => e.Model)
                .HasMaxLength(50)
                .HasColumnName("model");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNotes).HasColumnName("order_notes");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("order_status");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(10, 2)
                .HasColumnName("paid_amount");
            entity.Property(e => e.PartSum)
                .HasPrecision(10, 2)
                .HasColumnName("part_sum");
            entity.Property(e => e.PaymentsCount).HasColumnName("payments_count");
            entity.Property(e => e.ProblemDescription).HasColumnName("problem_description");
            entity.Property(e => e.RemainingAmount)
                .HasPrecision(10, 2)
                .HasColumnName("remaining_amount");
            entity.Property(e => e.RepairStartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("repair_started_at");
            entity.Property(e => e.RequiredToCloseAmount)
                .HasPrecision(10, 2)
                .HasColumnName("required_to_close_amount");
            entity.Property(e => e.SerialNumber)
                .HasMaxLength(100)
                .HasColumnName("serial_number");
            entity.Property(e => e.ServiceSum)
                .HasPrecision(10, 2)
                .HasColumnName("service_sum");
            entity.Property(e => e.TotalCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_cost");
            entity.Property(e => e.WarrantyMonths).HasColumnName("warranty_months");
        });

        modelBuilder.Entity<VwOrderPayment>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_order_payments");

            entity.Property(e => e.IsWarrantyRepair).HasColumnName("is_warranty_repair");
            entity.Property(e => e.LastPaymentDate)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_payment_date");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.PaidAmount)
                .HasPrecision(10, 2)
                .HasColumnName("paid_amount");
            entity.Property(e => e.PaymentsCount).HasColumnName("payments_count");
            entity.Property(e => e.RemainingAmount)
                .HasPrecision(10, 2)
                .HasColumnName("remaining_amount");
            entity.Property(e => e.RequiredToCloseAmount)
                .HasPrecision(10, 2)
                .HasColumnName("required_to_close_amount");
            entity.Property(e => e.TotalCost)
                .HasPrecision(10, 2)
                .HasColumnName("total_cost");
        });

        modelBuilder.Entity<VwOrderServicePartDetail>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_order_service_part_details");

            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.PartName)
                .HasMaxLength(100)
                .HasColumnName("part_name");
            entity.Property(e => e.PartNumber)
                .HasMaxLength(50)
                .HasColumnName("part_number");
            entity.Property(e => e.PriceAtMoment)
                .HasPrecision(10, 2)
                .HasColumnName("price_at_moment");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
        });

        modelBuilder.Entity<VwOrderStatusTimestamp>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_order_status_timestamps");

            entity.Property(e => e.AcceptedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("accepted_at");
            entity.Property(e => e.CanceledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("canceled_at");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IssuedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.LastStatusChangedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_status_changed_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.RepairStartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("repair_started_at");
        });

        modelBuilder.Entity<VwPartUsageStatistic>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_part_usage_statistics");

            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.PartId).HasColumnName("part_id");
            entity.Property(e => e.PartName)
                .HasMaxLength(100)
                .HasColumnName("part_name");
            entity.Property(e => e.PartNumber)
                .HasMaxLength(50)
                .HasColumnName("part_number");
            entity.Property(e => e.TotalAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_amount");
            entity.Property(e => e.TotalQuantity).HasColumnName("total_quantity");
            entity.Property(e => e.UsageCount).HasColumnName("usage_count");
        });

        modelBuilder.Entity<VwRepairDuration>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_repair_duration");

            entity.Property(e => e.AcceptedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("accepted_at");
            entity.Property(e => e.CanceledAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("canceled_at");
            entity.Property(e => e.CompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("completed_at");
            entity.Property(e => e.CreatedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("created_at");
            entity.Property(e => e.IssuedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("issued_at");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.OrderLifetimeInterval).HasColumnName("order_lifetime_interval");
            entity.Property(e => e.OrderNumber)
                .HasMaxLength(30)
                .HasColumnName("order_number");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .HasColumnName("order_status");
            entity.Property(e => e.RepairDurationDays).HasColumnName("repair_duration_days");
            entity.Property(e => e.RepairDurationHours).HasColumnName("repair_duration_hours");
            entity.Property(e => e.RepairDurationInterval).HasColumnName("repair_duration_interval");
            entity.Property(e => e.RepairStartedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("repair_started_at");
        });

        modelBuilder.Entity<VwServiceStatistic>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_service_statistics");

            entity.Property(e => e.AvgPriceAtMoment)
                .HasPrecision(10, 2)
                .HasColumnName("avg_price_at_moment");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ServiceId).HasColumnName("service_id");
            entity.Property(e => e.ServiceName)
                .HasMaxLength(100)
                .HasColumnName("service_name");
            entity.Property(e => e.TotalQuantity).HasColumnName("total_quantity");
            entity.Property(e => e.TotalRevenue)
                .HasPrecision(10, 2)
                .HasColumnName("total_revenue");
            entity.Property(e => e.UsageCount).HasColumnName("usage_count");
        });

        modelBuilder.Entity<VwTechnicianWorkload>(entity =>
        {
            entity
                .HasNoKey()
                .ToView("vw_technician_workload");

            entity.Property(e => e.AssignedServicesCount).HasColumnName("assigned_services_count");
            entity.Property(e => e.CompletedServicesCount).HasColumnName("completed_services_count");
            entity.Property(e => e.FirstCompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("first_completed_at");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .HasColumnName("first_name");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.LastCompletedAt)
                .HasColumnType("timestamp without time zone")
                .HasColumnName("last_completed_at");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .HasColumnName("last_name");
            entity.Property(e => e.Specialization)
                .HasMaxLength(100)
                .HasColumnName("specialization");
            entity.Property(e => e.TechnicianId).HasColumnName("technician_id");
            entity.Property(e => e.TotalServiceAmount)
                .HasPrecision(10, 2)
                .HasColumnName("total_service_amount");
            entity.Property(e => e.TotalServiceQuantity).HasColumnName("total_service_quantity");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
