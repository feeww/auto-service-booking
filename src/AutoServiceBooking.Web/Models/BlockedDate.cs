using System.ComponentModel.DataAnnotations;

namespace AutoServiceBooking.Web.Models
{
    public class BlockedDate
    {
        protected BlockedDate()
        {
        }

        public BlockedDate(DateTime date, string? reason)
        {
            SetDate(date);
            SetReason(reason);
        }

        public int Id { get; private set; }

        public DateTime Date { get; private set; }

        [Required]
        [StringLength(200)]
        public string Reason { get; private set; } = string.Empty;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        private void SetDate(DateTime date)
        {
            if (date == default)
            {
                throw new ArgumentException("Дата обов'язкова.", nameof(date));
            }

            Date = date.Date;
        }

        private void SetReason(string? reason)
        {
            string preparedReason = string.IsNullOrWhiteSpace(reason)
                ? "Сервіс не приймає записи цього дня"
                : reason.Trim();

            if (preparedReason.Length > 200)
            {
                throw new ArgumentException("Причина може містити максимум 200 символів.", nameof(reason));
            }

            Reason = preparedReason;
        }
    }
}
