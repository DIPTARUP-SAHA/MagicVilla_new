using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        public ActionResult<IEnumerable<VillaDTO>> GetVillas() // returns a list
        {
            return Ok(VillaStore.villaList);
            
        }
        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] //Not Found
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request

        //[ProducesResponseType(200,Type = typeof(VillaDTO))] // Success
        //[ProducesResponseType(404)] //Not Found
        //[ProducesResponseType(400)] // Bad Request
        public ActionResult<VillaDTO> GetVilla(int id) // returns a single data
        {
            if(id == 0) return BadRequest();

            var villa =VillaStore.villaList.FirstOrDefault( u =>u.Id==id);
            if(villa == null) return NotFound();

            return Ok(villa);
            
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] //Internal server error
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request

        public ActionResult<VillaDTO> CreateVilla([FromBody] VillaDTO villaDTO)
        {
            /* When [APIController] annotation is not present then this point is hit
            other wise when [APIController] is present the controller is hit only when the
            model state is valid.*/
            //if(!ModelState.IsValid) return BadRequest(ModelState);

            // Custom Validation
            if(VillaStore.villaList.FirstOrDefault(u => u.Name.ToLower() == villaDTO.Name.ToLower()) != null)
            {
               
                 ModelState.AddModelError("", "Villa Already Exists");
                return BadRequest(ModelState);
            }
                
            if (villaDTO == null) return BadRequest(villaDTO);
            if(villaDTO.Id >  0) return StatusCode(StatusCodes.Status500InternalServerError);

            villaDTO.Id = VillaStore.villaList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
            VillaStore.villaList.Add(villaDTO);

            //return Ok(villaDTO);

            return CreatedAtRoute("GetVilla", new { id = villaDTO.Id }, villaDTO);
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public IActionResult DeleteVilla(int id)
        {
            if (id == 0) return BadRequest();
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null) return NotFound();
            VillaStore.villaList.Remove(villa);
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public IActionResult UpdateVilla(int id, [FromBody] VillaDTO villaDTO)
        {
            if (villaDTO == null || villaDTO.Id != id) return BadRequest();

            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            villa.Name = villaDTO.Name;
            villa.Sqft = villaDTO.Sqft;
            villa.Occupancy = villaDTO.Occupancy;

            return NoContent();
        }

        // jsonpatch.com for more info
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult UpdatePartialVilla(int id, JsonPatchDocument<VillaDTO> patchDTO)
        { 
            if (patchDTO == null || id ==  0 ) return BadRequest();
            var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            if (villa == null) return BadRequest();
            patchDTO.ApplyTo(villa,ModelState);
            if(!ModelState.IsValid) { return BadRequest(ModelState); }


            return NoContent();
        
        }
    }
}
