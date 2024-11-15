using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TechnicalTest_Profescipta.Common.DTO
{
    public class SalesOrder
    {
        public long SoOrderId { get; set; }

        public string OrderNo { get; set; } = null!;

        public DateTime? OrderDate { get; set; } = null!;

        public int ComCustomerId { get; set; }

        public string Address { get; set; } = null!;
        public List<SalesOrderItem> Items { get; set; } = new List<SalesOrderItem>();

        [NotMapped]
        public string CustomerName { get; set; }
    }

    public class SalesOrderItem
    {
        public long SoItemId { get; set; }
        public long SoOrderId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
        public double Price { get; set; }
        public double Total => Quantity * Price;
        public int no { get; set; } = 1;
        public Boolean isSave { get; set; } = true;

    }

}
