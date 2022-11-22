namespace Api.Models.Likes
{
    public class LikeModel
    {
        public string ContentType { get; set; } = string.Empty;
        public Guid ContentId { get; set; }
    }
}
