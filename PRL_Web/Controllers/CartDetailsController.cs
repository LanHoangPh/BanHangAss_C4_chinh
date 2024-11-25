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

        // GET: CartDetails
        public async Task<IActionResult> Index()
        {
            var banHangDbContext = _context.CartDetails.Include(c => c.Cart).Include(c => c.Product);
            return View(await banHangDbContext.ToListAsync());
        }

        // GET: CartDetails/Details/5
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

        // GET: CartDetails/Create
        public IActionResult Create()
        {
            ViewData["CartId"] = new SelectList(_context.Carts, "CartId", "UserName");
            ViewData["ProductId"] = new SelectList(_context.Products, "ProductId", "ImageUrl");
            return View();
        }

        // POST: CartDetails/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: CartDetails/Edit/5
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

        // POST: CartDetails/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
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

        // GET: CartDetails/Delete/5
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
