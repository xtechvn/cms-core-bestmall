using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.Models
{
    public class FanpageArticleImage
    {
        public long Id { get; set; }
        public long ArticleId { get; set; }
        public string ImageUrl { get; set; }
        public DateTime CreatedDate { get; set; }
    }

}
