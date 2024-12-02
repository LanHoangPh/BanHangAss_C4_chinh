using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DLL.Models;

namespace PRL_Web.Controllers
{
    public class CartDetailsController : Controller
    {
        private readonly BanHangDbContext _context;

        public CartDetailsController(BanHangDbContext context)
        {
            _context = context;
        }
        // Xử lý Dữ kiện khi ấn mua hàng
        [HttpPost]
        public async Task<IActionResult> BuyToCart(Guid cartDetailId)
        {
            var userName = HttpContext.Session.GetString("UserName");

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["Error"] = "Không tìm thấy tài khoản của bạn.";
                return RedirectToAction("Login", "Users");
            }

            var cartDetail = await _context.CartDetails
                .Include(cd => cd.Product)
                .FirstOrDefaultAsync(cd => cd.CartDetailId == cartDetailId);

            if (cartDetail == null || cartDetail.Product == null)
            {
                TempData["Error"] = "Không tìm thấy sản phẩm trong giỏ hàng!";
                return RedirectToAction("IndexCart");
            }

            if (cartDetail.Product.SoLuong < cartDetail.SoLuong)
            {
                TempData["Error"] = $"Sản phẩm '{cartDetail.Product.TenSp}' không đủ số lượng trong kho!";
                return RedirectToAction("IndexCart");
            }

            var existingOrder = await _context.Orders
                .Include(o => o.OrderDetails)
                .FirstOrDefaultAsync(o => o.UserId == user.UserId && o.TrangThai == 1);

            if (existingOrder == null)
            {
                // Nếu chưa có Order, tạo Order mới
                existingOrder = new Order
                {
                    OrderId = Guid.NewGuid(),
                    UserId = user.UserId,
                    OrderDate = DateTime.Now,
                    TrangThai = 1, // Pending
                    TongTien = 0
                };
                _context.Orders.Add(existingOrder);
                await _context.SaveChangesAsync();
            }

            var existingOrderDetail = existingOrder.OrderDetails
                .FirstOrDefault(od => od.ProductId == cartDetail.ProductId);

            if (existingOrderDetail == null)
            {
                var newOrderDetail = new OrderDetail
                {
                    OrderDetailId = Guid.NewGuid(),
                    OrderId = existingOrder.OrderId,
                    ProductId = cartDetail.ProductId,
                    SoLuong = cartDetail.SoLuong,
                    GiaSanPham = cartDetail.Product.Gia
                };
                _context.OrderDetails.Add(newOrderDetail);
            }
            else
            {
                existingOrderDetail.SoLuong += cartDetail.SoLuong;
                _context.OrderDetails.Update(existingOrderDetail);
            }

            // Cập nhật tồn kho sản phẩm
            cartDetail.Product.SoLuong -= cartDetail.SoLuong;
            _context.Products.Update(cartDetail.Product);

            // Cập nhật tổng tiền của Order
            existingOrder.TongTien += cartDetail.SoLuong * cartDetail.Product.Gia;
            _context.Orders.Update(existingOrder);

            // Xóa sản phẩm khỏi giỏ hàng
            _context.CartDetails.Remove(cartDetail);

            await _context.SaveChangesAsync();
            await UpdateCartBadge();

            TempData["Success"] = "Mua sản phẩm thành công!";
            return RedirectToAction("IndexCart");
        }



