using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalTest_Profescipta.DAL.Helper
{
    public partial class EFHelper
    {
        public static string GeneretaOrderBy(string TxtSortBy, string TxtDefaultSortBy, bool? BitAsc)
        {
            var orderBy = TxtSortBy;
            if (string.IsNullOrWhiteSpace(orderBy))
            {
                orderBy = TxtDefaultSortBy;
            }

            if (BitAsc ?? true)
            {
                orderBy += " asc";
            }
            else
            {
                orderBy += " desc";
            }

            return orderBy;
        }
    }
}
