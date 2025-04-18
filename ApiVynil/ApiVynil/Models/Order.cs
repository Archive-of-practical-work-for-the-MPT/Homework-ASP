using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class Order
{
    public int Id { get; set; }

    public int UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public decimal TotalAmount { get; set; }

    public int StatusId { get; set; }

    public string? ShippingAddress { get; set; }
}
