using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public DateTimeOffset BirthDate { get; set; }
        public Guid? AvatarId { get; set; }

        public virtual Avatar? Avatar {get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual List<Post> Posts { get; set; } = new List<Post>();
    }
}
