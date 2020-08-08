using Dapper.Contrib.Extensions;
using System;
using System.Collections.Generic;
using System.Text;

namespace CyborgianStates.Models
{

    [Table("User")]
    public class User
    {
        [Key]
        public long Id { get; set; }
        public long ExternalUserId { get; set; }
    }
}
