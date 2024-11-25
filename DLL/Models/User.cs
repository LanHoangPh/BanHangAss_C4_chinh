using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DLL.Models
{
    public class User
    {
        [Key]
        public Guid UserId { get; set; }

        [Required(ErrorMessage = "Ten không đc để trống")]
        [MaxLength(50)]
        public string Username { get; set; } // Tên người dùng là duy nhất

        [Required]
        public string Password { get; set; }

        [MaxLength(100)]
        public string FullName { get; set; }

        [MaxLength(100), EmailAddress]
        public string Email { get; set; }

        [MaxLength(15)]
        public string Phone { get; set; }

        public int Role { get; set; } 

        public DateTime ThoiGianTao { get; set; } = DateTime.Now;

        // Navigation Properties
        // Mối quan hệ  1 - 1 với Customer
        public Customer? Customer { get; set; }
        // Mối quan hệ  1 - 1 với Cart
        public Cart? Cart { get; set; }
        // Mối quan hệ  1 - n với Order
        public ICollection<Order>? Order { get; set; }
    }
}
