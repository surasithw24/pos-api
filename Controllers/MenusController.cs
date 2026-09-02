using Microsoft.AspNetCore.Mvc;
using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;

namespace POSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MenusController : ControllerBase
    {
        private readonly Client _supabase;

        public MenusController(Client supabase)
        {
            _supabase = supabase;
        }

        // GET: api/menus
        [HttpGet]
        public async Task<IActionResult> GetMenus()
        {
            try
            {
                // 🟢 เพิ่ม Order สั่งเรียงตาม ProductId จากน้อยไปมาก
                var response = await _supabase.From<ProductModel>()
                    .Order(x => x.ProductId, Postgrest.Constants.Ordering.Ascending)
                    .Get();

                var products = response.Models ?? new List<ProductModel>();

                var categoriesDict = new Dictionary<int, string>();
                try
                {
                    var catResponse = await _supabase.From<FoodCategoryModel>().Get();
                    if (catResponse.Models != null)
                    {
                        categoriesDict = catResponse.Models
                            .GroupBy(c => c.CategoryID)
                            .ToDictionary(g => g.Key, g => g.First().CategoryName);
                    }
                }
                catch (Exception catEx)
                {
                    Console.WriteLine($"[Warning] Failed to fetch categories: {catEx.Message}");
                }

                var data = products.Select(x => new
                {
                    menuId = x.ProductId,
                    menuName = x.ProductName,
                    price = x.CurrentPrice,
                    imageUrl = x.ProductImage,
                    categoryID = x.CategoryID,
                    categoryName = (x.CategoryID.HasValue && categoriesDict.TryGetValue(x.CategoryID.Value, out var catName))
                                    ? catName
                                    : "ไม่มีหมวดหมู่"
                });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // GET: api/menus/categories
        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var response = await _supabase.From<FoodCategoryModel>().Get();
                var data = (response.Models ?? new List<FoodCategoryModel>()).Select(x => new
                {
                    categoryId = x.CategoryID,
                    categoryName = x.CategoryName
                });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message, details = ex.StackTrace });
            }
        }

        // POST: api/menus/upload (อัปโหลดรูปภาพขึ้น Supabase Storage)
        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "กรุณาเลือกไฟล์รูปภาพ" });

            try
            {
                var fileExt = Path.GetExtension(file.FileName);
                var fileName = $"{Guid.NewGuid()}{fileExt}";

                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                var bytes = memoryStream.ToArray();

                // 🟢 เปลี่ยนชื่อเป็น "product-images" ให้ตรงกับ Supabase
                await _supabase.Storage.From("product-images").Upload(bytes, fileName);

                // 🟢 เปลี่ยนชื่อตรงนี้ด้วยครับ
                var publicUrl = _supabase.Storage.From("product-images").GetPublicUrl(fileName);

                return Ok(new { url = publicUrl });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.InnerException?.Message ?? ex.Message });
            }
        }

        // POST: api/menus
        [HttpPost]
        public async Task<IActionResult> AddMenu([FromBody] ProductModel newProduct)
        {
            try
            {
                await _supabase.From<ProductModel>().Insert(newProduct);
                return Ok(new { message = "เพิ่มเมนูสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/menus/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateMenu(int id, [FromBody] ProductModel updatedProduct)
        {
            try
            {
                var response = await _supabase.From<ProductModel>()
                    .Where(x => x.ProductId == id)
                    .Single();

                if (response == null)
                    return NotFound(new { error = "ไม่พบรายการเมนู" });

                response.ProductName = updatedProduct.ProductName;
                response.CurrentPrice = updatedProduct.CurrentPrice;
                response.ProductImage = updatedProduct.ProductImage;
                response.CategoryID = updatedProduct.CategoryID;

                await response.Update<ProductModel>();
                return Ok(new { message = "แก้ไขเมนูสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE: api/menus/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMenu(int id)
        {
            try
            {
                await _supabase.From<ProductModel>()
                    .Where(x => x.ProductId == id)
                    .Delete();

                return Ok(new { message = "ลบเมนูสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [Table("tblProducts")]
    public class ProductModel : BaseModel
    {
        [PrimaryKey("productId", false)]
        public int ProductId { get; set; }

        [Column("productName")]
        public string ProductName { get; set; } = string.Empty;

        [Column("currentPrice")]
        public decimal CurrentPrice { get; set; }

        [Column("productImage")]
        public string? ProductImage { get; set; }

        [Column("categoryID")]
        public int? CategoryID { get; set; }
    }

    // 🟢 แก้ไขชื่อตารางจาก tblFoodCateqory เป็น tblFoodCategory
    [Table("tblFoodCategory")]
    public class FoodCategoryModel : BaseModel
    {
        [PrimaryKey("categoryID", false)]
        public int CategoryID { get; set; }

        [Column("categoryName")]
        public string CategoryName { get; set; } = string.Empty;
    }
}