using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Domain.Enums;
using Ticketing.Application.Common.Mappings;
using Ticketing.Domain.Entities;

namespace Ticketing.Application.Features.SLAs.Queries.GetSLAById
{
    public class GetSLAByIdQueryHandler : IRequestHandler<GetSLAByIdQuery, GetSLAResponseDTO>
    {
        private ISLARepository slarepository;
        private IDepartmentRepository departmentrepository;
        private IMapper mapper;

        public GetSLAByIdQueryHandler(ISLARepository _slarepository, IMapper _mapper, IDepartmentRepository department)
        {
            slarepository = _slarepository;
            mapper = _mapper;
            departmentrepository = department;
        }

        public async Task<GetSLAResponseDTO> Handle(GetSLAByIdQuery request, CancellationToken cancellation)
        {
            var department = await departmentrepository.GetByIdAsync(request.DepartmentId, cancellation);
            if (department is null)
            {
                throw new InvalidOperationException($"Department with the id {request.DepartmentId} does not exist");
            }
            var sla = await slarepository.GetSLA(request.DepartmentId, cancellation);
            return new GetSLAResponseDTO
            {
                DepartmentId = request.DepartmentId,
                SLAs = sla.Select(sla => mapper.Map<SLAResponseItemDTO>(sla)).ToList()
            };
        }
    }
}