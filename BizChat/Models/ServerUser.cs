using System.ComponentModel.DataAnnotations.Schema;

namespace BizChat.Models
{
    public class ServerUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public int? ServerId { get; set; }
        public bool? IsModerator { get; set; } = false;
        public bool? IsOwner { get; set; } = false;
        public bool? IsWaiting { get; set; } = false;
        public virtual ApplicationUser? User { get; set; }
        public virtual Server? Server { get; set; }
    }
}
