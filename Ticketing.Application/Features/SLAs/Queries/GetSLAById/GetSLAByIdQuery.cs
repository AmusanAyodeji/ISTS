using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;

namespace Ticketing.Application.Features.SLAs.Queries.GetSLAById
{
    public record GetSLAByIdQuery(Guid DepartmentId) : IRequest<GetSLAResponseDTO>;
}