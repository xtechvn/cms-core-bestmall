using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Utilities.Contants.ProductV2
{
    public enum ProductStatus
    {
        ACTIVE=1,
        DEACTIVE=2,//ẩn fe
        REMOVE=3, // ẩn fe vs cms
        ON_WAITING_CONFIRM = 4 // chờ duyệt

    }
}
