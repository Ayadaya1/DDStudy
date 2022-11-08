using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Post
    {
        public Guid Id { get; set; }
        public DateTimeOffset Created { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<PostAttach> Attaches { get; set; } = new List<PostAttach>();
        public string Text { get; set; } = string.Empty;

        public virtual User User { get; set; } = null!;
    }
}
