using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechnicalTest_Profescipta.Common.Entity;
using TechnicalTest_Profescipta.Common.Interface;
using TechnicalTest_Profescipta.DAL.Context;

namespace TechnicalTest_Profescipta.Services
{
    public class CustomerServices : ICustomerServices
    {
        public CustomerServices() { }

        public async Task<List<ComCustomer>> getListCustomer(CancellationToken cancellationToken)
        {
            DBContext dbContext = new DBContext();

            try
            {
                return await dbContext.ComCustomers.ToListAsync(cancellationToken);
            }
            catch (Exception ex)
            {
                throw ex;
            }
            finally { 
                await dbContext.DisposeAsync();
            }
        }
    }
}
