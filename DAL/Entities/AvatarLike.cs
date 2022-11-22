using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class AvatarLike : Like
    {
        public Avatar Avatar { get; set; } = null!;
        public Guid AvatarId { get; set; }
    }
}
