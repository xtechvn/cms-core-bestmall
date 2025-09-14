using System;
using System.Collections.Generic;

namespace Entities.Models;

public partial class AllotmentFund
{
    public int Id { get; set; }

    public int FundType { get; set; }

    public double AccountBalance { get; set; }

    public long? AccountClientId { get; set; }

    public DateTime? CreateDate { get; set; }

    public DateTime? UpdateTime { get; set; }
}
