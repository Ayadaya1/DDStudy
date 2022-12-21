using Api.Models.Attaches;
using Api.Models.User;
using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models.Posts
{
    public class PostModel
    {
        public Guid Id { get; set; }
        public List<AttachExternalModel> Attaches { get; set; } = new List<AttachExternalModel>();
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset Created { get; set; }
        public UserModel User { get; set; } = null!;
        public int Comments { get; set; } = 0;
        public int Likes { get; set; } = 0;
    }
}
