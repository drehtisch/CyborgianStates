using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Models
{
    public class User
    {
        public ObjectId Id { get; set; }
        public ulong DiscordUserId { get; set; }
        public List<ObjectId> Nations { get; set; } = new List<ObjectId>();
        public List<Permission> Permissions { get; set; } = new List<Permission>();
        public List<ObjectId> Roles { get; set; } = new List<ObjectId>();
    }
}
