using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using DuAnThuongMaiDienTu.Models;
using DuAnThuongMaiDienTu.Repositories;

namespace DuAnThuongMaiDienTu.Controllers
{
    public class ProductController : Controller
    {
        private readonly IProductRepository _productRepository;
        private readonly ICategoryRepository _categoryRepository;

        public ProductController(IProductRepository productRepository, ICategoryRepository categoryRepository)
        {
            _productRepository = productRepository;
            _categoryRepository = categoryRepository;
        }

        // Hiển thị danh sách sản phẩm
        public async Task<IActionResult> Index()
        {
            var products = await _productRepository.GetAllAsync();
            return View(products);
        }

        // Hiển thị chi tiết sản phẩm
        public async Task<IActionResult> Display(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Hiển thị form thêm sản phẩm
        public async Task<IActionResult> Add()
        {
            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");
            return View();
        }

        // Xử lý thêm sản phẩm
        [HttpPost]
        public async Task<IActionResult> Add(Product product, IFormFile imageUrl)
        {
            // Bỏ qua validation cho ImageUrl
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        product.ImageUrl = await SaveImage(imageUrl);
                    }
                    await _productRepository.AddAsync(product);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi thêm sản phẩm: " + ex.Message);
                }
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Hiển thị form sửa sản phẩm
        public async Task<IActionResult> Update(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Xử lý sửa sản phẩm
        [HttpPost]
        public async Task<IActionResult> Update(int id, Product product, IFormFile imageUrl)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Bỏ qua validation cho ImageUrl
            ModelState.Remove("ImageUrl");

            if (ModelState.IsValid)
            {
                try
                {
                    var existingProduct = await _productRepository.GetByIdAsync(id);

                    if (imageUrl != null && imageUrl.Length > 0)
                    {
                        product.ImageUrl = await SaveImage(imageUrl);
                    }
                    else
                    {
                        product.ImageUrl = existingProduct.ImageUrl;
                    }

                    existingProduct.Name = product.Name;
                    existingProduct.Price = product.Price;
                    existingProduct.Description = product.Description;
                    existingProduct.CategoryId = product.CategoryId;
                    existingProduct.ImageUrl = product.ImageUrl;

                    await _productRepository.UpdateAsync(existingProduct);
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi khi cập nhật: " + ex.Message);
                }
            }

            var categories = await _categoryRepository.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", product.CategoryId);
            return View(product);
        }

        // Hiển thị form xóa sản phẩm
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _productRepository.GetByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // Xử lý xóa sản phẩm
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _productRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "Lỗi khi xóa: " + ex.Message);
                return RedirectToAction(nameof(Index));
            }
        }

        // Hàm lưu ảnh
        private async Task<string> SaveImage(IFormFile image)
        {
            try
            {
                var imagesPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");

                if (!Directory.Exists(imagesPath))
                {
                    Directory.CreateDirectory(imagesPath);
                }

                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(image.FileName);
                var savePath = Path.Combine(imagesPath, fileName);

                using (var fileStream = new FileStream(savePath, FileMode.Create))
                {
                    await image.CopyToAsync(fileStream);
                }

                return "/images/" + fileName;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi lưu ảnh: {ex.Message}");
                return null;
            }
        }
    }
}
