using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Models.Users;

namespace AutoServiceBooking.Web.Models
{
    public class Booking
    {
        protected Booking()
        {
        }

        public Booking(int clientUserId, int vehicleId, int autoServiceId, DateTime scheduledAt, string? problemDescription)
        {
            Validate(clientUserId, vehicleId, autoServiceId, scheduledAt);

            ClientUserId = clientUserId;
            VehicleId = vehicleId;
            AutoServiceId = autoServiceId;
            ScheduledAt = scheduledAt;
            ProblemDescription = problemDescription?.Trim();
            Status = BookingStatus.Pending;
        }

        public int Id { get; private set; }

        public int ClientUserId { get; private set; }

        public ClientUser ClientUser { get; private set; } = null!;

        public int VehicleId { get; private set; }

        public Vehicle Vehicle { get; private set; } = null!;

        public int AutoServiceId { get; private set; }

        public AutoService AutoService { get; private set; } = null!;

        public DateTime ScheduledAt { get; private set; }

        [StringLength(1000)]
        public string? ProblemDescription { get; private set; }

        public decimal? FinalPrice { get; private set; }

        [StringLength(1000)]
        public string? AdminComment { get; private set; }

        public BookingStatus Status { get; private set; } = BookingStatus.Pending;

        public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;

        public DateTime? CompletedAt { get; private set; }

        public void UpdateDetails(int vehicleId, int autoServiceId, DateTime scheduledAt, string? problemDescription)
        {
            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Редагувати можна тільки записи зі статусом 'Очікує'.");
            }

            Validate(ClientUserId, vehicleId, autoServiceId, scheduledAt);

            VehicleId = vehicleId;
            AutoServiceId = autoServiceId;
            ScheduledAt = scheduledAt;
            ProblemDescription = problemDescription?.Trim();
        }

        public void Confirm()
        {
            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Підтвердити можна тільки записи зі статусом 'Очікує'.");
            }

            Status = BookingStatus.Confirmed;
        }

        public void UpdateAdminComment(string? adminComment)
        {
            AdminComment = adminComment?.Trim();
        }

        private void SetFinalPrice(decimal finalPrice)
        {
            if (finalPrice < 0)
            {
                throw new ArgumentException("Фінальна ціна не може бути від'ємною.", nameof(finalPrice));
            }

            FinalPrice = finalPrice;
        }

        public void Reject()
        {
            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Відхилити можна тільки записи зі статусом 'Очікує'.");
            }

            Status = BookingStatus.Rejected;
        }

        public void StartWork()
        {
            if (Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Почати роботу можна тільки для підтвердженого запису.");
            }

            Status = BookingStatus.InProgress;
        }

        private void Complete()
        {
            if (Status != BookingStatus.InProgress)
            {
                throw new InvalidOperationException("Завершити можна тільки запис, який перебуває в роботі.");
            }

            Status = BookingStatus.Completed;
            CompletedAt = DateTime.UtcNow;
        }

        public void Complete(decimal finalPrice, string? adminComment)
        {
            SetFinalPrice(finalPrice);
            UpdateAdminComment(adminComment);
            Complete();
        }

        public void Cancel()
        {
            if (Status == BookingStatus.Completed)
            {
                throw new InvalidOperationException("Завершений запис не можна скасувати.");
            }

            if (Status == BookingStatus.Rejected)
            {
                throw new InvalidOperationException("Відхилений запис не можна скасувати.");
            }

            Status = BookingStatus.Cancelled;
        }

        private static void Validate(int clientUserId, int vehicleId, int autoServiceId, DateTime scheduledAt)
        {
            if (clientUserId <= 0)
            {
                throw new ArgumentException("Клієнт обов'язковий.", nameof(clientUserId));
            }

            if (vehicleId <= 0)
            {
                throw new ArgumentException("Автомобіль обов'язковий.", nameof(vehicleId));
            }

            if (autoServiceId <= 0)
            {
                throw new ArgumentException("Послуга обов'язкова.", nameof(autoServiceId));
            }

            if (scheduledAt == default)
            {
                throw new ArgumentException("Дата та час запису обов'язкові.", nameof(scheduledAt));
            }
        }
    }
}
