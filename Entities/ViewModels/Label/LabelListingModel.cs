namespace Entities.ViewModels.Label
{
    public class LabelListingModel : Entities.Models. Label
    {
        public string ShortName { get; set; }
        public string FullName { get; set; }
        public int TotalRow { get; set; }
    }
}
