using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Domain.Entities;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Common.Mappings;

namespace Ticketing.Application.Features.SLAs.Commands.CreateSLA;

public class CreateSLACommandHandler : IRequestHandler<CreateSLACommand, CreateSLAResponseDTO>
{
    private ISLARepository slarepository;
    private IMapper mapper;

    public CreateSLACommandHandler(ISLARepository _slarepository, IMapper _mapper)
    {
        mapper = _mapper;
        slarepository = _slarepository;
    }

    public async Task<CreateSLAResponseDTO> Handle(CreateSLACommand request, CancellationToken cancellationToken)
    {
        var priorities = request.Request.Priorities;
        if (priorities is null || priorities.Count == 0)
        {
            throw new ArgumentException("At least one priority SLA must be provided.");
        }

        var slaEntities = priorities.Select(priority => new SLA
        {
            DepartmentId = request.Request.DepartmentId,
            Priority = priority.Priority,
            ResponseTimeMinutes = priority.ResponseTimeMinutes,
            ResolutionTimeMinutes = priority.ResolutionTimeMinutes
        }).ToList();

        var existingSlas = await slarepository.GetSLA(
            request.Request.DepartmentId,
            cancellationToken
        );

        foreach (var sla in slaEntities)
        {
            var priority = sla.Priority;
            if (priority == default)
            {
                throw new ArgumentException("Priority must be specified for each SLA entry.");
            }

#pragma warning disable CS8602
            var exists = existingSlas.Any(existing =>
                existing.DepartmentId == sla.DepartmentId &&
                existing.Priority == priority
            );
#pragma warning restore CS8602

            if (exists)
            {
                throw new InvalidOperationException($"SLA already exists for priority: {priority}");
            }
        }

        foreach (var sla in slaEntities)
        {
            await slarepository.AddAsync(sla, cancellationToken);
        }

        await slarepository.SaveChangesAsync(cancellationToken);
        return new CreateSLAResponseDTO
        {
            DepartmentId = request.Request.DepartmentId,
            SLAs = slaEntities.Select(sla => mapper.Map<SLAResponseItemDTO>(sla)).ToList()
        };
    }
}