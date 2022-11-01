using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class CreateUserModel
    {
        [Required]
        public string Name { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Неправильно введён e-mail адрес!")]
        public string Email { get; set; }

        [Required]
        [StringLength(20, MinimumLength = 6, ErrorMessage = "Пароль должен состоять хотя бы из 6 символов!")]
        [RegularExpression(@"(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).*", 
        ErrorMessage = "Пароль должен содержать хотя бы одну заглавную, одну строчную латинскую букву и одну цифру!")]
        public string Password { get; set; }

        [Required]
        [Compare(nameof(Password), ErrorMessage = "Пароли не совпадают!")]
        public string RetryPassword { get; set; }

        [Required]
        public DateTimeOffset BirthDate { get; set; }

        public CreateUserModel(string name, string email, string password, string retryPassword, DateTimeOffset birthDate)
        {
            Name = name;
            Email = email;
            Password = password;
            RetryPassword = retryPassword;
            BirthDate = birthDate;
        }
    }
}
