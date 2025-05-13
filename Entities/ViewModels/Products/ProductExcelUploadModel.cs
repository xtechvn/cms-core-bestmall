using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Entities.ViewModels.Products
{
    public class ProductExcelUploadModel
    {
        public int group_product_id { get; set; }
        public string name { get; set; }
        public string description { get; set; }
        public string sku { get; set; }
        public string product_code { get; set; }
        public string attribute_1_name { get; set; }
        public string variation_1_name { get; set; }
        public string variation_images { get; set; }
        public string attribute_2_name { get; set; }
        public string variation_2_name { get; set; }
        public string variation_sku { get; set; }
        public double price { get; set; }
        public double profit { get; set; }
        public double amount { get; set; }
        public int stock { get; set; }
        public string avatar { get; set; }
        public string video { get; set; }
        public string image_1 { get; set; }
        public string image_2 { get; set; }
        public string image_3 { get; set; }
        public string image_4 { get; set; }
        public string image_5 { get; set; }
        public string image_6 { get; set; }
        public string image_7 { get; set; }
        public string image_8 { get; set; }
        public float weight { get; set; }
        public float width { get; set; }
        public float height    { get; set; }
        public float depth { get; set; }
        public string brand { get; set; }

    }
}
