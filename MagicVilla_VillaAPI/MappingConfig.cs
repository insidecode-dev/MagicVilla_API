using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;

namespace MagicVilla_VillaAPI
{
    public class MappingConfig:Profile
    {
        public MappingConfig()
        {

            // ReverseMap() function ensures that mapping can be done from both side
            CreateMap<Villa, VillaDTO>().ReverseMap(); 
            CreateMap<Villa, VillaCreateDTO>().ReverseMap();
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
            //
            CreateMap<VillaNumber, VillaNumberDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();

            // after identity

            //mapping for ApplicationUser and UserDTO classes 
            CreateMap<ApplicationUser,UserDTO>().ReverseMap();   
        }
    }
}
