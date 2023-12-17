using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BizChat.Models
{
    public class Message
    {
        [Key][DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }
        [Required] public string? Content { get; set; }
        public DateTime Date { get; set; }

        public int? ChannelId { get; set; }
        public string? UserId { get; set; }

        virtual public ApplicationUser? User { get; set; }
        virtual public Channel? Channel { get; set; }
    }
}
