using System;
using MediatR;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;

namespace Ticketing.Application.Features.SLAs.Commands.UpdateSLA
{
    public record UpdateSLACommand(UpdateSLARequestDTO Request) : IRequest<UpdateSLAResponseDTO>;
}