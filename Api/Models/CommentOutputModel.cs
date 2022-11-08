namespace Api.Models
{
    public class CommentOutputModel
    {
        public string AuthorName { get; set; } = string.Empty;
        public DateTimeOffset Created { get; set; }
        public string Text { get; set; } = string.Empty;
    }
}
