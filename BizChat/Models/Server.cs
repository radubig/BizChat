using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class Server
    {

        [Key] public int Id { get; set; }
        [Required] public string? Name { get; set; }
        [Required] public string? Description { get; set; }
        public virtual ICollection<ServerUser>? Users { get; set; }
        public virtual ICollection<Category>? ServerCategories { get; set; }
        public virtual ICollection<Channel>? ServerChannels { get; set; }
    }
}
