using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class Device
{
    public int DeviceId { get; set; }

    public int ClientId { get; set; }

    public int DeviceTypeId { get; set; }

    public string? Brand { get; set; }

    public string? Model { get; set; }

    public string? SerialNumber { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public string? EquipmentDescription { get; set; }

    public string? ExternalCondition { get; set; }

    public string? Notes { get; set; }

    public virtual Client Client { get; set; } = null!;

    public virtual DeviceType DeviceType { get; set; } = null!;

    public virtual ICollection<RepairOrder> RepairOrders { get; set; } = new List<RepairOrder>();
}
