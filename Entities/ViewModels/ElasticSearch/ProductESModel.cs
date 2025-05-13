using System;

public class ProductESModel
{

   public long id { get; set; } // ID ElasticSearch
   public string product_id { get; set; }
   public string name { get; set; }
   public double amount { get; set; }
   public string product_code { get; set; }
   public string product_name_no_tv { get; set; }
   public string description { get; set; }
}