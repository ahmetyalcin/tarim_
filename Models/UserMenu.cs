using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimHibe.Models
{
    [Table("UserMenus")]
    public class UserMenu
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        public int MenuItemId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual Users User { get; set; } = null!;

        [ForeignKey(nameof(MenuItemId))]
        public virtual MenuItem MenuItem { get; set; } = null!;
    }
}
