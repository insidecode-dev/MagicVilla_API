using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("api/VillaAPI")] this is the same as below but [controller attribute is better because if any changes will be made in name of controller the name inside Route attribute should be changed, and this is not a good practise and it is called hardcode 
    [Route("api/[controller]")]
    [ApiController] // this attribute ensures that this is a api controller
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]// this attribute will notify the swagger documentation that this endpoint is GET endpoint 
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
            => VillaStore.villas;

        [HttpGet("{id:int}", Name = "GetVilla")] // we can give name the endpoint
        //this makes our endpoints's documented, it means we can see what status codes can it return 
        [ProducesResponseType(StatusCodes.Status200OK)] // we can write integer constant 200 instead of StatusCodes.Status200OK, but it's hardcoded
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public ActionResult<VillaDTO> GetVilla(int id) //we changed return type of actionMethod from VillaDTO to ActionResult<VillaDTO> , to work with http status codes for different situations
        {
            if (id == 0)
            {
                return BadRequest(); //400
            }

            var villa = VillaStore.villas.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();  //404
            }

            return Ok(villa);
        }

        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            if (villaDTO is null)
            {
                return BadRequest();
            }

            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            villaDTO.Id = VillaStore.villas.OrderByDescending(x => x.Id).FirstOrDefault().Id + 1;
            VillaStore.villas.Add(villaDTO);

            //sometimes when resource is created you give them the URL, where the actual resource is created
            return CreatedAtRoute("GetVilla", new { id=villaDTO.Id}, villaDTO);
        }
    }
}
