﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using DAL.Entities;

namespace DAL.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public DateTimeOffset BirthDate { get; set; }
        public Guid? AvatarId { get; set; }
        //public Guid PrivacySettingsId { get; set; }

        public virtual Avatar? Avatar { get; set; }
        public virtual ICollection<UserSession>? Sessions { get; set; }
        public virtual List<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public virtual ICollection<Subscription> Subscribers { get; set; } = new List<Subscription>();

        [Required]
        public virtual PrivacySettings PrivacySettings { get; set; } = new PrivacySettings();

    }
}
