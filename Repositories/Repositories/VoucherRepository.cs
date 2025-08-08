using DAL;
using DAL.StoreProcedure;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Nest;
using Repositories.IRepositories;
using Repositories.Repositories.BaseRepos;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Utilities;
using Utilities.Contants;

namespace Repositories.Repositories
{
    public class VoucherRepository : IVoucherRepository
    {
        private readonly VoucherDAL voucherDAL;


        public VoucherRepository(IHttpContextAccessor context, IOptions<DataBaseConfig> dataBaseConfig,
            IOptions<MailConfig> mailConfig, ILogger<UserRepository> logger)
        {
            voucherDAL = new VoucherDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }
        public async Task<GenericViewModel<Voucher>> GetVoucherPagingList(string keyword, int status, int pageIndex, int pageSize)
        {
            var model = new GenericViewModel<Voucher>();

            try
            {
                DataTable dt = await voucherDAL.GetVoucherPagingList(keyword,status,pageIndex,pageSize);
                if (dt != null && dt.Rows.Count > 0)
                {
                    model.ListData = (from row in dt.AsEnumerable()
                                      select new Voucher
                                      {
                                          Id = Convert.ToInt32(row["Id"]),
                                          Code = row["code"].ToString(),
                                          Cdate = row["cdate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["cdate"]) : null,
                                          Udate = row["udate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["udate"]) : null,
                                          EDate = row["eDate"] != DBNull.Value ? (DateTime?)Convert.ToDateTime(row["eDate"]) : null,
                                          LimitUse = Convert.ToInt32(row["limitUse"]),
                                          PriceSales = row["price_sales"] != DBNull.Value ? (decimal?)Convert.ToDecimal(row["price_sales"]) : null,
                                          Unit = row["unit"].ToString(),
                                          RuleType = row["rule_type"] != DBNull.Value ? (int?)Convert.ToInt32(row["rule_type"]) : null,
                                          GroupUserPriority = row["group_user_priority"] != DBNull.Value ? row["group_user_priority"].ToString() : null,
                                          IsPublic = row["is_public"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_public"]) : null,
                                          Description = row["description"] != DBNull.Value ? row["description"].ToString() : null,
                                          IsLimitVoucher = row["is_limit_voucher"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_limit_voucher"]) : null,
                                          LimitTotalDiscount = row["limit_total_discount"] != DBNull.Value ? (double?)Convert.ToDouble(row["limit_total_discount"]) : null,
                                          StoreApply = row["store_apply"] != DBNull.Value ? row["store_apply"].ToString() : null,
                                          IsMaxPriceProduct = row["is_max_price_product"] != DBNull.Value ? (bool?)Convert.ToBoolean(row["is_max_price_product"]) : null,
                                          MinTotalAmount = row["min_total_amount"] != DBNull.Value ? (double?)Convert.ToDouble(row["min_total_amount"]) : null,
                                          CampaignId = row["campaign_id"] != DBNull.Value ? (int?)Convert.ToInt32(row["campaign_id"]) : null
                                      }).ToList();
                    model.CurrentPage = pageIndex;
                    model.PageSize = pageSize;
                    model.TotalRecord = Convert.ToInt32(dt.Rows[0]["TotalRow"]);
                    model.TotalPage = (int)Math.Ceiling((double)model.TotalRecord / model.PageSize);
                }
            }
            catch (Exception ex)
            {
                LogHelper.InsertLogTelegram("GetList - OrderRepository: " + ex);
            }
            return model;
        }
        public async Task<long> InsertVoucher(Voucher model)
        {
            return await voucherDAL.InsertVoucher(model);
        }
        public async Task<long> UpdateVoucher(Voucher model)
        {
            return await voucherDAL.UpdateVoucher(model);
        }
        public async Task<Voucher> GetById(int voucherId)
        {
            return await voucherDAL.GetById(voucherId);
        }

    }
}
