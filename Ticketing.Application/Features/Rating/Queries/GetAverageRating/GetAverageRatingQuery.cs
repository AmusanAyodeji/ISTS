using MediatR;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Rating.Queries.GetAverageRating;

public record GetAverageRatingQuery(Guid? AgentId = null) : IRequest<AverageRatingDto>;

public class AverageRatingDto
{
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
}
