using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class RoleChannelPerms
    {
        [Key] public int Id { get; set; }
        public bool ViewPerm { get; set; } = false;
        public bool SendMsgPerm { get; set; } = false;

    }
}
