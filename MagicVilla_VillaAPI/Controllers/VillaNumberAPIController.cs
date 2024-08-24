using AutoMapper;
using Azure;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.DTO;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net;

namespace MagicVilla_VillaAPI.Controllers
{
    [Route("api/VillaNumberAPI")]
    [ApiController]
    public class VillaNumberAPIController : ControllerBase
    {
        private ILogger<VillaNumberAPIController> _logger;

        /* Code for error logging
         public VillaAPIController(ILogger<VillaAPIController> logger)
        {
            _logger = logger;
        }
        */
        protected APIResponse _response;
        private readonly IVillaNumberRepository _dbVillaNumber;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaNumberAPIController(IVillaNumberRepository dbVillaNumber, IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVillaNumber = dbVillaNumber;
            _mapper = mapper;
            _dbVilla = dbVilla;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        public async Task<ActionResult<APIResponse>> GetVillaNumbers() // returns a list
        {
            try
            {
                //_logger.LogInformation("Getting all Villas");
                IEnumerable<VillaNumber> villaNumberList = await _dbVillaNumber.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaNumberDTO>>(villaNumberList);
                _response.StatusCode = System.Net.HttpStatusCode.OK;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;

        }


        [HttpGet("{id:int}", Name = "GetVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] //Not Found
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request
        public async Task<ActionResult<APIResponse>> GetVillaNumber(int id) // returns a single data
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Get Villa Error with Id " + id);
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.OK;
                return Ok(_response);

            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;


        }



        [HttpPost]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status500InternalServerError)] //Internal server error
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request

        public async Task<ActionResult<APIResponse>> CreateVillaNumber([FromBody] VillaNumberCreateDTO createDTO)
        {
            try
            {
                /* When [APIController] annotation is not present then this point is hit
                other wise when [APIController] is present the controller is hit only when the
                model state is valid.*/
                //if(!ModelState.IsValid) return BadRequest(ModelState);

                // Custom Validation
                if (await _dbVillaNumber.GetAsync(u => u.VillaNo == createDTO.VillaNo) != null)
                {

                    ModelState.AddModelError("", "Villa Number Already Exists");
                    return BadRequest(ModelState);
                }

                if (await _dbVilla.GetAsync(u => u.Id == createDTO.VillaID) == null)
                {

                    ModelState.AddModelError("CustomError", "Villa Id is invalid");
                    return BadRequest(ModelState);
                }

                if (createDTO == null) return BadRequest(createDTO);
                //if(createDTO.Id >  0) return StatusCode(StatusCodes.Status500InternalServerError);
                VillaNumber villaNumber = _mapper.Map<VillaNumber>(createDTO);

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
                villaNumber.CreatedDate = DateTime.Now;
                await _dbVillaNumber.CreateAsync(villaNumber);
                _response.Result = _mapper.Map<VillaNumberDTO>(villaNumber);
                _response.StatusCode = HttpStatusCode.Created;


                //EF will automatically add ID to the model after creation i.e why we can access it Created Route
                return CreatedAtRoute("GetVilla", new { id = villaNumber.VillaNo }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<APIResponse>> DeleteVillaNumber(int id)
        {
            try
            {
                if (id == 0) return BadRequest();
                var villaNumber = await _dbVillaNumber.GetAsync(u => u.VillaNo == id);
                if (villaNumber == null) return NotFound();
                await _dbVillaNumber.RemoveAsync(villaNumber);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;
                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;
        }

        [HttpPut("{id:int}", Name = "UpdateVillaNumber")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<APIResponse>> UpdateVillaNumber(int id, [FromBody] VillaNumberUpdateDTO updateDTO)
        {
            try
            {
                if (updateDTO == null || updateDTO.VillaNo != id) return BadRequest();

                if (await _dbVilla.GetAsync(u => u.Id == updateDTO.VillaID) == null)
                {

                    ModelState.AddModelError("CustomError", "Villa Id is invalid");
                    return BadRequest(ModelState);
                }
                /* This changes will automatically be done by Entity Framework
                 var villa = VillaStore.villaList.FirstOrDefault(u => u.Id == id);
                villa.Name = updateDTO.Name;
                villa.Sqft = updateDTO.Sqft;
                villa.Occupancy = updateDTO.Occupancy;*/
                VillaNumber model = _mapper.Map<VillaNumber>(updateDTO);
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
                await _dbVillaNumber.UpdateAsync(model);
                _response.StatusCode = HttpStatusCode.NoContent;
                _response.IsSuccess = true;

                return Ok(_response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;
        }

    }
}
