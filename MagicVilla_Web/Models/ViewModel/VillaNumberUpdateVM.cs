using MagicVilla_Web.Models.Dto;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace MagicVilla_Web.Models.ViewModel
{
    public class VillaNumberUpdateVM
    {
        public VillaNumberUpdateDTO VillaNumberUpdateDTO{ get; set; }
        public VillaNumberUpdateVM()
        {
            VillaNumberUpdateDTO = new VillaNumberUpdateDTO();
        }


        [ValidateNever]
        public IEnumerable<SelectListItem> VillaList { get; set; }  
    }
}
