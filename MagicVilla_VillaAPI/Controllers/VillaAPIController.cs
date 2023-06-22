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

        public VillaAPIController(ApplicationDbContext dbContext)
        {
            
            _dbContext = dbContext;
        }



        [HttpGet]// this attribute will notify the swagger documentation that this endpoint is GET endpoint 
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<VillaDTO>> GetVillas()
        {
            
            return Ok(_dbContext.Villas.ToList());
        }
         

        
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

            var villa = _dbContext.Villas.FirstOrDefault(x => x.Id == id);
            if (villa == null)
            {
                return NotFound();  //404
            }

            return Ok(villa);
        }

        
        [HttpPost("NewVilla", Name = "CreateVilla")] // value inside Name parameter is used for url generation, when requesting an endpoint in visual studio your request is sent to the value inside Name parameter, but a string(NewVilla) before Name parameter is just how you see as the name of endpoint in swagger documentation, when you sent request in swagger doumentation or postman you send it using this name
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            //if(!ModelState.IsValid)
            //{
            //    return BadRequest(ModelState);  
            //}

            if (_dbContext.Villas.FirstOrDefault(x=>x.Name.ToLower()==villaDTO.Name.ToLower())!=null)
            {
                ModelState.AddModelError("CustomError","Name already exists");
                return BadRequest(ModelState);
            }

            if (villaDTO is null)
            {
                return BadRequest();
            }

            if (villaDTO.Id > 0)
            {
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
            
            _dbContext.Villas.Add(new Villa {
                Id=villaDTO.Id,
                Name = villaDTO.Name,
                Amenity = villaDTO.Amenity,
                //CreatedDate = villaDTO.
                Details = villaDTO.Details,
                ImageUrl = villaDTO.ImageUrl,
                Occupancy = villaDTO.Occupancy,
                Rate = villaDTO.Rate,
                Sqft = villaDTO.Sqft,
                //UpdatedDate = villaDTO.
                });
            _dbContext.SaveChanges();
            //sometimes when resource is created you give them the URL, where the actual resource is created
            return CreatedAtRoute("GetVilla", new {id=villaDTO.Id}, villaDTO);
        }



        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult DeleteVilla(int id)
        {
            if (id == 0)
            {
                return BadRequest(); //400
            }
            var villa = _dbContext.Villas.FirstOrDefault(x=>x.Id==id);
            if (villa is null)
            {
                return NotFound(); //404
            }
            _dbContext.Villas.Remove(villa);
            _dbContext.SaveChanges();
            return NoContent(); //204
        }

        [HttpPut("{id:int}", Name ="UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if (villaDTO==null || id!=villaDTO.Id) return BadRequest(); //400
            

            var villa = _dbContext.Villas.AsNoTracking().FirstOrDefault(x=>x.Id==id);
            if (villa is null) { return NotFound(); } //404
            if (!ModelState.IsValid){ return BadRequest(ModelState); } //400

            var updatedVilla = new Villa { 
                Id = villaDTO.Id,
                Name = villaDTO.Name,
                Amenity = villaDTO.Amenity,
                Sqft = villaDTO.Sqft,
                Rate = villaDTO.Rate,
                Occupancy = villaDTO.Occupancy,
                ImageUrl = villaDTO.ImageUrl,
                Details = villaDTO.Details
            };

            _dbContext.Villas.Update(updatedVilla);
            _dbContext.SaveChanges();
            return NoContent();
        }

        [HttpPatch("{id:int}", Name ="UpdateVillaPartially")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult UpdateVillaPartially(int id, JsonPatchDocument<VillaDTO> villaDTO)
        {
            if (id == 0) return BadRequest();

            var villa = _dbContext.Villas.AsNoTracking().FirstOrDefault(x=>x.Id==id); // I use AsNoTracking here because by default ef core tracks the object retrieved from database, as the same time entity that is sent to Update() method is also being tracked, but both of these entities's id is same, and ef core cannot track two or more entity with the same id, as a result it throw exception
            if (villa is null) return BadRequest();

            //villaDTO contains just updated property, so we should create a VillaDTO object at first and initialize it with the villa object requested from database with id

            VillaDTO updatedVillaDto = new VillaDTO {
            Id = id,
            Sqft = villa.Sqft,
            Rate = villa.Rate,
            Occupancy= villa.Occupancy,
            Amenity = villa.Amenity,
            Details = villa.Details,
            ImageUrl = villa.ImageUrl,
            Name = villa.Name
            }; //conflict

            // then apply changes from jsonpatch object to our VillaDTO object
            villaDTO.ApplyTo(updatedVillaDto);

            //and then creating a villa object to send Update method of Villas DbSet object 
            Villa newVilla = new Villa { 
               Amenity = updatedVillaDto.Amenity,
               Details= updatedVillaDto.Details,
               Id = id,
               ImageUrl= updatedVillaDto.ImageUrl,
               Name = updatedVillaDto.Name,
               Occupancy= updatedVillaDto.Occupancy,
               Rate= updatedVillaDto.Rate,
               Sqft= updatedVillaDto.Sqft               
            };

            if (!ModelState.IsValid) return BadRequest(ModelState);

            //here is updated finally
            _dbContext.Villas.Update(newVilla);
            _dbContext.SaveChanges();
            return NoContent();
        }
    }
}
