using AutoMapper;
using MagicVilla_Utility;
using MagicVilla_Web.Models;
using MagicVilla_Web.Models.Dto;
using MagicVilla_Web.Services.IServices;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Data;

namespace MagicVilla_Web.Controllers
{
    public class VillaController : Controller
    {
        private readonly IVillaService _villaService;
        private readonly IMapper _mapper;
        public VillaController(IVillaService service, IMapper mapper)
        {
            _villaService = service;
            _mapper = mapper;
        }

        
        public async Task<IActionResult> Index()
        {
            List<VillaDTO> villaDTO = new();
            var response = await _villaService.GetAllAsync<ApiResponse>();
            if (response != null && response.IsSuccess)
            {
                villaDTO = JsonConvert.DeserializeObject<List<VillaDTO>>(Convert.ToString(response.Result));
            }
            return View(villaDTO);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> CreateVilla()
        {
            return View();
        }
        
        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateVilla(VillaCreateDTO villaCreateDTO)
        {
            if (ModelState.IsValid)
            {
                var response = await _villaService.CreateAsync<ApiResponse>(villaCreateDTO);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Villa created successfully !";
                    return RedirectToAction(nameof(Index));
                }
            }
            TempData["error"] = "Error encountered !";
            return View(villaCreateDTO);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateVilla(int villaId)
        {
            var response = await _villaService.GetAsync<ApiResponse>(villaId);
            if (response != null && response.IsSuccess)
            {
                var villaDTO = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
                return View(_mapper.Map<VillaUpdateDTO>(villaDTO));
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateVilla(VillaUpdateDTO villaUpdateDTO)
        {
            if (ModelState.IsValid)
            {
                var response = await _villaService.UpdateAsync<ApiResponse>(villaUpdateDTO);
                if (response != null && response.IsSuccess)
                {
                    TempData["success"] = "Villa updated successfully !";
                    return RedirectToAction(nameof(Index));                    
                }
            }
            TempData["error"] = "Error encountered !";
            return View(villaUpdateDTO);
        }

        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeleteVilla(int villaId)
        {
            var response = await _villaService.GetAsync<ApiResponse>(villaId);
            if (response != null && response.IsSuccess)
            {
                var villaDTO = JsonConvert.DeserializeObject<VillaDTO>(Convert.ToString(response.Result));
                return View(villaDTO);
            }
            return NotFound();
        }

        [Authorize(Roles = "admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteVilla(VillaDTO villaDTO)
        {
            var response = await _villaService.DeleteAsync<ApiResponse>(villaDTO.Id);
            if (response != null && response.IsSuccess)
            {
                TempData["success"] = "Villa deleted successfully !";
                return RedirectToAction(nameof(Index));
            }
            TempData["error"] = "Error encountered !";
            return View(villaDTO);
        }

    }
}
