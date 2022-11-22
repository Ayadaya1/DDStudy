using Api.Models.User;

namespace Api.Models.Attaches
{
    public class LikeableAvatarModel
    {
        public UserModel User { get; set; } = null!;
        public string? Avatar { get; set; }
        public int Likes { get; set; } = 0;
    }
}
