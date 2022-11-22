using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class CommentLike : Like
    {
        public Comment Comment { get; set; } = null!;
        public Guid CommentId { get; set; }

    }
}
