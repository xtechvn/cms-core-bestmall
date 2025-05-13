using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Orders
{
    public class OrderESModel
    {
        public long orderid { get; set; }

        public long clientid { get; set; }

        public string orderno { get; set; }

        public DateTime createddate { get; set; }

        public int? createdby { get; set; }

        public DateTime? updatelast { get; set; }

        public int? userupdateid { get; set; }

        public double? price { get; set; }

        public double? profit { get; set; }

        public double? discount { get; set; }

        public double? amount { get; set; }

        public int orderstatus { get; set; }

        public short paymenttype { get; set; }

        public int paymentstatus { get; set; }

        public string utmsource { get; set; }

        public string utmmedium { get; set; }

        /// <summary>
        /// chính là label so với wiframe
        /// </summary>
        public string note { get; set; }

        public int? voucherid { get; set; }

        public int? isdelete { get; set; }

        public int? userid { get; set; }

        public string usergroupids { get; set; }

        public string receivername { get; set; }

        public string phone { get; set; }

        public int? provinceid { get; set; }

        public int? districtid { get; set; }

        public int? wardid { get; set; }

        public string address { get; set; }
    }
}
