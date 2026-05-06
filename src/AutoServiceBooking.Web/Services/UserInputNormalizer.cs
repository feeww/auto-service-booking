namespace AutoServiceBooking.Web.Services
{
    public static class UserInputNormalizer
    {
        public static string NormalizeEmail(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        public static string? NormalizePhoneNumber(string? phoneNumber)
        {
            if (string.IsNullOrWhiteSpace(phoneNumber))
            {
                return null;
            }

            string normalizedPhone = phoneNumber
                .Trim()
                .Replace(" ", string.Empty)
                .Replace("-", string.Empty);

            if (normalizedPhone.StartsWith("+380", StringComparison.Ordinal))
            {
                return normalizedPhone;
            }

            if (normalizedPhone.StartsWith("380", StringComparison.Ordinal))
            {
                return $"+{normalizedPhone}";
            }

            if (normalizedPhone.StartsWith("0", StringComparison.Ordinal))
            {
                return $"+380{normalizedPhone[1..]}";
            }

            return normalizedPhone;
        }
    }
}
