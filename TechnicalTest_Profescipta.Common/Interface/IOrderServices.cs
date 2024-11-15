using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechnicalTest_Profescipta.Common.DTO;

namespace TechnicalTest_Profescipta.Common.Interface
{
    public interface IOrderServices
    {
        Task<SalesOrder> SaveOrUpdate(SalesOrder request, CancellationToken cancellationToken = default);
        Task<DatatablesResponse<List<SalesOrder>>> datalist(DatatableRequestOrder request, CancellationToken cancellationToken = default);
        Task<Boolean> Delete(long orderId, CancellationToken cancellationToken = default);
        Task<SalesOrder> getbyId(long id, CancellationToken cancellationToken= default);
        Task<Stream> export(CancellationToken cancellationToken = default);
    }
}
