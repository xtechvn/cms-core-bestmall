using Entities.Models;
using System.Data;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IOrderMergeRepository
    {
        public  Task<long> InsertOrderMerge(OrderMerge model);


        public  Task<long> UpdateOrderMerge(OrderMerge model);


        public  Task<DataTable> GetOrderMergePaging(int pageIndex, int pageSize, string keyword);
        public OrderMerge GetById(long OrderId);

    }
}
