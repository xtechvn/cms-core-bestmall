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
        public Task<GenericViewModel<Voucher>> GetVoucherPagingList(string keyword, int status, int pageIndex, int pageSize);
        public  Task<long> InsertVoucher(Voucher model);
        public  Task<long> UpdateVoucher(Voucher model);
        public  Task<Voucher> GetById(int voucherId);
    }
}
