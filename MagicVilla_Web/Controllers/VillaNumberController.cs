using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Models.ViewModel;
using MagicVilla_Web.Services;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Data;

namespace MagicVilla_Web.Controllers
{
    public class VillaNumberController : Controller
    {
        private readonly IVillaNumberService _villaNumberService;
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;

        public VillaNumberController(IMapper mapper, IVillaNumberService villaNumberService, IVillaService villaService)
        {
            _mapper = mapper;
            _villaNumberService = villaNumberService;
            _villaService = villaService;
        }

        
        public async Task<IActionResult> IndexVillaNumber()
        {
            List<VillaNumberDTO> villaNumberDTO = null;
            var result = await _villaNumberService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (result.IsSuccess || result.Result != null)
            {
                villaNumberDTO = JsonConvert.DeserializeObject<List<VillaNumberDTO>>(Convert.ToString(result.Result));
            }
            return View(villaNumberDTO);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateVillaNumber()
        {
            VillaNumberCreateVM villaNumberCreateVM = new();
            var response = await _villaService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (response.Result != null && response.IsSuccess)
            {
                //this populates our dropdown's data
                villaNumberCreateVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result)).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
            }
            return View(villaNumberCreateVM);
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVillaNumber(VillaNumberCreateVM villaNumberCreateVM)
        {
            if (ModelState.IsValid)
            {
                var response = await _villaNumberService.CreateAsync<ApiResponse>(villaNumberCreateVM.VillaNumberCreateDTO, HttpContext.Session.GetString(StaticDetails.SessionToken));
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "VillaNumber created successfully !";
                    return RedirectToAction(nameof(IndexVillaNumber));
                }
                else if (response.ErrorMessages.Count > 0)
                {
                    ModelState.AddModelError("ErrorMessages", response.ErrorMessages.FirstOrDefault());
                }
            }

            // if we get error message in post method we need return model back to view again but also we should initialize IEnumerable<SlectListItem> VillaList with villas again, here we do it     
            var villas = await _villaService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (villas.Result != null && villas.IsSuccess)
            {
                // this populates our dropdown's data
                villaNumberCreateVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(villas.Result)).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
            }
            TempData["success"] = "Error encountered";
            return View(villaNumberCreateVM);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateVillaNumber(int villaNo /* I named method parameter as villaNo because the value of this comes from IndexVillaNumber action method and in its view I have named asp-route-villaNo as incoming parameter for UpdateVillaNumber */)
        {
            VillaNumberUpdateVM villaNumberUpdateVM = new();
            var response = await _villaNumberService.GetAsync<ApiResponse>(villaNo, HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (response.Result != null && response.IsSuccess)
            {
                VillaNumberDTO villaNumberUpdateDTO = JsonConvert.DeserializeObject<VillaNumberDTO>(Convert.ToString(response.Result));
                villaNumberUpdateVM.VillaNumberUpdateDTO = _mapper.Map<VillaNumberUpdateDTO>(villaNumberUpdateDTO);
            }

            var villas = await _villaService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (villas.Result != null && villas.IsSuccess)
            {
                // this populates our dropdown's data
                villaNumberUpdateVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(villas.Result)).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
                return View(villaNumberUpdateVM);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVillaNumber(VillaNumberUpdateVM villaNumberUpdateVM)
        {
            var response = await _villaNumberService.UpdateAsync<ApiResponse>(villaNumberUpdateVM.VillaNumberUpdateDTO, HttpContext.Session.GetString(StaticDetails.SessionToken));
            // I don't check response.Result because in update method api returns us NoContent http status, it means Result will be empty 
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "VillaNumber updated successfully !";
                return RedirectToAction(nameof(IndexVillaNumber));
            }
            else if (response.ErrorMessages.Count > 0)
            {
                ModelState.AddModelError("ErrorMessages", response.ErrorMessages.FirstOrDefault());
            }

            var villas = await _villaService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (villas.Result != null && villas.IsSuccess)
            {
                // this populates our dropdown's data
                villaNumberUpdateVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(villas.Result)).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
            }
            TempData["success"] = "Error encountered";
            return View(villaNumberUpdateVM);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteVillaNumber(int villaNo)
        {
            VillaNumberDeleteVM villaNumberDeleteVM = new();
            var response = await _villaNumberService.GetAsync<ApiResponse>(villaNo, HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (response.Result != null && response.IsSuccess)
            {
                VillaNumberDTO villaNumberDTO = JsonConvert.DeserializeObject<VillaNumberDTO>(Convert.ToString(response.Result));
                villaNumberDeleteVM.VillaNumberDTO = villaNumberDTO;
            }

            var villas = await _villaService.GetAllAsync<ApiResponse>(HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (villas.Result != null && villas.IsSuccess)
            {
                // this populates our dropdown's data
                villaNumberDeleteVM.VillaList = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(villas.Result)).Select(i => new SelectListItem
                {
                    Text = i.Name,
                    Value = i.Id.ToString(),
                });
                return View(villaNumberDeleteVM);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVillaNumber(VillaNumberDeleteVM villaNumberDeleteVM)
        {            
            var response = await _villaNumberService.DeleteAsync<ApiResponse>(villaNumberDeleteVM.VillaNumberDTO.VillaNo, HttpContext.Session.GetString(StaticDetails.SessionToken));
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "VillaNumber deleted successfully !";
                return RedirectToAction(nameof(IndexVillaNumber));
            }
            else if (response.ErrorMessages.Count > 0)
            {
                ModelState.AddModelError("ErrorMessages", response.ErrorMessages.FirstOrDefault());
            }
            TempData["success"] = "Error encountered";
            return View(villaNumberDeleteVM);
        }
    }
}
