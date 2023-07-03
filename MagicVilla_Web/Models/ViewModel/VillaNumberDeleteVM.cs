using MagicVilla_Web.Models.Dto;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MagicVilla_Web.Models.ViewModel
{
    public class VillaNumberDeleteVM
    {
        public VillaNumberDTO VillaNumberDTO { get; set; }
        public VillaNumberDeleteVM()
        {
            VillaNumberDTO = new VillaNumberDTO();
        }


        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }  
    }
}
