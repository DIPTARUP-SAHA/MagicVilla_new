using MagicVilla_VillaAPI.Models.DTO;

namespace MagicVilla_VillaAPI.Data
{
    public static class VillaStore
    {
        public static List<VillaDTO> villaList = new List<VillaDTO>{
                new VillaDTO{Id = 1, Name ="pool view" ,Sqft = 100 , Occupancy = 4},
                new VillaDTO{Id = 2, Name ="Beach view",Sqft = 300 , Occupancy = 3}
            };

    }
}
