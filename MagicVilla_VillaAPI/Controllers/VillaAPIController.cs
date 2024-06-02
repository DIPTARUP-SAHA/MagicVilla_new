using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        public IEnumerable<VillaDTO> GetVillas() // returns a list
        {
            return VillaStore.villaList;
            
        }
        [HttpGet("id")]
        public VillaDTO GetVilla(int id) // returns a single data
        {
            return VillaStore.villaList.FirstOrDefault( u =>u.Id==id);
            
        }
    }
}
