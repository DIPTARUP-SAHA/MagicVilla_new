using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaAPI")]
    [ApiController]
    public class VillaAPIController : ControllerBase
    {
        private ILogger<VillaAPIController> _logger;

        /* Code for error logging
         public VillaAPIController(ILogger<VillaAPIController> logger)
        {
            _logger = logger;
        }
        */

        private readonly ApplicationDbContext _db;
        private readonly IMapper _mapper;

        public VillaAPIController(ApplicationDbContext db, IMapper mapper)
        {
            _db = db;
            _mapper = mapper;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        public async Task<ActionResult<IEnumerable<VillaDTO>>> GetVillas() // returns a list
        {
            //_logger.LogInformation("Getting all Villas");
            IEnumerable<Villa> villaList =  await _db.Villas.ToListAsync();

            return Ok(_mapper.Map<List<VillaDTO>>(villaList));
            
        }
        [HttpGet("{id:int}",Name ="GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] //Not Found
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request

        //[ProducesResponseType(200,Type = typeof(VillaDTO))] // Success
        //[ProducesResponseType(404)] //Not Found
        //[ProducesResponseType(400)] // Bad Request
        public async Task<ActionResult<VillaDTO>> GetVilla(int id) // returns a single data
        {
            if (id == 0)
            {
                _logger.LogError("Get Villa Error with Id " + id);
                return BadRequest();
            }

            var villa =await _db.Villas.FirstOrDefaultAsync( u =>u.Id==id);
            if(villa == null) return NotFound();

            return Ok(_mapper.Map<VillaDTO>(villa));
            
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] //Internal server error
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request

        public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            /* When [APIController] annotation is not present then this point is hit
            other wise when [APIController] is present the controller is hit only when the
            model state is valid.*/
            //if(!ModelState.IsValid) return BadRequest(ModelState);

            // Custom Validation
            if(await _db.Villas.FirstOrDefaultAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
               
                 ModelState.AddModelError("", "Villa Already Exists");
                return BadRequest(ModelState);
            }
                
            if (createDTO == null) return BadRequest(createDTO);
            //if(createDTO.Id >  0) return StatusCode(StatusCodes.Status500InternalServerError);
            Villa model = _mapper.Map<Villa>(createDTO);

            /*Villa model = new()
            {
                Amenity = createDTO.Amenity,
                Details = createDTO.Details,
                
                ImageUrl = createDTO.ImageUrl,
                Name = createDTO.Name,
                Occupancy = createDTO.Occupancy,
                Rate = createDTO.Rate,
                Sqft = createDTO.Sqft
            };*/
            await _db.Villas.AddAsync(model);
            await _db.SaveChangesAsync();
            //EF will automatically add ID to the model after creation i.e why we can access it Created Route
            return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
        }
            
        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<IActionResult> DeleteVilla(int id)
        {
            if (id == 0) return BadRequest();
            var villa =await  _db.Villas.FirstOrDefaultAsync(u => u.Id == id);
            if (villa == null) return NotFound();
            _db.Villas.Remove(villa);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            if (updateDTO == null || updateDTO.Id != id) return BadRequest();

            /* This changes will automatically be done by Entity Framework
             var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
            villa.Name = updateDTO.Name;
            villa.Sqft = updateDTO.Sqft;
            villa.Occupancy = updateDTO.Occupancy;*/
            Villa model = _mapper.Map<Villa>(updateDTO);
            /*Villa model = new()
            {
                Amenity = updateDTO.Amenity,
                Details = updateDTO.Details,
                Id = updateDTO.Id,
                ImageUrl = updateDTO.ImageUrl,
                Name = updateDTO.Name,
                Occupancy = updateDTO.Occupancy,
                Rate = updateDTO.Rate,
                Sqft = updateDTO.Sqft
            };*/
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        // jsonpatch.com for more info
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async  Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        { 
            if (patchDTO == null || id ==  0 ) return BadRequest();
            var villa = await _db.Villas.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);

            
            if (villa == null) return BadRequest();
            patchDTO.ApplyTo(villaDTO,ModelState);
            Villa model = _mapper.Map<Villa>(villaDTO);
            
            _db.Villas.Update(model);
            await _db.SaveChangesAsync();
            
            if (!ModelState.IsValid) { return BadRequest(ModelState); }


            return NoContent();
        
        }
    }
}
