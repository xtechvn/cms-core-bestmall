using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace Entities.ViewModels.ElasticSearch
{
    public class CustomerESViewModel
    {
        // Đặt tên JsonProperty trùng với tên cột trong ảnh
        [JsonProperty("Id")]
        public int Id { get; set; }

        [JsonProperty("ClientName")]
        public string ClientName { get; set; }

        [JsonProperty("ClientCode")]
        public string ClientCode { get; set; }

        [JsonProperty("ClientType")]
        public string ClientType { get; set; }

        [JsonProperty("Email")]
        public string Email { get; set; }

        [JsonProperty("Status")]
        public int Status { get; set; }

        [JsonProperty("Phone")]
        public string Phone { get; set; } // string vì có thể null hoặc chuỗi số

        [JsonProperty("JoinDate")]
        public DateTime JoinDate { get; set; }

        [JsonProperty("UpdateTime")]
        public DateTime UpdateTime { get; set; }

        [JsonProperty("suggest_search")] // Cột này vẫn là snake_case trong ảnh
        public string SuggestSearch { get; set; }

        [JsonProperty("UserId")]
        public string UserId { get; set; } // Dựa trên ảnh là null, có thể là string hoặc int?

        [JsonProperty("TaxNo")]
        public string TaxNo { get; set; } // Dựa trên ảnh là null, có thể là string

        [JsonProperty("BusinessAddress")]
        public string BusinessAddress { get; set; } // Dựa trên ảnh là null, có thể là string

        [JsonProperty("ExportBill")] // Cột mới
        public string ExportBill { get; set; } // Dựa trên ảnh là null, có thể là string

        [JsonProperty("Gender")] // Cột mới
        public string Gender { get; set; } // Dựa trên ảnh là null, có thể là string (ví dụ: "Nam", "Nữ") hoặc int (0, 1)

        [JsonProperty("Birthday")] // Cột mới
        public DateTime? Birthday { get; set; } // DateTime? vì có thể null
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
