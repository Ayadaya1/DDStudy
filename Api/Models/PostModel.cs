using DAL.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Api.Models
{
    public class PostModel
    {
        public List<string> Attaches { get; set; } = new List<string>();
        public string Text { get; set; } = string.Empty;
        public DateTimeOffset Created { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Comments { get; set; } = 0;
    }
}
