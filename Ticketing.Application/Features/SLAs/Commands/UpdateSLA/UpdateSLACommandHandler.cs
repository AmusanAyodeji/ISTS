using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Common.Mappings;

namespace Ticketing.Application.Features.SLAs.Commands.UpdateSLA
{
    public class UpdateSLACommandHandler : IRequestHandler<UpdateSLACommand, UpdateSLAResponseDTO>
    {
        private IMapper mapper;
        private ISLARepository slarepository;

        public UpdateSLACommandHandler(IMapper _mapper, ISLARepository _slarepository)
        {
            mapper = _mapper;
            slarepository = _slarepository;
        }

        public async Task<UpdateSLAResponseDTO> Handle(UpdateSLACommand request, CancellationToken cancellation)
        {
            var priorities = request.Request.Priorities;
            if (priorities is null || priorities.Count == 0)
            {
                throw new ArgumentException("At least one priority SLA must be provided.");
            }

            var slas = await slarepository.GetSLA(request.Request.DepartmentId, cancellation);
            if (slas.Count == 0)
            {
                throw new InvalidOperationException("SLAs doesn't exist");
            }

            foreach (var currentpriority in slas)
            {
#pragma warning disable CS8602
                var updated = priorities
                    .FirstOrDefault(priority => priority.Priority == currentpriority.Priority)
                    ?? throw new InvalidOperationException($"Updated SLA data for priority {currentpriority.Priority} was not provided.");

                currentpriority.ResponseTimeMinutes = updated.ResponseTimeMinutes;
                currentpriority.ResolutionTimeMinutes = updated.ResolutionTimeMinutes;
#pragma warning restore CS8602
                currentpriority.UpdatedAt = DateTime.UtcNow;
                slarepository.Update(currentpriority);
            }
            await slarepository.SaveChangesAsync(cancellation);
            return new UpdateSLAResponseDTO
            {
                DepartmentId = request.Request.DepartmentId,
                SLAs = slas.Select(sla => mapper.Map<SLAResponseItemDTO>(sla)).ToList()
            };
        }
    }

}