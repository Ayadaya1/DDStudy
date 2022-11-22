using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Entities
{
    public class PostLike : Like
    {
        public virtual Post Post { get; set; } = null!;
        public Guid PostId { get; set; }
    }
}
