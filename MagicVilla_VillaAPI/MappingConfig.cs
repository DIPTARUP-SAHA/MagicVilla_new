using AutoMapper;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
namespace MagicVilla_VillaAPI
{
    public class MappingConfig:Profile
    {
        public MappingConfig() { 
            CreateMap<Villa,VillaDTO>();
            CreateMap<VillaDTO, Villa>();
            //instead of writing double mappings we can write Reverse Map
            CreateMap<Villa, VillaCreateDTO>().ReverseMap();
            CreateMap<Villa, VillaUpdateDTO>().ReverseMap();
            
            
            CreateMap<VillaNumber, VillaNumberDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberUpdateDTO>().ReverseMap();
            CreateMap<VillaNumber, VillaNumberCreateDTO>().ReverseMap();
        }  
    }
}
