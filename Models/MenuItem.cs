using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TarimHibe.Models
{
    [Table("MenuItems")]
    public class MenuItem
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = "";

        public string Icon { get; set; } = "";

        public bool HasSubMenu { get; set; }

        // ← Tek kolon: TopMenuId
        public int? TopMenuId { get; set; }

        [ForeignKey(nameof(TopMenuId))]
        public virtual MenuItem? Parent { get; set; }

        public string Link { get; set; } = "";

        public int Index { get; set; }

        public int EntityStatus { get; set; }

        public virtual ICollection<MenuRole> MenuRoles { get; set; }
            = new List<MenuRole>();

        public virtual ICollection<UserMenu> UserMenus { get; set; }
            = new List<UserMenu>();

        public virtual ICollection<MenuItem> Children { get; set; }
            = new List<MenuItem>();
    }
}
