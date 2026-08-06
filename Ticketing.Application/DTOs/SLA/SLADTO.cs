using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs.SLA
{
    public class CreateSLAPriority{
        public TicketPriority Priority { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public int ResolutionTimeMinutes { get; set; }
    }
    public class CreateSLARequestDTO
    {
        public Guid DepartmentId { get; set; }
        public List<CreateSLAPriority> Priorities { get; set; } = new();
    }
    public class SLAResponseItemDTO
    {
        public Guid Id { get; set; }
        public Guid DepartmentId { get; set; }
        public TicketPriority Priority { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public int ResolutionTimeMinutes { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
    public class CreateSLAResponseDTO
    {
        public Guid DepartmentId { get; set; }
        public List<SLAResponseItemDTO> SLAs { get; set; } = new();
    }
    public class UpdateSLAPriorityDTO
    {
        public TicketPriority Priority { get; set; }
        public int ResponseTimeMinutes { get; set; }
        public int ResolutionTimeMinutes { get; set; }
    }
    public class UpdateSLARequestDTO
    {
        public Guid DepartmentId { get; set; }
        public List<UpdateSLAPriorityDTO> Priorities { get; set; } = new();
    }
    public class UpdateSLAResponseDTO: CreateSLAResponseDTO
    {
    }
    public class GetSLAResponseDTO : CreateSLAResponseDTO
    {
    }
}