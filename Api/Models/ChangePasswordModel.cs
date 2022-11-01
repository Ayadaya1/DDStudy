using System.ComponentModel.DataAnnotations;

namespace Api.Models
{
    public class ChangePasswordModel
    {
        [Required]
        [RegularExpression(@"(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z]).*",
        ErrorMessage = "Пароль должен содержать хотя бы одну заглавную, одну строчную латинскую букву и одну цифру!")]
        public string NewPassword { get; set; }
        
        public ChangePasswordModel(string newPassword)
        {
            NewPassword = newPassword;
        }
    }
}
