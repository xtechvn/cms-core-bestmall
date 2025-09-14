using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Microsoft.Extensions.Options;
using Repositories.IRepositories;
using System.Threading.Tasks;

namespace HuloToys_Service.Repositories
{
    public class AllotmentFundRepository : IAllotmentFundRepository
    {
        private readonly AllotmentFundDAL allotmentFundDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public AllotmentFundRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            allotmentFundDAL = new AllotmentFundDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }
        public int Insert(AllotmentFund model)
        {
            return allotmentFundDAL.Insert(model);
        }
        public int Update(AllotmentFund model)
        {
            return allotmentFundDAL.Update(model);

        }
        public AllotmentFund GetByAccountClientId(long accountClientId)
        {
            return allotmentFundDAL.GetByAccountClientId(accountClientId);
        }
        public async Task<AllotmentFund> GetByClientId(long clientId)
        {
            return await allotmentFundDAL.GetByClientId(clientId);
        }
    }
}
