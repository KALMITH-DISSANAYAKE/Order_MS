using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Order_MS.Models;

namespace Order_MS.Data;

public partial class OrderMSDbContext : DbContext
{
    public OrderMSDbContext()
    {
    }

    public OrderMSDbContext(DbContextOptions<OrderMSDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Branch> Branches { get; set; } = null!;

    public virtual DbSet<Driver> Drivers { get; set; } = null!;

    public virtual DbSet<DriverVehicleLink> DriverVehicleLinks { get; set; } = null!;

    public virtual DbSet<Inventory> Inventories { get; set; }

    public virtual DbSet<Item> Items { get; set; }

    public virtual DbSet<Order> Orders { get; set; }

    public virtual DbSet<OrderLine> OrderLines { get; set; }

    public virtual DbSet<OrderRequest> OrderRequests { get; set; }

    public virtual DbSet<OrderRequestLine> OrderRequestLines { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<Supplier> Suppliers { get; set; }

    public virtual DbSet<TransportAssignment> TransportAssignments { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=localhost;Database=OrderMS_DB;User Id=sa;Password=StrongPassw0rd!123;TrustServerCertificate=True;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.BranchId).HasName("PK__branch__E55E37DE9BB62DC5");

            entity.ToTable("branch");

            entity.HasIndex(e => e.BranchCode, "UQ__branch__A9F83E3BE85F3AD0").IsUnique();

            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.BranchCode)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("branch_code");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.Location)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("location");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.BranchCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_branch_created_by");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.BranchModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_branch_modified_by");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.DriverId).HasName("PK__driver__A411C5BD4CB8DB44");

            entity.ToTable("driver");

            entity.HasIndex(e => e.LicenseNumber, "UQ__driver__D482A00353AD99BD").IsUnique();

            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Available)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.DriversName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("drivers_name");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("license_number");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
        });

        modelBuilder.Entity<DriverVehicleLink>(entity =>
        {
            entity.HasKey(e => e.ConnectionId).HasName("PK__driver_v__E4AA4DD095ACF5D9");

            entity.ToTable("driver_vehicle_link");

            entity.HasIndex(e => new { e.DriverId, e.VehicleId }, "UQ_dvl_driver_vehicle").IsUnique();

            entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available")
                .HasColumnName("status");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverVehicleLinks)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dvl_driver");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DriverVehicleLinks)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_dvl_vehicle");
        });

        modelBuilder.Entity<Inventory>(entity =>
        {
            entity.HasKey(e => e.InventoryId).HasName("PK__inventor__B59ACC49AD704748");

            entity.ToTable("inventory");

            entity.HasIndex(e => new { e.BranchId, e.ItemId }, "UQ_inventory_branch_item").IsUnique();

            entity.Property(e => e.InventoryId).HasColumnName("inventory_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReorderLevel)
                .HasDefaultValue(0)
                .HasColumnName("reorder_level");

            entity.HasOne(d => d.Branch).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventory_branch");

            entity.HasOne(d => d.Item).WithMany(p => p.Inventories)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_inventory_item");
        });

        modelBuilder.Entity<Item>(entity =>
        {
            entity.HasKey(e => e.ItemId).HasName("PK__item__52020FDD875935DE");

            entity.ToTable("item");

            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.IsActive).HasColumnName("is_active");
            entity.Property(e => e.ItemName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("item_name");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.ReorderLevel)
                .HasDefaultValue(0)
                .HasColumnName("reorder_level");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.ItemCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_item_created_by");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.ItemModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_item_modified_by");

            entity.HasOne(d => d.Supplier).WithMany(p => p.Items)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_item_supplier");
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.OrderId).HasName("PK__orders__465962297B397E59");

            entity.ToTable("orders");

            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.OrderBranch).HasColumnName("order_branch");
            entity.Property(e => e.OrderRemark).HasColumnName("order_remark");
            entity.Property(e => e.OrderReqId).HasColumnName("order_req_id");
            entity.Property(e => e.OrderRequestedBy).HasColumnName("order_requested_by");
            entity.Property(e => e.OrderStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("Pending")
                .HasColumnName("order_status");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");

            entity.HasOne(d => d.Connection).WithMany(p => p.Orders)
                .HasForeignKey(d => d.ConnectionId)
                .HasConstraintName("FK_orders_connection");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.OrderCreatedByNavigations)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_orders_created_by");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.OrderModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_orders_modified_by");

            entity.HasOne(d => d.OrderBranchNavigation).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderBranch)
                .HasConstraintName("FK_Order_Branch");

            entity.HasOne(d => d.OrderReq).WithMany(p => p.Orders)
                .HasForeignKey(d => d.OrderReqId)
                .HasConstraintName("FK_orders_order_request");

            entity.HasOne(d => d.OrderRequestedByNavigation).WithMany(p => p.OrderOrderRequestedByNavigations)
                .HasForeignKey(d => d.OrderRequestedBy)
                .HasConstraintName("FK_Order_User");
        });

        modelBuilder.Entity<OrderLine>(entity =>
        {
            entity.HasKey(e => e.OrderlineId).HasName("PK__order_li__053FF212709F09A5");

            entity.ToTable("order_line");

            entity.Property(e => e.OrderlineId).HasColumnName("orderline_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OrderId).HasColumnName("order_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Item).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_ol_item");

            entity.HasOne(d => d.Order).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.OrderId)
                .HasConstraintName("FK_ol_orders");

            entity.HasOne(d => d.Supplier).WithMany(p => p.OrderLines)
                .HasForeignKey(d => d.SupplierId)
                .HasConstraintName("FK_ol_supplier");
        });

        modelBuilder.Entity<OrderRequest>(entity =>
        {
            entity.HasKey(e => e.OrderReqId).HasName("PK__order_re__0CF367CBA0BF63B6");

            entity.ToTable("order_request");

            entity.Property(e => e.OrderReqId).HasColumnName("order_req_id");
            entity.Property(e => e.ApprovedBy).HasColumnName("approved_by");
            entity.Property(e => e.ApprovedOn)
                .HasColumnType("datetime")
                .HasColumnName("approved_on");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.OrderReqRemark).HasColumnName("order_req_remark");
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.ReceivedOn)
                .HasColumnType("datetime")
                .HasColumnName("received_on");
            entity.Property(e => e.ReqStatus)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasDefaultValue("SubmittedForReview")
                .HasColumnName("req_status");
            entity.Property(e => e.RequestedBy).HasColumnName("requested_by");
            entity.Property(e => e.RequestedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("requested_on");
            entity.Property(e => e.TotalPrice)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("total_price");
            entity.Property(e => e.TotalQuantity).HasColumnName("total_quantity");

            entity.HasOne(d => d.ApprovedByNavigation).WithMany(p => p.OrderRequestApprovedByNavigations)
                .HasForeignKey(d => d.ApprovedBy)
                .HasConstraintName("FK_order_request_approved_by");

            entity.HasOne(d => d.Branch).WithMany(p => p.OrderRequests)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_request_branch");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.OrderRequestModifiedByNavigations)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_order_request_modified_by");

            entity.HasOne(d => d.RequestedByNavigation).WithMany(p => p.OrderRequestRequestedByNavigations)
                .HasForeignKey(d => d.RequestedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_order_request_requested_by");
        });

        modelBuilder.Entity<OrderRequestLine>(entity =>
        {
            entity.HasKey(e => e.OrderReqLineId).HasName("PK__order_re__7C549BF1D7563A98");

            entity.ToTable("order_request_line");

            entity.Property(e => e.OrderReqLineId).HasColumnName("order_req_line_id");
            entity.Property(e => e.ItemId).HasColumnName("item_id");
            entity.Property(e => e.OrderReqId).HasColumnName("order_req_id");
            entity.Property(e => e.Price)
                .HasColumnType("decimal(18, 2)")
                .HasColumnName("price");
            entity.Property(e => e.Quantity).HasColumnName("quantity");

            entity.HasOne(d => d.Item).WithMany(p => p.OrderRequestLines)
                .HasForeignKey(d => d.ItemId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_orl_item");

            entity.HasOne(d => d.OrderReq).WithMany(p => p.OrderRequestLines)
                .HasForeignKey(d => d.OrderReqId)
                .HasConstraintName("FK_orl_order_request");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.RoleId).HasName("PK__role__760965CC97EAD1AE");

            entity.ToTable("role");

            entity.HasIndex(e => e.RoleName, "UQ__role__783254B16CE0AC3C").IsUnique();

            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.Description)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("description");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.RoleName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("role_name");
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.HasKey(e => e.SupplierId).HasName("PK__supplier__6EE594E8290368ED");

            entity.ToTable("supplier");

            entity.Property(e => e.SupplierId).HasColumnName("supplier_id");
            entity.Property(e => e.Availability)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("availability");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.SupplierName)
                .HasMaxLength(100)
                .IsUnicode(false)
                .HasColumnName("supplier_name");
        });

        modelBuilder.Entity<TransportAssignment>(entity =>
        {
            entity.HasKey(e => e.AssignmentId).HasName("PK__Transpor__DA8918147F3960DE");

            entity.ToTable("TransportAssignment");

            entity.Property(e => e.AssignmentId).HasColumnName("assignment_id");
            entity.Property(e => e.AssignedOn).HasColumnName("assigned_on");
            entity.Property(e => e.ConnectionId).HasColumnName("connection_id");
            entity.Property(e => e.OrderReqId).HasColumnName("order_req_id");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");

            entity.HasOne(d => d.Connection).WithMany(p => p.TransportAssignments)
                .HasForeignKey(d => d.ConnectionId)
                .HasConstraintName("FK_TransportAssignment_driver_vehicle_link");

            entity.HasOne(d => d.OrderReq).WithMany(p => p.TransportAssignments)
                .HasForeignKey(d => d.OrderReqId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_TransportAssignment_order_request");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F5AA6C93C");

            entity.ToTable("user");

            entity.HasIndex(e => e.UserName, "UQ__user__7C9273C49EA7109E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedBy).HasColumnName("created_by");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.FirstName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("first_name");
            entity.Property(e => e.LastName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("last_name");
            entity.Property(e => e.ModifiedBy).HasColumnName("modified_by");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password_hash");
            entity.Property(e => e.RoleId).HasColumnName("role_id");
            entity.Property(e => e.UserName)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("user_name");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_user_branch");

            entity.HasOne(d => d.CreatedByNavigation).WithMany(p => p.InverseCreatedByNavigation)
                .HasForeignKey(d => d.CreatedBy)
                .HasConstraintName("FK_user_created_by");

            entity.HasOne(d => d.ModifiedByNavigation).WithMany(p => p.InverseModifiedByNavigation)
                .HasForeignKey(d => d.ModifiedBy)
                .HasConstraintName("FK_user_modified_by");

            entity.HasOne(d => d.Role).WithMany(p => p.Users)
                .HasForeignKey(d => d.RoleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_user_role");
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.VehicleId).HasName("PK__vehicle__F2947BC10D5E57E9");

            entity.ToTable("vehicle");

            entity.HasIndex(e => e.VehicleNumber, "UQ__vehicle__2D703C2ADEBCD0FB").IsUnique();

            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.Available)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasDefaultValue("Available");
            entity.Property(e => e.Capacity).HasColumnName("capacity");
            entity.Property(e => e.CreatedOn)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_on");
            entity.Property(e => e.ModifiedOn)
                .HasColumnType("datetime")
                .HasColumnName("modified_on");
            entity.Property(e => e.VehicleNumber)
                .HasMaxLength(20)
                .IsUnicode(false)
                .HasColumnName("vehicle_number");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
