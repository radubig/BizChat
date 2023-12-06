using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class ServerRole
    {
        [Key] public int Id { get; set; }
        [Required] public string? Name { get; set; }

        public bool DeleteMessagePerms { get; set; } = false;
        public bool KickUserPerms { get; set; } = false;
        public bool BanUserPerms { get; set; } = false;
    }
}