        public async Task<IActionResult> IndexCart()
        {
            var userName = HttpContext.Session.GetString("UserName");
            if (string.IsNullOrEmpty(userName))
            {
                TempData["ErroPro"] = "Vui lòng đăng nhập để xem giỏ hàng.";
                return RedirectToAction("Login", "Users");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["ErroPro"] = "Không tìm thấy thông tin người dùng.";
                return RedirectToAction("DangKy", "Users");
            }

            var cart = await _context.Carts
                .Include(c => c.CartDetails)
                    .ThenInclude(cd => cd.Product) 
                .FirstOrDefaultAsync(c => c.UserId == user.UserId);

            if (cart == null || cart.CartDetails.Count == 0)
            {
                TempData["Messeger"] = "Giỏ hàng của bạn đang trống thêm sản phẩm đi !";
                return RedirectToAction("IndexCus", "Products"); 
            }

            return View(cart.CartDetails);
        }
        // cập nhật số lượng trong giỏ hàng
        [HttpPost]
        public async Task<IActionResult> UpdateQuantity(Guid cartDetailId, string action)
        {
            var cartDetail = await _context.CartDetails
                .Include(cd => cd.Product)
                .FirstOrDefaultAsync(cd => cd.CartDetailId == cartDetailId);

            if (cartDetail == null)
            {
                TempData["Messeger"] = "Không tìm thấy sản phẩm trong giỏ hàng .";
                return RedirectToAction("IndexCus", "Products");
            }

            if (action == "tang")
            {
                if (cartDetail.SoLuong + 1 > cartDetail.Product.SoLuong)
                {
                    TempData["Messeger"] = "Không đủ số lượng trong kho.";
                }
                else
                {
                    cartDetail.SoLuong += 1;
                    _context.CartDetails.Update(cartDetail);
                    TempData["Messeger"] = "Tăng số lượng sản phẩm thành công.";
                }
            }
            else if (action == "giam")
            {
                if (cartDetail.SoLuong > 1)
                {
                    cartDetail.SoLuong -= 1;
                    _context.CartDetails.Update(cartDetail);
                    TempData["Messeger"] = "Giảm số lượng sản phẩm thành công.";
                }
                else
                {
                    TempData["Messeger"] = "Số lượng tối thiểu là 1.";
                }
            }

            await _context.SaveChangesAsync();
            await UpdateCartBadge();
            return RedirectToAction("IndexCart");
        }
        // Xóa sản phẩm khỏi giỏ hàng
        [HttpPost]
        public async Task<IActionResult> DeleteItem(Guid cartDetailId)
        {
            var cartDetail = await _context.CartDetails
                .FirstOrDefaultAsync(cd => cd.CartDetailId == cartDetailId);

            if (cartDetail == null)
            {
                TempData["Messeger"] = "Không tìm thấy sản phẩm trong giỏ hàng.";
                return RedirectToAction("IndexCart");
            }

            _context.CartDetails.Remove(cartDetail);
            await _context.SaveChangesAsync();
            await UpdateCartBadge();

            TempData["Messeger"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("IndexCart");

        }

        public async Task<IActionResult> UpdateCartBadge()
        {
            // Lấy thông tin người dùng hiện tại từ Session
            var userName = HttpContext.Session.GetString("UserName");
            if (userName == null)
            {
                TempData["CartItemCount"] = 0; // Nếu người dùng chưa đăng nhập
                return RedirectToAction("IndexCus", "Products");
            }

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == userName);
            if (user == null)
            {
                TempData["CartItemCount"] = 0; 
                return RedirectToAction("IndexCus", "Products");
            }

            // Lấy giỏ hàng của người dùng
            var cart = await _context.Carts
                .FirstOrDefaultAsync(c => c.UserId == user.UserId);

            if (cart == null)
            {
                TempData["CartItemCount"] = 0; // Nếu giỏ hàng rỗng
            }
            else
            {
                // Đếm số lượng sản phẩm trong CartDetails
                int itemCount = await _context.CartDetails
                    .Where(cd => cd.CartId == cart.CartId)
                    .SumAsync(cd => cd.SoLuong);

                TempData["CartItemCount"] = itemCount;
            }

            return RedirectToAction("IndexCus", "Products");
        }

        public async Task<IActionResult> Index()
        {
            var banHangDbContext = _context.CartDetails.Include(c => c.Cart).Include(c => c.Product);
            return View(await banHangDbContext.ToListAsync());
        }

        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartDetailId == id);
            if (cartDetail == null)
            {
                return NotFound();
            }

            return View(cartDetail);
        }

        public IActionResult Create()
        {
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "UserName");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("CartDetailId,CartId,ProductId,SoLuong")] CartDetail cartDetail)
        {
            if (ModelState.IsValid)
            {
                cartDetail.CartDetailId = Guid.NewGuid();
                _context.Add(cartDetail);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "UserName", cartDetail.CartId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl", cartDetail.ProductId);
            return View(cartDetail);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails.FindAsync(id);
            if (cartDetail == null)
            {
                return NotFound();
            }
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "UserName", cartDetail.CartId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl", cartDetail.ProductId);
            return View(cartDetail);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("CartDetailId,CartId,ProductId,SoLuong")] CartDetail cartDetail)
        {
            if (id != cartDetail.CartDetailId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(cartDetail);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CartDetailExists(cartDetail.CartDetailId))
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
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "UserName", cartDetail.CartId);
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl", cartDetail.ProductId);
            return View(cartDetail);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cartDetail = await _context.CartDetails
                .Include(c => c.Cart)
                .Include(c => c.Product)
                .FirstOrDefaultAsync(m => m.CartDetailId == id);
            if (cartDetail == null)
            {
                return NotFound();
            }

            return View(cartDetail);
        }

        // POST: CartDetails/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var cartDetail = await _context.CartDetails.FindAsync(id);
            if (cartDetail != null)
            {
                _context.CartDetails.Remove(cartDetail);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CartDetailExists(Guid id)
        {
            return _context.CartDetails.Any(e => e.CartDetailId == id);
        }
    }
}
