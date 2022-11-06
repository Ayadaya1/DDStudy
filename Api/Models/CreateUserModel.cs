using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateUserModel
    {
        [Required]
        public string Name { get; set; } = null!;

        [Required]
        [EmailAddress(ErrorMessage = "Неправильно введён e-mail адрес!")]
        public string Email { get; set; } = null!;

        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен состоять хотя бы из 6 символов!")]
        [RegularExpression(@"(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).*",
        ErrorMessage = "Пароль должен содержать хотя бы одну заглавную, одну строчную латинскую букву и одну цифру!")]
        public string Password { get; set; } = null!;

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают!")]
        public string RetryPassword { get; set; } = null!;

        [Required]
        public DateTimeOffset BirthDate { get; set; }

    }
}
