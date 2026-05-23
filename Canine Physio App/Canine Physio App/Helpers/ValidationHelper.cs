namespace Canine_Physio_App.Helpers
{
    /// <summary>
    /// Shared validation methods used across authentication pages.
    /// Eliminates duplication of validation logic (DRY principle).
    /// </summary>
    public static class ValidationHelper
    {
        /// <summary>
        /// Validates email format using System.Net.Mail.MailAddress.
        /// </summary>
        /// <param name="email">The email address to validate.</param>
        /// <returns>True if the email format is valid; otherwise false.</returns>
        public static bool IsValidEmail(string email)
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
    }
}
