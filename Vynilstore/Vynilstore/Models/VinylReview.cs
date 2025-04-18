using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class VinylReview
{
    public int Id { get; set; }

    public int VinylId { get; set; }

    public int UserId { get; set; }

    public int Rating { get; set; }

    public string? ReviewText { get; set; }

    public DateTime? CreatedAt { get; set; }

}
