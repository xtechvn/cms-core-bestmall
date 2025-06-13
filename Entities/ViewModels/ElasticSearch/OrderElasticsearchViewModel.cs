using Entities.Models;
using System;

namespace ENTITIES.ViewModels.ElasticSearch
{
   public class OrderElasticsearchViewModel
    {

        public int Id { get; set; }
        public int ClientId { get; set; }
        public string OrderNo { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CreatedBy { get; set; }
        public DateTime UpdateLast { get; set; }
        public int? UserUpdateId { get; set; } // Nullable int
        public double Price { get; set; }
        public double Profit { get; set; }
        public double Discount { get; set; }
        public double Amount { get; set; }
        public int OrderStatus { get; set; }
        public int? PaymentType { get; set; }
        public int? PaymentStatus { get; set; }
        public string UtmSource { get; set; }
        public string UtmMedium { get; set; }
        public string Note { get; set; }
        public int? VoucherId { get; set; }
        public int? IsDelete { get; set; }
        public int? UserId { get; set; }
        public string UserGroupIds { get; set; }
        public string ReceiverName { get; set; }
        public string Phone { get; set; }
        public int? ProvinceId { get; set; }
        public int? DistrictId { get; set; }
        public int? WardId { get; set; }
        public string Address { get; set; }
    }
}
