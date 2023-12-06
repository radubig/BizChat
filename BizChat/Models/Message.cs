using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class Message
    {
        [Key] public int Id { get; set; }
        [Required] public string? Content { get; set; }
        public DateTime Date { get; set; }

        public int? ChannelId { get; set; }
        // Nu avem nevoie de server id, cred

        virtual public ApplicationUser? User { get; set; }
        virtual public Channel? Channel { get; set; }
    }
}
