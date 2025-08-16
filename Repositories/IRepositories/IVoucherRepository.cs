using DAL;
using Entities.Models;
using Entities.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Repositories.IRepositories
{
    public interface IVoucherRepository
    {
        public Task<GenericViewModel<Voucher>> GetVoucherPagingList(string keyword, int status, int pageIndex, int pageSize, long? client_id = null);
        public  Task<int> InsertVoucher(Voucher model);
        public  Task<int> UpdateVoucher(Voucher model);
        public  Task<Voucher> GetById(int voucherId);
    }
}
