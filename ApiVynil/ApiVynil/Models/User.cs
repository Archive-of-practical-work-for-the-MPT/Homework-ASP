using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;

namespace ApiVynil.Models;

public partial class User
{
    public int Id { get; set; }

    public string Login { get; set; } = null!;

    public string Password { get; set; } = null!;

    public int RoleId { get; set; }

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public DateOnly? BirthDate { get; set; }

    public string? Email { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

	public void HashPassword(string password)
	{
		Password = BCrypt.Net.BCrypt.HashPassword(password);
	}


	public bool VerifyPassword(string password)
	{
		return BCrypt.Net.BCrypt.Verify(password, Password);
	}
}
