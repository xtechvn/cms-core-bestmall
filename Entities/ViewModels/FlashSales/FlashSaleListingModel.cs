using Entities.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.FlashSales
{
    public class FlashSaleListingModel: Entities.Models.FlashSale
    {
        public long FlashSaleProductCount { get; set; }
        public string FlashSaleStatusText { get; set; }
    }
}
