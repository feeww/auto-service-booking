namespace AutoServiceBooking.Web.Services.Bookings
{
    public class BookingOperationResult
    {
        private BookingOperationResult(bool success, bool notFound, string? errorMessage, string? fieldName)
        {
            Success = success;
            NotFound = notFound;
            ErrorMessage = errorMessage;
            FieldName = fieldName;
        }

        public bool Success { get; }

        public bool NotFound { get; }

        public string? ErrorMessage { get; }

        public string? FieldName { get; }

        public static BookingOperationResult Ok()
        {
            return new BookingOperationResult(true, false, null, null);
        }

        public static BookingOperationResult Fail(string errorMessage, string? fieldName = null)
        {
            return new BookingOperationResult(false, false, errorMessage, fieldName);
        }

        public static BookingOperationResult Missing()
        {
            return new BookingOperationResult(false, true, null, null);
        }
    }
}
