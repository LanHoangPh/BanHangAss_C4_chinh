using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class Product
    {
        [Key]
        public Guid ProductId { get; set; }

        [Required, MaxLength(100)]
        public string TenSp { get; set; }

        public string MoTa { get; set; }

        [Required]
        public decimal Gia { get; set; }

        [Required]
        public int SoLuong { get; set; }

        public string ImageUrl { get; set; }

        public int TrangThai { get; set; }

        // Navigation Properties
        // Mối quan hệ  1 - n với CartDetails
        public virtual ICollection<CartDetail> CartDetails { get; set; }
        // Mối quan hệ  1 - n với OrderDetails
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
    }
}
