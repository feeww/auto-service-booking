using System.ComponentModel.DataAnnotations;
using AutoServiceBooking.Web.Models.Users;

namespace AutoServiceBooking.Web.Models
{
    public class Booking
    {
        protected Booking()
        {
        }

        public Booking(
            int clientUserId,
            int vehicleId,
            int autoServiceId,
            DateTime scheduledAt,
            string? problemDescription,
            string customerName,
            string customerPhone,
            string? customerEmail)
        {
            ValidateLinkedBooking(clientUserId, vehicleId, autoServiceId, scheduledAt);
            ValidateCustomer(customerName, customerPhone);

            ClientUserId = clientUserId;
            VehicleId = vehicleId;
            AutoServiceId = autoServiceId;
            ScheduledAt = scheduledAt;
            ProblemDescription = problemDescription?.Trim();
            CustomerName = customerName.Trim();
            CustomerPhone = customerPhone.Trim();
            CustomerEmail = customerEmail?.Trim();
            Status = BookingStatus.Pending;
        }

        public Booking(
            int autoServiceId,
            DateTime scheduledAt,
            string? problemDescription,
            string customerName,
            string customerPhone,
            string? customerEmail,
            string vehicleMake,
            string vehicleModel,
            int vehicleYear,
            string vehicleLicensePlate,
            int vehicleMileage,
            VehicleFuelType vehicleFuelType)
        {
            ValidateCore(autoServiceId, scheduledAt);
            ValidateCustomer(customerName, customerPhone);
            ValidateGuestVehicle(vehicleMake, vehicleModel, vehicleYear, vehicleLicensePlate, vehicleMileage);

            AutoServiceId = autoServiceId;
            ScheduledAt = scheduledAt;
            ProblemDescription = problemDescription?.Trim();
            CustomerName = customerName.Trim();
            CustomerPhone = customerPhone.Trim();
            CustomerEmail = customerEmail?.Trim();
            GuestVehicleMake = vehicleMake.Trim();
            GuestVehicleModel = vehicleModel.Trim();
            GuestVehicleYear = vehicleYear;
            GuestVehicleLicensePlate = vehicleLicensePlate.Trim().ToUpperInvariant();
            GuestVehicleMileage = vehicleMileage;
            GuestVehicleFuelType = vehicleFuelType;
            Status = BookingStatus.Pending;
        }

        public int Id { get; private set; }

        public int? ClientUserId { get; private set; }

        public ClientUser? ClientUser { get; private set; }

        public int? VehicleId { get; private set; }

        public Vehicle? Vehicle { get; private set; }

        public int AutoServiceId { get; private set; }

        public AutoService AutoService { get; private set; } = null!;

        public DateTime ScheduledAt { get; private set; }

        [StringLength(1000)]
        public string? ProblemDescription { get; private set; }

        [Required]
        [StringLength(100)]
        public string CustomerName { get; private set; } = string.Empty;

        [Required]
        [StringLength(30)]
        public string CustomerPhone { get; private set; } = string.Empty;

        [EmailAddress]
        [StringLength(100)]
        public string? CustomerEmail { get; private set; }

        [StringLength(60)]
        public string? GuestVehicleMake { get; private set; }

        [StringLength(60)]
        public string? GuestVehicleModel { get; private set; }

        public int? GuestVehicleYear { get; private set; }

        [StringLength(20)]
        public string? GuestVehicleLicensePlate { get; private set; }

        public int? GuestVehicleMileage { get; private set; }

        public VehicleFuelType? GuestVehicleFuelType { get; private set; }

        public decimal? FinalPrice { get; private set; }

        public decimal? EstimatedPrice { get; private set; }

        public int? EstimatedDurationMinutes { get; private set; }

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

            if (ClientUserId == null)
            {
                throw new InvalidOperationException("Гостьовий запис не можна редагувати як запис збереженого авто.");
            }

