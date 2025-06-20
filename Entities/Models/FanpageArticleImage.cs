using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class FanpageArticleImage
{
    public long Id { get; set; }

    public long ArticleId { get; set; }

    public string ImageUrl { get; set; }

    public DateTime CreatedDate { get; set; }
}
