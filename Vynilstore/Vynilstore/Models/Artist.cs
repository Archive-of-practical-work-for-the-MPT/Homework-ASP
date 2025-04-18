using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class Artist
{
    public int Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string? Country { get; set; }
}
