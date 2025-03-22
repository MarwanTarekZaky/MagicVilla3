using System.Collections;
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
    public VillaAPIController(IVillaRepository db, IMapper mapper)
    {
        _dbVilla = db;
        _mapper = mapper;
    }
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IEnumerable<VillaDTO>>>GetVillas()
    {
        IEnumerable<Villa> villaList = await _dbVilla.GetAllAsync();
        return Ok(_mapper.Map<List<VillaDTO>>(villaList));
    }

    [HttpGet("{id:int}", Name = "GetVilla")]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<VillaDTO>> GetVilla(int id)
    {
        if(id <= 0){
            return BadRequest();
        }

        var villa = await _dbVilla.GetAsync(v => v.Id == id);
        if (villa == null)
        {
            return NotFound();
        }
        return Ok(_mapper.Map<VillaDTO>(villa));
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<VillaDTO>> CreateVilla([FromBody]VillaCreateDTO createDTO)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _dbVilla.GetAsync(v => v.Name.ToLower() == createDTO.Name.ToLower())!=null)
        { 
            ModelState.AddModelError(nameof(createDTO.Name), "Villa already exists");
            return BadRequest(ModelState);
        }
        if (createDTO == null)
        {
            return BadRequest(createDTO);
        }
        Villa model = _mapper.Map<Villa>(createDTO);
        await _dbVilla.CreateAsync(model);
        
        return CreatedAtRoute("GetVilla", new { id = model.Id }, model);
    }


    [HttpDelete("{id:int}", Name = "DeleteVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteVilla(int id)
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
        
        return NoContent();
        
    }

    [HttpPut("{id:int}", Name = "UpdateVilla")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UpdateVilla(int id, [FromBody] VillaUpdateDTO updateDTO)
    {
        if (id <= 0 || updateDTO.Id != id)
        {
            return BadRequest();
        }
        var villa =  await _dbVilla.GetAsync(v => v.Id == id);
        if (villa == null)
        {
            return NotFound();
        }
    
        Villa model = _mapper.Map<Villa>(updateDTO);
        await _dbVilla.UpdateAsync(model);
        return NoContent();
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