using DLL.Models;

namespace PRL_Web.ViewModel
{
    public class CheckoutViewModel
    {
        public Order Order { get; set; }
        public IEnumerable<PaymentMethod> PaymentMethods { get; set; }
        public decimal SoTienThanhToan { get; set; }
    }
}
