using Entities.ViewModels.Funding;
using System.Collections.Generic;
using Entities.Models;
using System.Threading.Tasks;
using Entities.ViewModels.SupplierConfig;
using Entities.ViewModels;

namespace Repositories.IRepositories
{
    public interface ISupplierRepository
    {
        List<SupplierViewModel> GetSuppliers(SupplierSearchModel searchModel);
        SupplierViewModel GetById(int supplierId);
        SupplierDetailViewModel GetDetailById(int supplierId);

        int Add(SupplierConfigUpsertModel model);
        int Update(SupplierConfigUpsertModel model);
        Task<string> ExportSuppliers(SupplierSearchModel searchModel, string FilePath);
        Task<List<Supplier>> GetSuggestionList(string name);


        public Supplier GetSuplierById(long supplierId);
        public IEnumerable<Supplier> GetSuggestSupplier(string text, int limit);
        public IEnumerable<Supplier> GetSuggestSupplierForHotel(int hotel_id, string text, int limit);
        int GetByIDOrName(int suplier_id, string name);

        // Contact
        public IEnumerable<SupplierContactViewModel> GetSupplierContactList(int supplier_id);
        public SupplierContact GetSupplierContactById(int Id);
        public int UpsertSupplierContact(SupplierContact model);
        public long DeleteSupplierContact(long id);

        // Banking account
        IEnumerable<SupplierPaymentViewModel> GetSupplierPaymentList(int supplier_id);
        BankingAccount GetSupplierPaymentById(int Id);
        int UpsertSupplierPayment(BankingAccount model);
        int DeleteSupplierPayment(int id);

        // Order history
        GenericViewModel<SupplierOrderGridViewModel> GetSupplierOrderList(SupplierOrderSearchModel model);

        // Service
        GenericViewModel<SupplierOrderGridViewModel> GetSupplierServiceList(SupplierServiceSearchModel model);

        // Ticket
        GenericViewModel<SupplierTicketGridViewModel> GetSupplierTicketList(SupplierTicketSearchModel model);

        // Program
        GenericViewModel<SupplierProgramGridViewModel> GetSupplierProgramList(SupplierProgramSearchModel model);

    }
}
