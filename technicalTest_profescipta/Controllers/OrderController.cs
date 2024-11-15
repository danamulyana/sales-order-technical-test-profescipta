using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechnicalTest_Profescipta.Common.DTO;
using TechnicalTest_Profescipta.Common.Interface;

namespace technicalTest_profescipta.Controllers
{
    [Route("[controller]")]
    public class OrderController : Controller
    {
        private readonly ICustomerServices _customerServices;
        private readonly IOrderServices _orderServices;
        public OrderController(ICustomerServices customerServices, IOrderServices orderServices) {
            _customerServices = customerServices;
            _orderServices = orderServices;
        }
        public IActionResult Index()
        {
            return View();
        }
        [HttpGet("New")]
        public async Task<IActionResult> New(CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerServices.getListCustomer();

                var listCustomer = customer.Select(a => new SelectListItem
                {
                    Value = a.ComCustomerId.ToString(),
                    Text = a.CustomerName,
                }).ToList();

                ViewBag.customer = listCustomer;
                ViewBag.isNew = true;

                var order = new SalesOrder();

                return View("Manage", order);
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("edit/{id}")]
        public async Task<IActionResult> Edit(long id, CancellationToken cancellationToken)
        {
            try
            {
                var customer = await _customerServices.getListCustomer();

                var listCustomer = customer.Select(a => new SelectListItem
                {
                    Value = a.ComCustomerId.ToString(),
                    Text = a.CustomerName,
                }).ToList();

                var order = await _orderServices.getbyId(id);

                ViewBag.customer = listCustomer;
                ViewBag.isNew = false;

                return View("Manage", order);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(long id)
        {
            try
            {
                var response = await _orderServices.Delete(id);
                return Ok(new { success = response});
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("datalist")]
        public async Task<IActionResult> datalist([FromBody] DatatableRequestOrder request)
        {
            try
            {
                var response = await _orderServices.datalist(request);
                return Json(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("save")]
        public async Task<IActionResult> Save([FromBody] SalesOrder request)
        {
            try
            {
                var response = await _orderServices.SaveOrUpdate(request);
                return Json(response);
            }catch(Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("export")]
        public async Task<IActionResult> Export(CancellationToken cancellationToken)
        {
            try
            {
                var stream = await _orderServices.export(cancellationToken);

                return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Sales Order.xlsx");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
