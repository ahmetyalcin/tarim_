using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data;

namespace TarimHibe.Models
{
    [Table("MenuRoles")]
    public class MenuRole
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(RoleId))]
        public virtual Role Role { get; set; } = null!;

        [ForeignKey(nameof(MenuItemId))]
        public virtual MenuItem MenuItem { get; set; } = null!;
    }
}
