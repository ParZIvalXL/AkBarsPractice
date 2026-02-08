using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using PracticalWork.Library.Abstractions.Services;
using PracticalWork.Library.Contracts.v1.Readers;
using PracticalWork.Library.Contracts.v1.Readers.Request;
using PracticalWork.Library.Controllers.Mappers.v1;
using PracticalWork.Library.Models;

namespace PracticalWork.Library.Controllers.Api.v1;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/readers")]
public class ReadersController : ControllerBase
{
    private readonly IReaderService _readerService;
    
    public ReadersController(IReaderService readerService)
    {
        _readerService = readerService;
    }
    
    [HttpPost]
    [Produces("application/json")]
    [ProducesResponseType( 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CreateReader(CreateReaderRequest request)
    {
        var reader = request.ToReader();
        
        var id = await _readerService.CreateReader(reader);
        return Ok(id);
    }
    
    [HttpPost("{id}/extend")]
    [ProducesResponseType( 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> ExtendCard([FromRoute] Guid id, ExtendReaderCardRequest request)
    {
        await _readerService.ExtendCard(id, request.NewExpiryDate);

        return Ok();
    }
    
    [HttpPost("{id}/close")]
    [ProducesResponseType( 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> CloseCard([FromRoute] Guid id)
    {
        await _readerService.CloseCard(id);

        return Ok();
    }
    
    [HttpGet("{id}/Books")]
    [ProducesResponseType( 200)]
    [ProducesResponseType(500)]
    public async Task<IActionResult> GetBooks([FromRoute] Guid id)
    {
        var reader = await _readerService.GetReaderBooks(id);
        return Ok(reader);
    }
}