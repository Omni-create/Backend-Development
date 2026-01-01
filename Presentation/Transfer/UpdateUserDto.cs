using System;
using System.ComponentModel.DataAnnotations;

namespace Backend_Dev.Transfer;

public class UpdateUserDto
{
    [Required]
    public string Username { get; set; } = null!;

    public string? Password { get; set; }

    [Required]
    public string FirstName { get; set; } = null!;

    [Required]
    public string LastName { get; set; } = null!;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = null!;

    [Phone]
    public string? Phone { get; set; }

    public UserRoleDto? UserRole { get; set; }
}
