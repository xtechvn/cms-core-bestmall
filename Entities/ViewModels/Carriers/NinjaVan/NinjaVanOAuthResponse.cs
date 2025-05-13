using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Carriers.NinjaVan
{
    public class NinjaVanOAuthResponse
    {
        public string access_token { get; set; }
        public long expires { get; set; }
        public long expires_in { get; set; }
    }
}
