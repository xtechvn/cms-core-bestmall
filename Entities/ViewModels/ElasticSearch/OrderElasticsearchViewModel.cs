using Entities.Models;
using Nest;
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
    public class OrderMergeESModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
       
        [PropertyName("ClientId")]

        public long ClientId { get; set; }
        [PropertyName("OrderNo")]

        public string OrderNo { get; set; }
        [PropertyName("CreatedDate")]

        public DateTime CreatedDate { get; set; }
        [PropertyName("CreatedBy")]

        public int? CreatedBy { get; set; }
        [PropertyName("UpdateLast")]

        public DateTime? UpdateLast { get; set; }
        [PropertyName("UserUpdateId")]

        public int? UserUpdateId { get; set; }
        [PropertyName("Price")]

        public double? Price { get; set; }
        [PropertyName("Profit")]

        public double? Profit { get; set; }
        [PropertyName("Discount")]

        public double? Discount { get; set; }
        [PropertyName("Amount")]

        public double? Amount { get; set; }
        [PropertyName("OrderStatus")]

        public int? OrderStatus { get; set; }
        [PropertyName("PaymentType")]

        public short? PaymentType { get; set; }
        [PropertyName("PaymentStatus")]

        public int? PaymentStatus { get; set; }
        [PropertyName("UtmSource")]

        public string UtmSource { get; set; }
        [PropertyName("UtmMedium")]

        public string UtmMedium { get; set; }

        [PropertyName("Note")]


        public string Note { get; set; }
        [PropertyName("VoucherId")]

        public int? VoucherId { get; set; }
        [PropertyName("IsDelete")]

        public int? IsDelete { get; set; }
        [PropertyName("UserId")]

        public int? UserId { get; set; }
        [PropertyName("UserGroupIds")]

        public string UserGroupIds { get; set; }
        [PropertyName("ReceiverName")]

        public string ReceiverName { get; set; }
        [PropertyName("Phone")]

        public string Phone { get; set; }
        [PropertyName("ProvinceId")]

        public int? ProvinceId { get; set; }
        [PropertyName("DistrictId")]

        public int? DistrictId { get; set; }
        [PropertyName("WardId")]

        public int? WardId { get; set; }
        [PropertyName("Address")]

        public string Address { get; set; }

        [PropertyName("RefundStatus")]

        public int? RefundStatus { get; set; }
        [PropertyName("RefundReason")]

        public string RefundReason { get; set; }
        [PropertyName("RefundDate")]

        public DateTime? RefundDate { get; set; }
        [PropertyName("ShippingFee")]

        public double? ShippingFee { get; set; }

    }

}
