using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class Vinyl
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;

    public int ArtistId { get; set; }

    public int GenreId { get; set; }

    public int LabelId { get; set; }

    public int? ReleaseYear { get; set; }

    public string? CoverImagePath { get; set; }

    public decimal Price { get; set; }

    public decimal? Weight { get; set; }

    public int Diameter { get; set; }

    public int Rpm { get; set; }

    public string? Condition { get; set; }

    public int QuantityInStock { get; set; }

}
