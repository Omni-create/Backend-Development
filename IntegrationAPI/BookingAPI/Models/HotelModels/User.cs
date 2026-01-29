using System.ComponentModel.DataAnnotations;
namespace HotelApi.Models;

public enum UserRole
{
    Guest,
    Admin,
    Manager,
    Staff
}
public partial class User
{
    public int UserId { get; set; }
    [Required]
    public string Username { get; set; } = null!;
    [Required]
    public string Password { get; set; } = null!;
    public DateTime? CreatedDate { get; set; }
    public UserRole UserRole { get; set; }
    [Required]
    public string FirstName { get; set; } = null!;
    [Required]
    public string LastName { get; set; } = null!;
    [Required]
    public string Email { get; set; } = null!;

    public string Phone { get; set; } = null!;

    public virtual ICollection<PaymentInfo> PaymentInfos { get; set; } = new List<PaymentInfo>();

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    public User()
    {
        PaymentInfos = new List<PaymentInfo>();
        Reservations = new List<Reservation>();
        UserRole = UserRole.Guest;
    }
}
