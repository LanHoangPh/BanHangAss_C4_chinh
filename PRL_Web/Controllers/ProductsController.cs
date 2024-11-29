﻿using DLL.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using X.PagedList.Extensions;

namespace PRL_Web.Controllers
{
    public class ProductsController : Controller
    {
        private readonly BanHangDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(BanHangDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        public IActionResult IndexCus(int? page)
        {
            int pageSize = 6;
            int pageNumber = 0;

            if (page == null) pageNumber = 1;
            else pageNumber = (int)page;
            var products = _context.Products
                .Where(x => x.TrangThai == 1)
                .OrderBy(x => x.ProductId) 
                .ToPagedList(pageNumber, pageSize);
            return View(products);

        }

        public IActionResult Index(int? page)
        {
            int pageSize = 7;
            int pageNumber = 0;

            if (page == null) pageNumber = 1;
            else pageNumber = (int)page;

            var product = _context.Products.OrderBy(x => x.ProductId).ToPagedList(pageNumber, pageSize);

            return View(product);
        }

        public IActionResult Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham = _context.Products
                .FirstOrDefault(m => m.ProductId == id);
            if (sanPham == null)
            {
                return NotFound();
            }

            return View(sanPham);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

        public IActionResult Create(Product product, IFormFile uploadedImage)
        {
            if (!ModelState.IsValid)
            {
                product.ProductId = Guid.NewGuid();

                if (uploadedImage != null && uploadedImage.Length > 0)
                {
                    var filePath = Path.Combine(_environment.WebRootPath, "images", uploadedImage.FileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        uploadedImage.CopyTo(stream);
                    }

                    product.ImageUrl = "/images/" + uploadedImage.FileName;
                }

                _context.Add(product);
                _context.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View();
        }

        public IActionResult Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var sanPham =_context.Products.Find(id);
            if (sanPham == null)
            {
                return NotFound();
            }
            return View(sanPham);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(Guid id, Product product, IFormFile uploadedImage)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                try
                {
                    if (uploadedImage != null && uploadedImage.Length > 0)
                    {
                        var filePath = Path.Combine(_environment.WebRootPath, "images", uploadedImage.FileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            uploadedImage.CopyTo(stream);
                        }
                        product.ImageUrl = "/images/" + uploadedImage.FileName;
                    }

                    _context.Update(product);
                    _context.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!SanPhamExists(product.ProductId))
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
            return View(product);
        }

        private bool SanPhamExists(Guid id)
        {
            return _context.Products.Any(e => e.ProductId == id);
        }
    }
}
