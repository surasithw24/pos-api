using Microsoft.AspNetCore.Mvc;
using Supabase;
using Postgrest.Attributes;
using Postgrest.Models;

namespace POSApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CustomersController : ControllerBase
    {
        private readonly Client _supabase;

        public CustomersController(Client supabase)
        {
            _supabase = supabase;
        }

        // GET: api/customers
        [HttpGet]
        public async Task<IActionResult> GetCustomers()
        {
            try
            {
                var response = await _supabase.From<CustomerModel>().Get();

                var data = response.Models.Select(x => new
                {
                    x.CustomersId,
                    x.CustomersName,
                    x.CustomersStatus
                });

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // POST: api/customers
        [HttpPost]
        public async Task<IActionResult> AddCustomer([FromBody] CustomerModel newCustomer)
        {
            try
            {
                if (string.IsNullOrEmpty(newCustomer.CustomersStatus))
                {
                    newCustomer.CustomersStatus = "ว่าง";
                }

                await _supabase.From<CustomerModel>().Insert(newCustomer);
                return Ok(new { message = "เพิ่มข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // PUT: api/customers/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] CustomerModel updatedCustomer)
        {
            try
            {
                // ดึงข้อมูลเดิมออกมาก่อน เพื่อป้องกันไม่ให้ CustomersStatus กลายเป็น null
                var existing = await _supabase.From<CustomerModel>()
                    .Where(x => x.CustomersId == id)
                    .Single();

                if (existing == null)
                {
                    return NotFound(new { error = "ไม่พบข้อมูลโต๊ะที่ต้องการแก้ไข" });
                }

                existing.CustomersName = updatedCustomer.CustomersName;

                // ถ้ามีการส่ง status ใหม่มาค่อยเปลี่ยน ถ้าไม่มีให้ใช้ค่าเดิม
                if (!string.IsNullOrEmpty(updatedCustomer.CustomersStatus))
                {
                    existing.CustomersStatus = updatedCustomer.CustomersStatus;
                }

                await existing.Update<CustomerModel>();

                return Ok(new { message = "แก้ไขข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }

        // DELETE: api/customers/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            try
            {
                await _supabase.From<CustomerModel>()
                    .Where(x => x.CustomersId == id)
                    .Delete();
                return Ok(new { message = "ลบข้อมูลสำเร็จ" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = ex.Message });
            }
        }
    }

    [Table("tblCustomers")]
    public class CustomerModel : BaseModel
    {
        [PrimaryKey("customersId", false)]
        public int CustomersId { get; set; }

        [Column("customersName")]
        public string CustomersName { get; set; } = string.Empty;

        [Column("customersStatus")]
        public string? CustomersStatus { get; set; }
    }
}