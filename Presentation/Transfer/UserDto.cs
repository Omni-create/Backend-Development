using System;
using System.Collections.Generic;

namespace Presentation.Transfer;

public enum UserRoleDto
{
    Guest,
    Admin,
    Manager,
    Staff
}

public class UserDto
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public DateTime? CreatedDate { get; set; }
    public UserRoleDto UserRole { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string Email { get; set; }
    public string Phone { get; set; }
}