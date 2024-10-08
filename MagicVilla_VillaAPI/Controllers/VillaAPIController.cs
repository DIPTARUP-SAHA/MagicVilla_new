﻿using AutoMapper;
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
        protected APIResponse _response;
        private readonly IVillaRepository _dbVilla;
        private readonly IMapper _mapper;

        public VillaAPIController(IVillaRepository dbVilla, IMapper mapper)
        {
            _dbVilla = dbVilla;
            _mapper = mapper;
            this._response = new();
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        public async Task<ActionResult<APIResponse>> GetVillas() // returns a list
        {
            try
            {
                //_logger.LogInformation("Getting all Villas");
                IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
                _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
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


        [HttpGet("{id:int}", Name = "GetVilla")]
        [ProducesResponseType(StatusCodes.Status200OK)] // Success
        [ProducesResponseType(StatusCodes.Status404NotFound)] //Not Found
        [ProducesResponseType(StatusCodes.Status400BadRequest)] // Bad Request
        public async Task<ActionResult<APIResponse>> GetVilla(int id) // returns a single data
        {
            try
            {
                if (id == 0)
                {
                    _logger.LogError("Get Villa Error with Id " + id);
                    _response.StatusCode = HttpStatusCode.BadRequest;
                    return BadRequest(_response);
                }

                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null)
                {
                    _response.StatusCode = HttpStatusCode.NotFound;
                    return NotFound(_response);
                }

                _response.Result = _mapper.Map<VillaDTO>(villa);
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

        public async Task<ActionResult<APIResponse>> CreateVilla([FromBody] VillaCreateDTO createDTO)
        {
            try
            {
                /* When [APIController] annotation is not present then this point is hit
                other wise when [APIController] is present the controller is hit only when the
                model state is valid.*/
                //if(!ModelState.IsValid) return BadRequest(ModelState);

                // Custom Validation
                if (await _dbVilla.GetAsync(u => u.Name.ToLower() == createDTO.Name.ToLower()) != null)
                {

                    ModelState.AddModelError("", "Villa Already Exists");
                    return BadRequest(ModelState);
                }

                if (createDTO == null) return BadRequest(createDTO);
                //if(createDTO.Id >  0) return StatusCode(StatusCodes.Status500InternalServerError);
                Villa villa = _mapper.Map<Villa>(createDTO);

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
                villa.CreatedDate = DateTime.Now;
                await _dbVilla.CreateAsync(villa);
                _response.Result = _mapper.Map<VillaDTO>(villa);
                _response.StatusCode = HttpStatusCode.Created;


                //EF will automatically add ID to the model after creation i.e why we can access it Created Route
                return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
            }
            catch (Exception ex)
            {
                _response.IsSuccess = false;
                _response.ErrorMessages = new List<String>() { ex.ToString() };
            }
            return _response;
        }

        [HttpDelete("{id:int}", Name = "DeleteVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]

        public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
        {
            try
            {
                if (id == 0) return BadRequest();
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                if (villa == null) return NotFound();
                await _dbVilla.RemoveAsync(villa);
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

        [HttpPut("{id:int}", Name = "UpdateVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]

        public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
        {
            try
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
                await _dbVilla.UpdateAsync(model);
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

        // jsonpatch.com for more info
        [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<APIResponse>> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
        {
            try
            {
                if (patchDTO == null || id == 0) return BadRequest();
                var villa = await _dbVilla.GetAsync(u => u.Id == id);
                VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);


                if (villa == null) return BadRequest();
                patchDTO.ApplyTo(villaDTO, ModelState);
                Villa model = _mapper.Map<Villa>(villaDTO);

                await _dbVilla.UpdateAsync(model);
                // await _db.SaveChangesAsync();

                if (!ModelState.IsValid) { return BadRequest(ModelState); }


                return NoContent();
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
