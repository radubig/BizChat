using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class Channel
    {
        [Key] public int Id { get; set; }
        [Required] public string? Name { get; set; }
        [Required] public string? Description { get; set; }

        public int? ServerId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Server? Server { get; set; }
        public virtual ICollection<Message>? ServerMessages { get; set; }
    }
}
