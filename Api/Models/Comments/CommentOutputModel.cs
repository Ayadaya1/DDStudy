using Api.Models.User;

namespace Api.Models.Comments
{
    public class CommentOutputModel
    {
        public Guid Id { get; set; }
        public UserModel Author { get; set; } = null!;
        public DateTimeOffset Created { get; set; }
        public string Text { get; set; } = string.Empty;
        public int Likes { get; set; }
    }
}
