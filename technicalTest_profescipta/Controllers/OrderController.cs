using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using TechnicalTest_Profescipta.Common.DTO;
using TechnicalTest_Profescipta.Common.Interface;

namespace technicalTest_profescipta.Controllers
{
    public class OrderController : Controller
    {
        private readonly ICustomerServices _customerServices;
        public OrderController(ICustomerServices customerServices) {
            _customerServices = customerServices;
        }
        public IActionResult Index()
        {
            return View();
        }

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

                return View("Manage");
            }
            catch (Exception ex) { 
                return BadRequest(ex.Message);
            }
        }

    }
}
