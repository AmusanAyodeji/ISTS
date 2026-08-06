using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Application.Features.SLAs.Commands.CreateSLA;
using Ticketing.Application.Features.SLAs.Commands.UpdateSLA;
using Ticketing.Application.Features.SLAs.Queries.GetSLAById;
using Ticketing.Domain.Entities;
using Swashbuckle.AspNetCore.Filters;
using Ticketing.Api.SwaggerExamples;

namespace Ticketing.API.Controllers;

public class SLAController : BaseApiController
{
    [HttpPost]
    [Authorize(Policy = "ManagerOrAdmin")]
    [SwaggerRequestExample(typeof(CreateSLARequestDTO), typeof(CreateSLARequestDTOExample))]
    [ProducesResponseType(typeof(ApiResponse<CreateSLAResponseDTO>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateSLARequestDTO request, CancellationToken cancellation)
    {
        var result = await Mediator.Send(new CreateSLACommand(request), cancellation);
        return StatusCode(StatusCodes.Status201Created,ApiResponse<CreateSLAResponseDTO>.Success(result, "SLAS created successfully."));
    }

    [HttpGet("{DepartmentId:guid}")]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<GetSLAResponseDTO>), StatusCodes.Status200OK)]
    public async Task<IActionResult> ReadById(Guid DepartmentId, CancellationToken cancellation)
    {
        var result = await Mediator.Send(new GetSLAByIdQuery(DepartmentId), cancellation);
        return StatusCode(StatusCodes.Status200OK, ApiResponse<GetSLAResponseDTO>.Success(result, "SLA Retrieved successfully."));
    }

    [HttpPatch]
    [Authorize(Policy = "ManagerOrAdmin")]
    [ProducesResponseType(typeof(ApiResponse<UpdateSLAResponseDTO>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<UpdateSLAResponseDTO>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Update([FromBody] UpdateSLARequestDTO request, CancellationToken cancellation)
    {
        var result = await Mediator.Send(new UpdateSLACommand(request), cancellation);
        return StatusCode(StatusCodes.Status200OK, ApiResponse<UpdateSLAResponseDTO>.Success(result, "SLA Updated successfully."));
    }
}