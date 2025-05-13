using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Label
{
    public class LabelSearchRequestViewModel
    {
        public string name { get; set; } = string.Empty;
        public string code { get; set; } = string.Empty;
        public int status { get; set; }
        public int currentPage { get; set; }
        public int pageSize { get; set; }
    }
}
