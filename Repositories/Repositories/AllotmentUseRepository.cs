using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Threading.Tasks;

namespace HuloToys_Service.Repositories
{
    public class AllotmentUseRepository: IAllotmentUseRepository
    {
        private readonly AllotmentUseDAL allotmentUseDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AllotmentUseRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            allotmentUseDAL = new AllotmentUseDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }
        public int Insert(AllotmentUse model)
        {
            return allotmentUseDAL.Insert(model);
        }
        public int Update(AllotmentUse model)
        {
            return allotmentUseDAL.Update(model);

        }
       
        public async Task<AllotmentUse> GetByDataId(long dataId)
        {
            return await allotmentUseDAL.GetByDataId(dataId);
        }
    }
}
