using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class Track
{
    public int Id { get; set; }

    public int VinylId { get; set; }

    public string Title { get; set; } = null!;

    public string? Duration { get; set; }

    public int TrackNumber { get; set; }

    public string Side { get; set; } = null!;
}
