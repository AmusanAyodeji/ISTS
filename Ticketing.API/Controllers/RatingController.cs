using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Ticketing.API.Common.Models;
using Ticketing.Application.DTOs.Rating;
using Ticketing.Application.Features.Rating.Commands;
using Ticketing.Application.Features.Rating.Queries.GetAverageRating;

namespace Ticketing.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RatingController : BaseApiController
    {
        [HttpPost]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Create(RatingCreationDTO request, CancellationToken cancellation)
        {
            await Mediator.Send(new RatingCreationCommand(request), cancellation);
            return Ok(ApiResponse<object>.Success(new { }, "Ticket has been rated successfully"));
        }

        [HttpGet("average")]
        [Authorize]
        [ProducesResponseType(typeof(ApiResponse<AverageRatingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAverageRating([FromQuery] Guid? agentId, CancellationToken cancellation)
        {
            var result = await Mediator.Send(new GetAverageRatingQuery(agentId), cancellation);
            return Ok(ApiResponse<AverageRatingDto>.Success(result, "Average rating retrieved successfully."));
        }
    }
}