using System.Collections;
using System.Net;
using System.Text.Json;
using AutoMapper;
using MagicVilla_VillaAPI.Data;
using MagicVilla_VillaAPI.Models;
using MagicVilla_VillaAPI.Models.Dto;
using MagicVilla_VillaAPI.Repository.IRepository;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace MagicVilla_VillaAPI.Controllers;

[Route("api/VillaAPI")]
[ApiController]
public class VillaAPIController: ControllerBase
{
    private readonly IVillaRepository _dbVilla;
    private readonly IMapper _mapper;
    protected APIResponse _response;
    public VillaAPIController(IVillaRepository db, IMapper mapper)
    {
        _dbVilla = db;
        _mapper = mapper;
        _response = new APIResponse();
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<APIResponse>>GetVillas()
    {
        try
        {
            IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
            _response.Result = _mapper.Map<List<VillaDTO>>(villaList);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { e.Message };
        }
       return _response;
    }

    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> GetVilla(int id)
    {
        try
        {
            if(id <= 0){
                _response.StatusCode = HttpStatusCode.BadRequest;
                return BadRequest(_response);
            }

            var villa = await _dbVilla.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                _response.StatusCode = HttpStatusCode.NotFound;
                return NotFound(_response);
            }

            _response.Result = _mapper.Map<VillaDTO>(villa);
            _response.StatusCode = HttpStatusCode.OK;
            return Ok(_response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { e.Message };
        }
        return _response;
        
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<APIResponse>> CreateVilla([FromBody]VillaCreateDTO createDTO)
    {
        try
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (await _dbVilla.GetAsync(v => v.Name.ToLower() == createDTO.Name.ToLower()) != null)
            {
                ModelState.AddModelError(nameof(createDTO.Name), "Villa already exists");
                return BadRequest(ModelState);
            }

            if (createDTO == null)
            {
                return BadRequest(createDTO);
            }

            Villa villa = _mapper.Map<Villa>(createDTO);
            await _dbVilla.CreateAsync(villa);
            _response.Result = _mapper.Map<VillaDTO>(villa);
            _response.StatusCode = HttpStatusCode.Created;
            return CreatedAtRoute("GetVilla", new { id = villa.Id }, _response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.BadRequest;
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { e.Message };
        }

        return _response;
    }


    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<APIResponse>> DeleteVilla(int id)
    {
        try
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var villa = await _dbVilla.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            await _dbVilla.RemoveAsync(villa);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.NotFound;
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { e.Message };
        }
        return _response;


    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status406NotAcceptable)]
    public async Task<ActionResult<APIResponse>> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
    {
        try
        {
            if (id <= 0 || updateDTO.Id != id)
            {
                return BadRequest();
            }

            var villa = await _dbVilla.GetAsync(v => v.Id == id);
            if (villa == null)
            {
                return NotFound();
            }

            Villa model = _mapper.Map<Villa>(updateDTO);
            await _dbVilla.UpdateAsync(model);
            _response.StatusCode = HttpStatusCode.NoContent;
            _response.IsSuccess = true;
            return Ok(_response);
        }
        catch (Exception e)
        {
            _response.StatusCode = HttpStatusCode.NotAcceptable;
            _response.IsSuccess = false;
            _response.ErrorMessages = new List<string> { e.Message };
        }
        return _response;
    }

    [HttpPatch("{id:int}", Name = "UpdatePartialVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdatePartialVilla(int id, JsonPatchDocument<VillaUpdateDTO> patchDTO)
    {
        if (patchDTO==null || id == 0)
        {
            return BadRequest();
        }
        var villa = await _dbVilla.GetAsync(v => v.Id == id, tracked: false);
        if (villa == null)
        {
            return NotFound();
        }
        VillaUpdateDTO villaDTO = _mapper.Map<VillaUpdateDTO>(villa);
       
        patchDTO.ApplyTo(villaDTO, ModelState);
        Villa model = _mapper.Map<Villa>(villaDTO);
        await _dbVilla.UpdateAsync(model);
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        return NoContent();
        
    }
        
}