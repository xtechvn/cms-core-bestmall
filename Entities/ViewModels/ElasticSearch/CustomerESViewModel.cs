using Nest;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
    public class CustomerESViewModel
    {
        [PropertyName("Id")]

        public long Id { get; set; }
        [PropertyName("ClientMapId")]

        public int? ClientMapId { get; set; }
        [PropertyName("SaleMapId")]

        public int? SaleMapId { get; set; }
        [PropertyName("ClientType")]

        public int? ClientType { get; set; }
        [PropertyName("ClientName")]

        public string ClientName { get; set; }
        [PropertyName("Email")]

        public string Email { get; set; }
        [PropertyName("Gender")]

        public int? Gender { get; set; }
        [PropertyName("Status")]

        public int Status { get; set; }
        [PropertyName("Note")]

        public string Note { get; set; }
        [PropertyName("Avartar")]

        public string Avartar { get; set; }
        [PropertyName("JoinDate")]

        public DateTime JoinDate { get; set; }
        [PropertyName("IsReceiverInfoEmail")]

        public bool? IsReceiverInfoEmail { get; set; }
        [PropertyName("Phone")]

        public string Phone { get; set; }
        [PropertyName("Birthday")]

        public DateTime? Birthday { get; set; }
        [PropertyName("UpdateTime")]

        public DateTime? UpdateTime { get; set; }
        [PropertyName("TaxNo")]

        public string TaxNo { get; set; }
        [PropertyName("AgencyType")]

        public int? AgencyType { get; set; }
        [PropertyName("PermisionType")]

        public int? PermisionType { get; set; }
        [PropertyName("BusinessAddress")]

        public string BusinessAddress { get; set; }
        [PropertyName("ExportBillAddress")]

        public string ExportBillAddress { get; set; }
        [PropertyName("ClientCode")]

        public string ClientCode { get; set; }
        [PropertyName("IsRegisterAffiliate")]

        public bool? IsRegisterAffiliate { get; set; }
        [PropertyName("ReferralId")]

        public string ReferralId { get; set; }
        [PropertyName("ParentId")]

        public int? ParentId { get; set; }
        [PropertyName("CitizenId")]

        public string CitizenId { get; set; }
    }
    public class ClientESViewModel
    {
      
        public long _id { get; set; } // ID customer
        public string ClientName { get; set; }
        public string Email { get; set; }
        public int Status { get; set; }
        public string Phone { get; set; }
        public DateTime JoinDate { get; set; }
        public int ClientType { get; set; }
        public string unix_timestamp { get; set; }
        public string suggest_search { get; set; }
        public string userid { get; set; }

    }
    public class earchClientESViewModel
    {

        public string clientname { get; set; }
        public string phone { get; set; }
        public string email { get; set; }
        public string id { get; set; }
        public string clienttype { get; set; }
        public string userid { get; set; }

    }
}
