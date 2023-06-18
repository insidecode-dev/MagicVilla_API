using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("api/VillaAPI")] this is the same as below but [controller attribute is better because if any changes will be made in name of controller the name inside Route attribute should be changed, and this is not a good practise and it is called hardcode 
    [Route("api/[controller]")]
    [ApiController] // this attribute ensures that this is a api controller
    public class VillaAPIController:ControllerBase 
    {
        [HttpGet]// this attribute will notify the swagger documentation that this endpoint is GET endpoint 
        public IEnumerable<VillaDTO> GetVillas()
            => VillaStore.villas;

        [HttpGet("{id:int}")]
        public VillaDTO GetVilla(int id)
            => VillaStore.villas.FirstOrDefault(x=>x.Id==id);
    }
}
