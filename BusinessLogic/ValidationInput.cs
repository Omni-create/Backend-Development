namespace BusinessLogic.ValidationInput
{
    public class ValidationInput
    {
        public bool IsBlank(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return false;
            }

            return true;
        }
        public bool IsPositiveNumber(double number)
        {
            if (number < 0)
            {
                return false;
            }

            return true;
        }
        public bool IsInt(string input)
        {
            int result;
            return int.TryParse(input, out result);
        }
        public bool IsDouble(string input)
        {
            double result;
            return double.TryParse(input, out result);
        }
        public string CheckRole(string role)
        {
            string[] validRoles = { "Admin", "Guest", "Staff", "Owner" };
            foreach (var validRole in validRoles)
            {
                if (role == validRole)
                {
                    return role;
                }
            }
            return "Guest"; // Default role
        }
        public bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }
        public string CheckRoomStatus(string status)
        {
            string[] validStatuses = { "Available", "Occupied", "Maintenance", "Cancelled" };
            foreach (var validStatus in validStatuses)
            {
                if (status == validStatus)
                {
                    return status;
                }
            }
            return "Available"; // Default status
        }
        public string CheckReservationStatus(string status)
        {
            string[] validStatuses = { "Confirmed", "Cancelled", "Completed", "Pending" };
            foreach (var validStatus in validStatuses)
            {
                if (status == validStatus)
                {
                    return status;
                }
            }
            return "Pending"; // Default status
        }
    }
}