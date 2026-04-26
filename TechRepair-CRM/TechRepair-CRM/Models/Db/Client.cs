using System;
using System.Collections.Generic;

namespace TechRepair_CRM.Models.Db;

public partial class Client
{
    public int ClientId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public string? Email { get; set; }

    public string? Address { get; set; }

    public DateTime RegistrationDate { get; set; }

    public string? Notes { get; set; }

    public virtual ICollection<Device> Devices { get; set; } = new List<Device>();
}
