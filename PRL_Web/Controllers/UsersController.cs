using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DLL.Models;
using System.Collections;
using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Common;
using Microsoft.CodeAnalysis.Scripting;

namespace PRL_Web.Controllers
{
    public class UsersController : Controller
    {
        private readonly BanHangDbContext _context;

        public UsersController(BanHangDbContext context)
        {
            _context = context;
        }


        public IActionResult ForgotPassword() => View();

        [HttpPost]
        public IActionResult ForgotPassword(string email)
        {
            var user = _context.Users.FirstOrDefault(x => x.Email == email);
            if (user == null)
            {
                ViewData["erroemail"] = "Không tìm thấy email này.";
                return View();
            }
            else
            {
                var token = Guid.NewGuid().ToString();
                user.ResetToken = token;
                user.ResetTokenExpiry = DateTime.Now.AddHours(1); 
                _context.SaveChanges();

                ViewData["successMessage"] = "Email đặt lại mật khẩu đã được gửi!";
                return RedirectToAction(nameof(ResetPassword));
            }
            return View();
        }

        public IActionResult ResetPassword(string token)
        {
            var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewData["error"] = "Liên kết không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            ViewBag.Token = token;
            return View();
        }

        [HttpPost]
        public IActionResult ResetPasswordConfirmation(string token, string newPassword, string confirmPassword)
        {
            if (newPassword != confirmPassword)
            {
                ViewData["error"] = "Mật khẩu xác nhận không khớp.";
                return RedirectToAction(nameof(ResetPassword), new { token });
            }

            var user = _context.Users.FirstOrDefault(u => u.ResetToken == token && u.ResetTokenExpiry > DateTime.Now);
            if (user == null)
            {
                ViewData["error"] = "Liên kết không hợp lệ hoặc đã hết hạn.";
                return RedirectToAction(nameof(ForgotPassword));
            }

            user.Password =newPassword; 
            user.ResetToken = null; 
            user.ResetTokenExpiry = null;
            _context.SaveChanges();

            ViewData["successMessage"] = "Mật khẩu đã được thay đổi thành công!";
            return RedirectToAction("Login"); 
        }

        public IActionResult LogOut()
        {
            return RedirectToAction("Login");
        }

        public IActionResult DangKy() => View();

        [HttpPost]
        public IActionResult DangKy(User user)
        {
            try
            {
                user.UserId = Guid.NewGuid(); 
                user.Role = 2;
                user.ThoiGianTao = DateTime.Now;
                _context.Users.Add(user);
                Cart cart = new Cart()
                {
                    UserId = user.UserId,
                    UserName = user.Username,
                    NgayTao = DateTime.Now,
                };
                _context.Carts.Add(cart); 
                _context.SaveChanges();

                return RedirectToAction(nameof(Login));
            }
            catch (Exception ex)
            {
                return BadRequest($"Lỗi xảy ra khi lưu dữ liệu: {ex.Message}");
            }
        }

        public IActionResult Login() => View();
       
        [HttpPost]
        public IActionResult Login(string username, string password)
        {
            var user = _context.Users.FirstOrDefault(x => x.Username == username && x.Password == password);
            if (user == null)
            {
                ViewData["ErroLogin"] = "Không tìm thấy tài khoản này";
            }
            else
            {
                HttpContext.Session.SetString("UserName", user.Username);
                HttpContext.Session.SetInt32("UserRole", user.Role);

                TempData["UserName"] = user.Username;
                if(user.Role == 2)
                {
                    return RedirectToAction("IndexCus", "Products");
                }
                else 
                {
                    return RedirectToAction("Index");
                }    
                
            }

            return View();
        }

        public IActionResult Index()
        {
            return View( _context.Users.ToList());
        }

        // GET: Users/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        // GET: Users/Create
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("UserId,Username,Password,FullName,Email,Phone,Role")] User user)
        {
            if (ModelState.IsValid)
            {
                user.UserId = Guid.NewGuid();
                user.ThoiGianTao = DateTime.Now;
                _context.Add(user);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(user);
        }

        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }
            return View(user);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("UserId,Username,Password,FullName,Email,Phone,Role,ThoiGianTao")] User user)
        {
            if (id != user.UserId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    user.ThoiGianTao = DateTime.Now;
                    _context.Update(user);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExists(user.UserId))
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
            return View(user);
        }

        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var user = await _context.Users
                .FirstOrDefaultAsync(m => m.UserId == id);
            if (user == null)
            {
                return NotFound();
            }

            return View(user);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExists(Guid id)
        {
            return _context.Users.Any(e => e.UserId == id);
        }
    }
}
