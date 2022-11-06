namespace Api.Models
{
    public class TokenRequestModel
    {
        public string Login { get; set; } = null!;
        public string Password { get; set; } = null!;

    }

    public class RefreshTokenRequestModel
    {
        public string RefreshToken { get; set; } = null!; 
    }
}
