using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DAL.Entities
{
    public class Comment
    {
        public Guid Id { get; set; }
        public string Text { get; set; } = "";
        public DateTimeOffset Created { get; set; }

        public virtual User User { get; set; } = null!;
    }
}
