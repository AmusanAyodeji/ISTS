using MediatR;
using Ticketing.Application.DTOs.Users;
using Ticketing.Application.Interfaces.Persistence;

namespace Ticketing.Application.Features.Users.Queries.GetErrors;

public class GetErrorsQueryHandler : IRequestHandler<GetErrorsQuery, List<JobErrorDTO>>
{
    private readonly IJobErrorRepository _jobErrorRepository;

    public GetErrorsQueryHandler(IJobErrorRepository jobErrorRepository)
    {
        _jobErrorRepository = jobErrorRepository;
    }

    public async Task<List<JobErrorDTO>> Handle(GetErrorsQuery request, CancellationToken cancellationToken)
    {
        var joberrors = await _jobErrorRepository.GetErrorsByJobId(request.JobId, cancellationToken);
        var response = new List<JobErrorDTO>();
        foreach(var error in joberrors)
        {
            response.Add(new JobErrorDTO { JobId = request.JobId, DepartmentId = error.DepartmentId, Message = error.Message });
        }
        return response;
    }
}
