namespace AutoServiceBooking.Web.Validation
{
    public static class UserValidationRules
    {
        public const int FullNameMinLength = 2;
        public const int FullNameMaxLength = 100;
        public const int EmailMaxLength = 100;
        public const int PhoneMaxLength = 30;
        public const int PasswordMinLength = 8;
        public const int PasswordMaxLength = 100;

        public const string FullNameRegex = @"^[A-Za-zА-Яа-яІіЇїЄєҐґ'’\-\s]+$";
        public const string PhoneRegex = @"^(\+?380|0)[\s\-]?\d{2}[\s\-]?\d{3}[\s\-]?\d{2}[\s\-]?\d{2}$";
        public const string PasswordRegex = @"^(?=.*[A-Za-zА-Яа-яІіЇїЄєҐґ])(?=.*\d).+$";

        public const string FullNameRequiredMessage = "Вкажіть ім'я";
        public const string FullNameLengthMessage = "Ім'я має містити від 2 до 100 символів";
        public const string FullNameRegexMessage = "Ім'я може містити тільки літери, пробіли, апостроф або дефіс";

        public const string EmailRequiredMessage = "Вкажіть email";
        public const string EmailInvalidMessage = "Некоректний email";
        public const string EmailLengthMessage = "Email може містити максимум 100 символів";

        public const string PhoneRequiredMessage = "Вкажіть телефон";
        public const string PhoneRegexMessage = "Вкажіть телефон у форматі +380XXXXXXXXX або 0XXXXXXXXX";
        public const string PhoneLengthMessage = "Телефон може містити максимум 30 символів";

        public const string PasswordRequiredMessage = "Вкажіть пароль";
        public const string PasswordLengthMessage = "Пароль має містити від 8 до 100 символів";
        public const string NewPasswordLengthMessage = "Новий пароль має містити від 8 до 100 символів";
        public const string PasswordRegexMessage = "Пароль має містити хоча б одну літеру та одну цифру";
        public const string NewPasswordRegexMessage = "Новий пароль має містити хоча б одну літеру та одну цифру";
        public const string PasswordCompareMessage = "Паролі не співпадають";
    }
}
