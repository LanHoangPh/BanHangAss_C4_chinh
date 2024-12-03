using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DLL.Models;
using PRL_Web.ViewModel;

namespace PRL_Web.Controllers
{
    public class OrderDetailsController : Controller
    {
        private readonly BanHangDbContext _context;

        public OrderDetailsController(BanHangDbContext context)
        {
            _context = context;
        }

        [HttpPost]
        public async Task<IActionResult> Checkout(Guid orderId, Guid paymentMethodId, decimal soTienThanhToan)
        {
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.TrangThai != 1)
            {
                TempData["Error"] = "Hóa đơn không hợp lệ!";
                return RedirectToAction("IndexOrder");
            }

            if (soTienThanhToan < order.TongTien)
            {
                TempData["Error"] = "Số tiền thanh toán không đủ!";
                return RedirectToAction("GoToCheckout", new { orderId });
            }

            var paymentHistory = new PaymentHistory
            {
                PaymentId = Guid.NewGuid(),
                OrderId = order.OrderId,
                PaymentMethodId = paymentMethodId,
                TongTien = soTienThanhToan,
                ThoiGianTT = DateTime.Now,
                Status = 1,
                GhiChu = $"Số tiền khách đưa {soTienThanhToan}$ - Số Tiền Phải Thối {soTienThanhToan - order.TongTien}$"
            };

            order.TrangThai = 2; 
            _context.PaymentHistories.Add(paymentHistory);
            _context.Orders.Update(order);

            await _context.SaveChangesAsync();

            TempData["Success"] = "Thanh toán thành công!";
            return RedirectToAction("IndexOrder");
        }

        [HttpPost]
        public async Task<IActionResult> Refund(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.TrangThai != 1)
            {
                TempData["Error"] = "Không thể hoàn trả hóa đơn này.";
                return RedirectToAction("IndexOrder");
            }

            foreach (var detail in order.OrderDetails)
            {
                var product = await _context.Products.FindAsync(detail.ProductId);
                if (product != null)
                {
                    product.SoLuong += detail.SoLuong;
                    _context.Products.Update(product);
                }
            }

            _context.OrderDetails.RemoveRange(order.OrderDetails);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Hóa đơn đã được hoàn trả thành công!";
            return RedirectToAction("IndexOrder");
        }

        [HttpPost]
        public async Task<IActionResult> GoToCheckout(Guid orderId)
        {
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.OrderId == orderId);

            if (order == null || order.TrangThai != 1)
            {
                TempData["Error"] = "Hóa đơn không hợp lệ!";
                return RedirectToAction("IndexOrder");
            }

            var paymentMethods = await _context.PaymentMethods.ToListAsync();

            var viewModel = new CheckoutViewModel
            {
                Order = order,
                PaymentMethods = paymentMethods
            };

            return View(viewModel);
        }

        public async Task<IActionResult> IndexOrder()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null)
            {
                TempData["Error"] = "Bạn cần đăng nhập để xem hóa đơn!";
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản của bạn!";
                return RedirectToAction("Login", "Users");
            }

            var pendingOrders = await _context.Orders
                .Include(o => o.OrderDetails) 
                    .ThenInclude(od => od.Product) 
                .Where(o => o.UserId == user.UserId && o.TrangThai == 1)
                .ToListAsync();

            return View(pendingOrders);
        }

        public async Task<IActionResult> IndexOrder2()
        {
            var userName = HttpContext.Session.GetString("UserName");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy người dùng. Vui lòng thử lại.";
                return RedirectToAction("Login", "Users");
            }

            var processedOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Include(o => o.PaymentHistory)
                .ThenInclude(o => o.PaymentMethod)
                .Where(o => o.UserId == user.UserId && o.TrangThai == 2)
                .OrderByDescending(ph => ph.OrderDate)
                .ToListAsync();

            return View(processedOrders);
        }

        public async Task<IActionResult> Index()
        {
            var banHangDbContext = _context.OrderDetails.Include(o => o.Order).Include(o => o.Product);
            return View(await banHangDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail == null)
            {
                return NotFound();
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl", orderDetail.ProductId);
            return View(orderDetail);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, OrderDetail orderDetail)
        {
            if (id != orderDetail.OrderDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(orderDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!OrderDetailExists(orderDetail.OrderDetailId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["OrderId"] = new SelectList(_context.Orders, "OrderId", "OrderId", orderDetail.OrderId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl", orderDetail.ProductId);
            return View(orderDetail);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var orderDetail = await _context.OrderDetails
                .Include(o => o.Order)
                .Include(o => o.Product)
                .FirstOrDefaultAsync(m => m.OrderDetailId == id);
            if (orderDetail == null)
            {
                return NotFound();
            }

            return View(orderDetail);
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var orderDetail = await _context.OrderDetails.FindAsync(id);
            if (orderDetail != null)
            {
                _context.OrderDetails.Remove(orderDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool OrderDetailExists(Guid id)
        {
            return _context.OrderDetails.Any(e => e.OrderDetailId == id);
        }
    }
}
