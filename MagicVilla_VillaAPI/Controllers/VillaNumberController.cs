using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VillaNumberController : ControllerBase
    {
        protected ApiResponse _apiResponse;
        private readonly IVillaNumberRepository _villaNumberRepository;
        private readonly IVillaRepository _villaRepository;
        private readonly IMapper _mapper;

        public VillaNumberController(IVillaNumberRepository villaNumberRepository, IMapper mapper, IVillaRepository villaRepository)
        {
            _apiResponse = new ApiResponse() { ErrorMessages = new List<string>() };
            _villaNumberRepository = villaNumberRepository;
            _mapper = mapper;
            _villaRepository = villaRepository;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<ApiResponse>> GetVillaNumbers()
        {
            try
            {
                IEnumerable<VillaNumber> villaNumbers = await _villaNumberRepository.GetAllAsync();
                _apiResponse.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumbers);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
            }
            return _apiResponse;
        }

        [HttpGet("{villaNo:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> GetVillaNumber(int villaNo)
        {
            try
            {
                _apiResponse.ErrorMessages.Clear();
                if (villaNo == 0)
                {
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.BadRequest.ToString());
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse); //400
                }
                var villaNumber = await _villaNumberRepository.GetAsync(x => x.VillaNo == villaNo, false);
                if (villaNumber is null)
                {
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.NotFound.ToString());
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsSuccess = false;
                    return NotFound(_apiResponse); //404
                }

                _apiResponse.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _apiResponse.StatusCode = HttpStatusCode.OK;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add($"{ex.Message}");
                _apiResponse.IsSuccess = false;

                //return InternalServerError with value 
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);
            }

        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> CreateVillaNumber(VillaNumberCreateDTO villaNumberCreateDTO)
        {
            try
            {
                _apiResponse.ErrorMessages.Clear();

                if (villaNumberCreateDTO.VillaNo == 0)
                {
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.BadRequest.ToString());
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    return BadRequest(_apiResponse);
                }

                //we check if there is such villa number 
                if (await _villaNumberRepository.GetAsync(x => x.VillaNo == villaNumberCreateDTO.VillaNo, false) != null)
                {
                    ModelState.AddModelError("CustomError", "Villa Number already Exists !");
                    return BadRequest(ModelState);
                }

                //we check if there is a villa with such id via villaRepository
                if (await _villaRepository.GetAsync(x => x.Id == villaNumberCreateDTO.VillaID, false) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa ID is invalid !");
                    return BadRequest(ModelState);
                }

                //mapping for repository
                var villa = _mapper.Map<VillaNumber>(villaNumberCreateDTO);

                //adding
                await _villaNumberRepository.CreateAsync(villa);

                //mapping for response
                var villaDTO = _mapper.Map<VillaNumberDTO>(villa);

                //preparing for response
                _apiResponse.Result = villaDTO;
                _apiResponse.StatusCode = HttpStatusCode.Created;

                //sending response                
                //here we send value to GetVillaNumber endpoint, name of value is villaNo in endpoint that we send, it means if we send request to this endpoint at the end, we have to make sure that names are match 
                return CreatedAtRoute("GetVillaNumber", new { villaNo = villa.VillaNo }, _apiResponse);
            }
            catch (Exception ex)
            {
                //adding error message
                _apiResponse.ErrorMessages.Add($"{ex.Message}");

                //initializing false value for IsSuccess
                _apiResponse.IsSuccess = false;

                //
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);
            }
        }

        [HttpDelete("{villaNo:int}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<ApiResponse>> DeleteVillaNumber(int villaNo)
        {
            try
            {
                _apiResponse.ErrorMessages.Clear();
                if (villaNo == 0)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.BadRequest.ToString());
                    return BadRequest(_apiResponse);//400
                }

                var villa = await _villaNumberRepository.GetAsync(x => x.VillaNo == villaNo);
                if (villa is null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.NotFound.ToString());
                    _apiResponse.IsSuccess = false;
                    return NotFound(_apiResponse);//404
                }

                await _villaNumberRepository.RemoveAsync(villa);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.ErrorMessages.Add(ex.Message);
                _apiResponse.IsSuccess = false;
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);
            }
        }

        [HttpPut("{villaNo:int}")]
        public async Task<ActionResult<ApiResponse>> UpdateVillaNumber(int villaNo, VillaNumberUpdateDTO villaNumberUpdateDTO)
        {
            try
            {
                if (villaNo == 0 || villaNumberUpdateDTO.VillaNo != villaNo)
                {
                    _apiResponse.StatusCode = HttpStatusCode.BadRequest;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.BadRequest.ToString());
                    return BadRequest(_apiResponse);
                }

                //we check if there is a villa with such id via villaRepository
                if (await _villaRepository.GetAsync(x => x.Id == villaNumberUpdateDTO.VillaID, false) == null)
                {
                    ModelState.AddModelError("CustomError", "Villa ID is invalid !");
                    return BadRequest(ModelState);
                }

                var villaNumber = await _villaNumberRepository.GetAsync(x => x.VillaNo == villaNo, false);
                if (villaNumber is null)
                {
                    _apiResponse.StatusCode = HttpStatusCode.NotFound;
                    _apiResponse.IsSuccess = false;
                    _apiResponse.ErrorMessages.Add(HttpStatusCode.NotFound.ToString());
                    return NotFound(_apiResponse);
                }

                var updatedVilla = _mapper.Map<VillaNumber>(villaNumberUpdateDTO);
                await _villaNumberRepository.UpdateAsync(updatedVilla);
                _apiResponse.StatusCode = HttpStatusCode.NoContent;
                //we cannot give parameter to NoContent() 
                return Ok(_apiResponse);
            }
            catch (Exception ex)
            {
                _apiResponse.IsSuccess = false;
                _apiResponse.ErrorMessages.Add(ex.Message);
                return StatusCode(StatusCodes.Status500InternalServerError, _apiResponse);
            }
        }

    }
}
