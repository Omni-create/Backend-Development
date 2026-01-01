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
            if (number <= 0)
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

        /// <summary>
        /// Validates user role against allowed values: Guest, Admin, Manager, Staff
        /// </summary>
        public string CheckRole(string role)
        {
            string[] validRoles = { "Guest", "Admin", "Manager", "Staff" };
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

        /// <summary>
        /// Validates room status against allowed values: Available, Occupied, Maintenance
        /// </summary>
        public string CheckRoomStatus(string status)
        {
            string[] validStatuses = { "Available", "Occupied", "Maintenance" };
            foreach (var validStatus in validStatuses)
            {
                if (status == validStatus)
                {
                    return status;
                }
            }
            return "Available"; // Default status
        }

        /// <summary>
        /// Validates reservation status against allowed values: Pending, Confirmed, CheckedIn, CheckedOut, Cancelled
        /// </summary>
        public string CheckReservationStatus(string status)
        {
            string[] validStatuses = { "Pending", "Confirmed", "CheckedIn", "CheckedOut", "Cancelled" };
            foreach (var validStatus in validStatuses)
            {
                if (status == validStatus)
                {
                    return status;
                }
            }
            return "Pending"; // Default status
        }

        /// <summary>
        /// Validates payment status against allowed values: Pending, Confirmed, Paid, Cancelled
        /// </summary>
        public string CheckPaymentStatus(string status)
        {
            string[] validStatuses = { "Pending", "Confirmed", "Paid", "Cancelled" };
            foreach (var validStatus in validStatuses)
            {
                if (status == validStatus)
                {
                    return status;
                }
            }
            return "Pending"; // Default status
        }

        /// <summary>
        /// Validates date range: ensures start date is before end date and end date is in the future
        /// </summary>
        public bool IsValidDateRange(DateOnly startDate, DateOnly endDate)
        {
            var today = DateOnly.FromDateTime(DateTime.Now);
            return startDate >= today && endDate > startDate;
        }

        /// <summary>
        /// Validates that the reservation period doesn't exceed maximum allowed days
        /// </summary>
        public bool IsValidReservationLength(DateOnly startDate, DateOnly endDate, int maxDays = 365)
        {
            var days = (endDate.ToDateTime(TimeOnly.MinValue) - startDate.ToDateTime(TimeOnly.MinValue)).Days;
            return days <= maxDays;
        }
    }
}
