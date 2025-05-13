using Entities.Models;
using Entities.ViewModels.Label;
using HuloToys_Service.Models.Label;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface ILabelRepository
    {
        public Task<List<LabelListingModel>> Listing(int status = -1, string label_name = null, int page_index = 1, int page_size = 100);
        Task<Label> GetById(int Id);
    }
}