            ValidateLinkedBooking(ClientUserId.Value, vehicleId, autoServiceId, scheduledAt);

            VehicleId = vehicleId;
            AutoServiceId = autoServiceId;
            ScheduledAt = scheduledAt;
            ProblemDescription = problemDescription?.Trim();
        }

        public void Confirm(decimal estimatedPrice, int estimatedDurationMinutes)
        {
            if (Status != BookingStatus.Pending)
            {
                throw new InvalidOperationException("Підтвердити можна тільки записи зі статусом 'Очікує'.");
            }

            SetEstimate(estimatedPrice, estimatedDurationMinutes);
            Status = BookingStatus.Confirmed;
        }

        private void SetEstimate(decimal estimatedPrice, int estimatedDurationMinutes)
        {
            if (estimatedPrice < 0)
            {
                throw new ArgumentException("Орієнтовна ціна не може бути від'ємною.", nameof(estimatedPrice));
            }

            if (estimatedDurationMinutes <= 0)
            {
                throw new ArgumentException("Орієнтовний час виконання має бути більшим за 0 хвилин.", nameof(estimatedDurationMinutes));
            }

            EstimatedPrice = estimatedPrice;
            EstimatedDurationMinutes = estimatedDurationMinutes;
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
            if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Відхилити можна тільки записи, які очікують або підтверджені.");
            }

            Status = BookingStatus.Rejected;
        }

        public void Reschedule(DateTime scheduledAt)
        {
            if (Status != BookingStatus.Pending && Status != BookingStatus.Confirmed)
            {
                throw new InvalidOperationException("Перенести можна тільки записи, які очікують або підтверджені.");
            }

            ValidateCore(AutoServiceId, scheduledAt);
            ScheduledAt = scheduledAt;
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

        private static void ValidateLinkedBooking(int clientUserId, int vehicleId, int autoServiceId, DateTime scheduledAt)
        {
            if (clientUserId <= 0)
            {
                throw new ArgumentException("Клієнт обов'язковий.", nameof(clientUserId));
            }

            if (vehicleId <= 0)
            {
                throw new ArgumentException("Автомобіль обов'язковий.", nameof(vehicleId));
            }

            ValidateCore(autoServiceId, scheduledAt);
        }

        private static void ValidateCore(int autoServiceId, DateTime scheduledAt)
        {
            if (autoServiceId <= 0)
            {
                throw new ArgumentException("Послуга обов'язкова.", nameof(autoServiceId));
            }

            if (scheduledAt == default)
            {
                throw new ArgumentException("Дата та час запису обов'язкові.", nameof(scheduledAt));
            }
        }

        private static void ValidateCustomer(string customerName, string customerPhone)
        {
            if (string.IsNullOrWhiteSpace(customerName))
            {
                throw new ArgumentException("Ім'я клієнта обов'язкове.", nameof(customerName));
            }

            if (string.IsNullOrWhiteSpace(customerPhone))
            {
                throw new ArgumentException("Телефон клієнта обов'язковий.", nameof(customerPhone));
            }
        }

        private static void ValidateGuestVehicle(string make, string model, int year, string licensePlate, int mileage)
        {
            if (string.IsNullOrWhiteSpace(make))
            {
                throw new ArgumentException("Марка автомобіля обов'язкова.", nameof(make));
            }

            if (string.IsNullOrWhiteSpace(model))
            {
                throw new ArgumentException("Модель автомобіля обов'язкова.", nameof(model));
            }

            if (year < 1900 || year > DateTime.UtcNow.Year + 1)
            {
                throw new ArgumentException("Рік випуску автомобіля некоректний.", nameof(year));
            }

            if (string.IsNullOrWhiteSpace(licensePlate))
            {
                throw new ArgumentException("Номерний знак обов'язковий.", nameof(licensePlate));
            }

            if (mileage < 0)
            {
                throw new ArgumentException("Пробіг не може бути від'ємним.", nameof(mileage));
            }
        }
    }
}
