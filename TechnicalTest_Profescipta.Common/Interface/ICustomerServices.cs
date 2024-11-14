using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechnicalTest_Profescipta.Common.Entity;

namespace TechnicalTest_Profescipta.Common.Interface
{
    public interface ICustomerServices
    {
        Task<List<ComCustomer>> getListCustomer(CancellationToken cancellationToken = default);
    }
}
