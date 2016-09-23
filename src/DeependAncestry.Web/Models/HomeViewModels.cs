using System.ComponentModel.DataAnnotations;

namespace DeependAncestry.Web.Models
{

    public class SearchFieldsViewModel
    {
        [Required(ErrorMessage = "Please enter name")]
        public string Name { get; set; }
        public string Gender { get; set; }
        public int? Page { get; set; }
    }

    public class GenderViewModel
    {
        public string Gender { get; set; }
        public string GenderName { get; set; }
        public string IsSelected { get; set; }
    }

    public class DirectionViewModel
    {
        public string Direction { get; set; }
        public string DirectionName { get; set; }
        public string IsSelected { get; set; }
    }

}