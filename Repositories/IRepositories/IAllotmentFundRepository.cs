using Entities.Models;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllotmentFundRepository
    {
        public int Insert(AllotmentFund model);
        public int Update(AllotmentFund model);
        public AllotmentFund GetByAccountClientId(long accountClientId);
        Task<AllotmentFund> GetByClientId(long clientId);
    }
}
