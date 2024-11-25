using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class PaymentHistory
    {
        [Key]
        public Guid PaymentId { get; set; }

        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]

        public Order? Order { get; set; }

        public Guid? PaymentMethodId { get; set; }
        [ForeignKey("PaymentMethodId")]
        public PaymentMethod? PaymentMethod { get; set; }

        public decimal TongTien { get; set; }

        public DateTime ThoiGianTT { get; set; } = DateTime.Now;

        public int Status { get; set; } // 1 = Success, 2 = Failed, 3 = Pending
    }
}
