using DAL;
using Entities.ConfigModels;
using Entities.Models;
using Entities.ViewModels.Label;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public class LabelRepository : ILabelRepository
    {
        private readonly LabelDAL labelDAL;
        private readonly IOptions<DataBaseConfig> dataBaseConfig;

        public LabelRepository(IOptions<DataBaseConfig> _dataBaseConfig)
        {
            labelDAL = new LabelDAL(_dataBaseConfig.Value.SqlServer.ConnectionString);
            dataBaseConfig = _dataBaseConfig;
        }

        public async Task<List<LabelListingModel>> Listing(int status = -1, string label_name = null, int page_index = 1, int page_size = 100)
        {
            return await labelDAL.Listing(status,label_name,page_index,page_size);
        }
        public Task<Label> GetById(int Id)
        {
            return labelDAL.FindAsync(Id);
        }
    }
}
