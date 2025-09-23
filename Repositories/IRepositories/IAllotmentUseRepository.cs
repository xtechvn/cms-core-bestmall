using Entities.Models;
using Entities.ViewModels;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IAllotmentUseRepository
    {
        public int Insert(AllotmentUse model);
        public int Update(AllotmentUse model);
        public  Task<AllotmentUse> GetByDataId(long dataId);
    }
}
