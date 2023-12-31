﻿using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System.Net;
using System.Web;
//using System.Text.Json;


namespace MagicVilla_VillaAPI.Controllers.v1
{
    //[Route("api/VillaAPI")] this is the same as below but [controller attribute is better because if any changes will be made in name of controller the name inside Route attribute should be changed, and this is not a good practise and it is called hardcode 
    
    [Route("api/v{version:apiVersion}/[controller]")] 
    [ApiVersion("1.0")]

    // [ApiController] attribute ensures that this is a api controller
    // It also gives us some additional help on the controller, it identifies that this is an api controller and it includes some basic features for example when using validations in model or entity, makes model validation is active, otherwise we should use ModelState.IsValid in each action that we use entity for model validation 
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        protected ApiResponse _apiResponse;

        private readonly IVillaRepository _villaRepository;

        //using automapper
        private readonly IMapper _mapper;

        public VillaAPIController(IMapper mapper, IVillaRepository villaRepository)
        {
            _mapper = mapper;
            _villaRepository = villaRepository;
            _apiResponse = new();
        }


        [HttpGet]// this attribute will notify the swagger documentation that this endpoint is GET endpoint         

        //[ResponseCache(Duration =30)] // with this attribute response of this endpoint is cached for 30seconds, it means once we sent request to this request, it will retrieve data from database but for next 30 seconds if we send request again it will not hit this endpoint, because result has been cached 

        //but instead of writing as above we can CacheProfile, in startup we give it name and set parameters, in controller we just call CacheProfile with its name inside ResponseCache and use its all predefined settings
        [ResponseCache(CacheProfileName = "Default30")]

        [ProducesResponseType(StatusCodes.Status403Forbidden)]// if I'm admin and if this is for custom role it returns this http status code
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]// if I'm not authorized, it means if I'm not logged in and got a jwt token 
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetVillasAsync([FromQuery(Name = "FilterOccupancy")] int? occupancy, [FromQuery] string? search, [FromQuery(Name ="how many villa ?")] int _pageSize = 0, [FromQuery(Name = "which page ?")] int _pageNumber=1)
        {
            try
            {
                // added filtering for Occupancy
                IEnumerable<Villa>? villas = null;
                if (occupancy>0)
                {
                    villas = await _villaRepository.GetAllAsync(x => x.Occupancy == occupancy, pageSize:_pageSize, pageNumber:_pageNumber);
                }
                else
                {
                    villas = await _villaRepository.GetAllAsync(pageSize: _pageSize, pageNumber: _pageNumber);
                }

                // added search for amenity and name properties 
                if (!string.IsNullOrEmpty(search))
                {
                    villas = villas.Where(x=> x.Amenity.ToLower().Contains(search.ToLower()) || x.Name.ToLower().Contains(search.ToLower()));
                }

                
                //adding pahgination information to header
                PaginationForResponseHeader pgResponseHeader = new() { PageNumber=_pageNumber, PageSize=_pageSize};
                //Response is property of ControllerBase class, its type is HttpResponse, that manipulates HttpResponse for executing action
                Response.Headers.Add("Pagination",JsonConvert.SerializeObject(pgResponseHeader));


                _apiResponse.Result = _mapper.Map<List<VillaDTO>>(villas);
                // find solution for dynamically insert code of httpsstatus code 
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
                //                                                
            }
            return _apiResponse;
        }




        [HttpGet("{id:int}", Name = "GetVilla")] // we can give name the endpoint

        //this makes our endpoints's documented, it means we can see what status codes can it return 
        [ProducesResponseType(StatusCodes.Status200OK)] // we can write integer constant 200 instead of StatusCodes.Status200OK, but it's hardcoded
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> GetVillaAsync(int id) //we changed return type of actionMethod from VillaDTO to ActionResult<VillaDTO> , to work with http status codes for different situations
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest(); //400
                }
                var villa = await _villaRepository.GetAsync(filter: x => x.Id == id, tracked: false);
                if (villa == null)
                {
                    return NotFound();  //404
                }
                _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
                //                                                
            }
            return _apiResponse;
        }

        [HttpPost("NewVilla", Name = "CreateVilla")]
        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)] // if I'm admin and if this is for custom role it returns this http status code
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] // if I'm not authorized, it means if I'm not logged in and got a jwt token 
        //[HttpPost("NewVilla", Name = "CreateVilla")] // value inside Name parameter is used for url generation, when requesting an endpoint in visual studio your request is sent to the value inside Name parameter, but a string(NewVilla) before Name parameter is just how you see as the name of endpoint in swagger documentation, when you sent request in swagger doumentation or postman you send it using this name        
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateVillaAsync([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                var vll = await _villaRepository.GetAsync(x => x.Name.ToLower() == createDTO.Name.ToLower(), false);
                if (vll != null)
                {
                    ModelState.AddModelError("ErrorMessages", "Name already exists");
                    return BadRequest(ModelState);
                }

                if (createDTO is null)
                {
                    return BadRequest(createDTO);
                }

                var villa = _mapper.Map<Villa>(createDTO);

                await _villaRepository.CreateAsync(villa);
                _apiResponse.Result = _mapper.Map<VillaDTO>(villa);
                _apiResponse.StatusCode = HttpStatusCode.Created;
                //sometimes when resource is created you give them the URL, where the actual resource is created
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
            }
            Response.Headers.Add("Content-Type", "application/json");   
            return _apiResponse;
        }


        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> DeleteVillaAsync(int id)
        {
            try
            {
                if (id == 0)
                {
                    return BadRequest(); //400
                }
                var villa = await _villaRepository.GetAsync(x => x.Id == id);
                if (villa is null)
                {
                    return NotFound(); //404
                }
                await _villaRepository.RemoveAsync(villa);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                return Ok(_apiResponse); //204
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
            }
            return _apiResponse;
        }

        //[Authorize(Roles = "admin")]
        [Authorize(AuthenticationSchemes = "Bearer", Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVillaAsync(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || id != updateDTO.Id) return BadRequest(); //400


                var villa = await _villaRepository.GetAsync(x => x.Id == id, false);
                if (villa is null) { return NotFound(); } //404
                if (!ModelState.IsValid) { return BadRequest(ModelState); } //400

                var updatedVilla = _mapper.Map<Villa>(updateDTO);

                await _villaRepository.UpdateAsync(updatedVilla);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
            }
            return _apiResponse;
        }

        [Authorize(Roles = "admin")]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [HttpPatch("{id:int}", Name = "UpdateVillaPartially")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ApiResponse>> UpdateVillaPartiallyAsync(int id, JsonPatchDocument<VillaUpdateDTO> villaDTO)
        {
            try
            {
                if (id == 0) return BadRequest();

                var villa = await _villaRepository.GetAsync(x => x.Id == id, false); // I use AsNoTracking here because by default ef core tracks the object retrieved from database, as the same time entity that is sent to Update() method is also being tracked, but both of these entities's id is same, and ef core cannot track two or more entity with the same id, as a result it throw exception

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
                await _villaRepository.UpdateAsync(newVilla);
                _apiResponse.Result = newVilla;
                _apiResponse.StatusCode = HttpStatusCode.NoContent;

                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
            }
            return _apiResponse;
        }
    }
}
