using aspnetserver.Auth.Model;
using System.ComponentModel.DataAnnotations;

namespace aspnetserver.Data.Models
{
    public class Item : IUserOwnedResource
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ZoneId { get; set; }
        public Zone Zone { get; set; }
        [Required]
        public string UserId { get; set; }
        public WarehouseRestUser User { get; set; }
    }
}
