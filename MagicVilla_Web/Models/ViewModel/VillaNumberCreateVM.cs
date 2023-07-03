using MagicVilla_Web.Models.Dto;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MagicVilla_Web.Models.ViewModel
{
    public class VillaNumberCreateVM
    {
        public VillaNumberCreateDTO VillaNumberCreateDTO { get; set; }
        public VillaNumberCreateVM()
        {
            VillaNumberCreateDTO = new VillaNumberCreateDTO();
        }


        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }  
    }
}
