using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Like
    {
        public User? User { get; set; }


        public Guid UserId { get; set; }
    }
}
