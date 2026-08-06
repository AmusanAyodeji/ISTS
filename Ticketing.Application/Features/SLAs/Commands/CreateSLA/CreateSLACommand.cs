using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;

namespace Ticketing.Application.Features.SLAs.Commands.CreateSLA;

public record CreateSLACommand(CreateSLARequestDTO Request) : IRequest<CreateSLAResponseDTO>;