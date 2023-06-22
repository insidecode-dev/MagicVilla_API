using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    //[Route("api/VillaAPI")] this is the same as below but [controller attribute is better because if any changes will be made in name of controller the name inside Route attribute should be changed, and this is not a good practise and it is called hardcode 
    [Route("api/[controller]")]

    // [ApiController] attribute ensures that this is a api controller
    // It also gives us some additional help on the controller, it identifies that this is an api controller and it includes some basic features for example when using validations in model or entity, makes model validation is active, otherwise we should use ModelState.IsValid in each action that we use entity for model validation 
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private readonly ApplicationDbContext _dbContext;

        //using automapper
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext dbContext, IMapper mapper)
        {

            _dbContext = dbContext;
            _mapper = mapper;
        }



        [HttpGet]// this attribute will notify the swagger documentation that this endpoint is GET endpoint 
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillasAsync()
        {
            IEnumerable<Villa> villas = await _dbContext.Villas.ToListAsync();
            return Ok(_mapper.Map<List<VillaDTO>>(villas));
        }



        [HttpGet("{id:int}", Name = "GetVilla")] // we can give name the endpoint

        //this makes our endpoints's documented, it means we can see what status codes can it return 
        [ProducesResponseType(StatusCodes.Status200OK)] // we can write integer constant 200 instead of StatusCodes.Status200OK, but it's hardcoded
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<VillaDTO>> GetVillaAsync(int id) //we changed return type of actionMethod from VillaDTO to ActionResult<VillaDTO> , to work with http status codes for different situations
        {
            if (id == 0)
            {
                return BadRequest(); //400
            }

            var villa = await _dbContext.Villas.FirstOrDefaultAsync(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();  //404
            }

            return Ok(_mapper.Map<VillaDTO>(villa));
        }


        [HttpPost("NewVilla", Name = "CreateVilla")] // value inside Name parameter is used for url generation, when requesting an endpoint in visual studio your request is sent to the value inside Name parameter, but a string(NewVilla) before Name parameter is just how you see as the name of endpoint in swagger documentation, when you sent request in swagger doumentation or postman you send it using this name
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<VillaDTO>> CreateVillaAsync([FromBody] VillaCreateDTO createDTO)
        {

            if (await _dbContext.Villas.FirstOrDefaultAsync(x => x.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError("CustomError", "Name already exists");
                return BadRequest(ModelState);
            }

            if (createDTO is null)
            {
                return BadRequest();
            }

            var villa = _mapper.Map<Villa>(createDTO);

            await _dbContext.Villas.AddAsync(villa);
            await _dbContext.SaveChangesAsync();
            //sometimes when resource is created you give them the URL, where the actual resource is created
            return CreatedAtRoute("GetVilla", new { id = villa.Id }, villa);
        }



        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteVillaAsync(int id)
        {
            if (id == 0)
            {
                return BadRequest(); //400
            }
            var villa = await _dbContext.Villas.FirstOrDefaultAsync(x => x.Id == id);
            if (villa is null)
            {
                return NotFound(); //404
            }
            _dbContext.Villas.Remove(villa);
            await _dbContext.SaveChangesAsync();
            return NoContent(); //204
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVillaAsync(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            if (updateDTO == null || id != updateDTO.Id) return BadRequest(); //400


            var villa = await _dbContext.Villas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id);
            if (villa is null) { return NotFound(); } //404
            if (!ModelState.IsValid) { return BadRequest(ModelState); } //400

            var updatedVilla = _mapper.Map<Villa>(updateDTO);

            _dbContext.Villas.Update(updatedVilla);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name = "UpdateVillaPartially")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateVillaPartiallyAsync(int id, JsonPatchDocument<VillaUpdateDTO> villaDTO)
        {
            if (id == 0) return BadRequest();

            var villa = await _dbContext.Villas.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id); // I use AsNoTracking here because by default ef core tracks the object retrieved from database, as the same time entity that is sent to Update() method is also being tracked, but both of these entities's id is same, and ef core cannot track two or more entity with the same id, as a result it throw exception

            if (villa is null) return BadRequest();

            //villaDTO contains just updated property, so we should create a VillaDTO object at first and initialize it with the villa object requested from database with id
            var updatedVillaDTO = _mapper.Map<VillaUpdateDTO>(villa);            

            // then apply changes from jsonpatch object to our VillaDTO object
            villaDTO.ApplyTo(updatedVillaDTO);

            //and then creating a villa object to send Update method of Villas DbSet object             
            var newVilla = _mapper.Map<Villa>(updatedVillaDTO);

            //
            if (!ModelState.IsValid) return BadRequest(ModelState);

            //here is updated finally
            _dbContext.Villas.Update(newVilla);
            await _dbContext.SaveChangesAsync();
            return NoContent();
        }
    }
}
