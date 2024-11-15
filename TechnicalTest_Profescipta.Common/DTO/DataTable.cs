using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TechnicalTest_Profescipta.Common.Entity;

namespace TechnicalTest_Profescipta.Common.DTO
{
    public class DatatablesRequest
    {
        public int draw { get; set; }
        public int start { get; set; }
        public int length { get; set; }
        public search search { get; set; }
        public List<order> order { get; set; }
    }


    public class DatatableRequestOrder : DatatablesRequest
    {
        public DateTime? orderDate { get; set; }
    }
    public class search
    {
        public string value { get; set; }
        public bool regex { get; set; }
    }

    public class order
    {
        public string dir { get; set; }
        public string column { get; set; }
    }

    public class DatatablesResponse<T>
    {
        public int draw { get; set; }
        public int recordsTotal { get; set; }
        public int recordsFiltered { get; set; }
        public T data { get; set; }
    }

}
