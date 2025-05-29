using DAL;
using Entities.Models;
using Entities.ViewModels.Label;
using HuloToys_Service.Models.Label;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ILabelRepository
    {
        public Task<List<LabelListingModel>> Listing(int status = -1, string label_name = null, string label_code = null, int page_index = 1, int page_size = 100);
        Task<Label> GetById(int Id);
        int Insert(Label model);
        int Update(Label model);
        Task<Label> GetByCode(string code);
    }
}
