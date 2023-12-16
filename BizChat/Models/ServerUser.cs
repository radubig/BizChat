using System.ComponentModel.DataAnnotations.Schema;

namespace BizChat.Models
{
    public class ServerUser
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string? UserId { get; set; }
        public int? ServerId { get; set; }
        public bool? IsModerator { get; set; }
        public bool? IsOwner { get; set; }
        public virtual ApplicationUser? User { get; set; }
        public virtual Server? Server { get; set; }
    }
}
