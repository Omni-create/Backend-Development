using System;

namespace BusinessLogic.Classes
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Role { get; set; } // e.g., Admin, Guest, Staff
        public string PhoneNumber { get; set; }

        public User(int id, string username, string email, string passwordHash, string firstName, string lastName, string role, string phoneNumber)
        {
            Id = id;
            Username = username;
            Email = email;
            PasswordHash = passwordHash;
            FirstName = firstName;
            LastName = lastName;
            Role = role;
            PhoneNumber = phoneNumber;
        }


    }
}