using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class CartDetail
    {
        [Key]
        public Guid CartDetailId { get; set; }

        [Required]
        public Guid? CartId { get; set; }
        [ForeignKey("CartId")]
        public virtual Cart? Cart { get; set; }

        [Required]
        public Guid? ProductId { get; set; }
        [ForeignKey("ProductId")]
        public virtual Product? Product { get; set; }

        [Required]
        public int SoLuong { get; set; }
    }
}
