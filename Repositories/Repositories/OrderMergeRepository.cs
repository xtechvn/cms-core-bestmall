using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Data;
using System.Threading.Tasks;

namespace Repositories.Repositories
{
    public class OrderMergeRepository : IOrderMergeRepository
    {
        private readonly OrderMergeDAL _orderMergeDAL;


        public OrderMergeRepository(IOptions<DataBaseConfig> dataBaseConfig)
        {
            _orderMergeDAL = new OrderMergeDAL(dataBaseConfig.Value.SqlServer.ConnectionString);

        }

        public async Task<long> InsertOrderMerge(OrderMerge model)
        {
           return await _orderMergeDAL.InsertOrderMerge(model);
        }


        public async Task<long> UpdateOrderMerge(OrderMerge model)
        {
            return await _orderMergeDAL.UpdateOrderMerge(model);

        }


        public async Task<DataTable> GetOrderMergePaging(int pageIndex, int pageSize, string keyword)
        {
            return await _orderMergeDAL.GetOrderMergePaging(pageIndex,pageSize,keyword);

        }
        public OrderMerge GetById(long OrderId)
        {
            return  _orderMergeDAL.GetById(OrderId);

        }
    }
}
