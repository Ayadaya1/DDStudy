using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Subscription
    {
        public Guid Id { get; set; }
        public User Subscriber { get; set; } = null!;

        public virtual User Target { get; set; } = null!;


    }
}
