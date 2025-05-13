using System;

namespace entities.models;

public partial class OrderDetailESModel
{
    public long orderdetailid { get; set; }

    public long orderid { get; set; }

    public string productid { get; set; }

    public string productcode { get; set; }

    public double? amount { get; set; }

    public double? price { get; set; }

    public double? profit { get; set; }

    public double? discount { get; set; }

    public int? quantity { get; set; }

    public double? totalprice { get; set; }

    public double? totalprofit { get; set; }

    public double? totaldiscount { get; set; }

    public double? totalamount { get; set; }

    public string productlink { get; set; }

    public int? usercreate { get; set; }

    public DateTime? createddate { get; set; }

    public int? userupdated { get; set; }

    public DateTime? updateddate { get; set; }
}
