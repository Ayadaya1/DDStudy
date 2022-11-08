using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class PostAttach : Attach
    {


        public virtual Post Post { get; set; } = null!;
    }
}
