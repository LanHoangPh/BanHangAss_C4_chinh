using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class Cart
    {
        [Key]
        public Guid CartId { get; set; }

        [Required]
        public Guid? UserId { get; set; }

        public string UserName { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime NgayTao { get; set; } = DateTime.Now;

        // Mối quan hệ  1 - n với CartDetails
        public virtual ICollection<CartDetail> CartDetails { get; set; }
    }
}
