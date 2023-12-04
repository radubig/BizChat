using Microsoft.AspNetCore.Identity;

namespace BizChat.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Server>? UserServers { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
        // TODO: implement the rest of ApplicationUser model
        // TODO: implement ServerRole
    }
}
