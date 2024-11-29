using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class Order
    {
        [Key]
        public Guid OrderId { get; set; }

        [Required]
        public Guid? UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual User? User { get; set; }

        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Required]
        public decimal TongTien { get; set; }

        [Required]
        public int TrangThai { get; set; } // 1 = Pending, 2 = Completed, 3 = Cancelled

        // Navigation Properties
        // Mối quan hệ  1 - n với OrderDetails
        public virtual ICollection<OrderDetail> OrderDetails { get; set; }
        // Mối quan hệ  1 - 1 với PaymentHistory
        public virtual PaymentHistory? PaymentHistory { get; set; }
    }
}
