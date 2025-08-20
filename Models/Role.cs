using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimHibe.Models
{
    [Table("Roles")]
    public class Role
    {
        [Key]
        public int RoleId { get; set; }

        [Required]
        public string RoleName { get; set; } = null!;

        // Users ile ilişki
        public virtual ICollection<Users> Users { get; set; }
            = new List<Users>();

        // MenuRole tablosu ile ilişki
        public virtual ICollection<MenuRole> MenuRoles { get; set; }
            = new List<MenuRole>();
    }
}
