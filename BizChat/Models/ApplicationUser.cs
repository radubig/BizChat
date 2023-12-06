using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace BizChat.Models
{
    public class ApplicationUser : IdentityUser
    {
        public virtual ICollection<Server>? UserServers { get; set; }
        public virtual ICollection<Message>? Messages { get; set; }
        
        public string? Name { get; set; }
        
        
    }
}
