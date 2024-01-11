using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class Channel
    {
        [Key] public int Id { get; set; }
        [Required(ErrorMessage = "The channel name cannot be empty")] public string? Name { get; set; }
        [Required(ErrorMessage = "The channel description cannot be empty")] public string? Description { get; set; }

        public int? ServerId { get; set; }
        public int? CategoryId { get; set; }

        public virtual Category? Category { get; set; }
        public virtual Server? Server { get; set; }
        public virtual ICollection<Message>? ServerMessages { get; set; }
    }
}
