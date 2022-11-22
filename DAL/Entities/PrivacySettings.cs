using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Common.Enums;

namespace DAL.Entities
{
    public class PrivacySettings
    {
        //public Guid Id { get; set; }
        public Privacy AvatarAccess { get; set; }
        public Privacy PostAccess { get; set; }
        public Privacy MessageAccess { get; set; } //На будущее
        public Privacy CommentAccess { get; set; }

        public virtual User User { get; set; } = null!;

        public PrivacySettings()
        {
            AvatarAccess = Privacy.Everybody;
            PostAccess = Privacy.Everybody;
            MessageAccess = Privacy.Everybody;
            CommentAccess = Privacy.Everybody;
            //Id = Guid.NewGuid();
        }
        [ForeignKey("User")]
        public Guid UserId { get; set; }
    }
}
