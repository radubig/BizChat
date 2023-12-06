using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class Category
    {
        [Key] public int Id { get; set; }
        [Required] public string? CategoryName { get; set; }

        public int? ServerId { get; set; }
        public virtual Server? Server { get; set; }
        public virtual ICollection<Channel>? Channels { get; set; }
    }
}
