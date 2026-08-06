using MediatR;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Rating.Queries.GetAverageRating;

public class GetAverageRatingQueryHandler : IRequestHandler<GetAverageRatingQuery, AverageRatingDto>
{
    private readonly IRatingRepository _ratingRepository;

    public GetAverageRatingQueryHandler(IRatingRepository ratingRepository)
    {
        _ratingRepository = ratingRepository;
    }

    public async Task<AverageRatingDto> Handle(GetAverageRatingQuery request, CancellationToken cancellationToken)
    {
        var average = request.AgentId.HasValue
            ? await _ratingRepository.GetAverageRatingForAgentAsync(request.AgentId.Value, cancellationToken)
            : await _ratingRepository.GetAverageRatingAsync(cancellationToken);

        var totalRatings = request.AgentId.HasValue
            ? await _ratingRepository.GetRatingCountForAgentAsync(request.AgentId.Value, cancellationToken)
            : await _ratingRepository.GetRatingCountAsync(cancellationToken);

        return new AverageRatingDto
        {
            AverageRating = average ?? 0,
            TotalRatings = totalRatings
        };
    }
}
