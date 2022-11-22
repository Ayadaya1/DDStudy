using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Avatar : Attach, ILikeable
    {
        public virtual User User { get; set; } = null!;

        public virtual ICollection<AvatarLike> Likes { get; set; } = new List<AvatarLike>();
    }
}
