using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class OrderItem
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public int VinylId { get; set; }

    public int Quantity { get; set; }

    public decimal Price { get; set; }

}
