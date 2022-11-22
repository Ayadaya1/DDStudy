using Api.Models.User;

namespace Api.Models.Comments
{
    public class CommentOutputModel
    {
        public UserModel Author { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Likes { get; set; }
    }
}
