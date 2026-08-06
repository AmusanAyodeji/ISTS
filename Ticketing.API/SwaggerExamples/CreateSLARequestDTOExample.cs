using Swashbuckle.AspNetCore.Filters;
using Ticketing.Application.DTOs.SLA;
using Ticketing.Domain.Enums;

namespace Ticketing.Api.SwaggerExamples
{
    public class CreateSLARequestDTOExample : IExamplesProvider<CreateSLARequestDTO>
    {
        public CreateSLARequestDTO GetExamples()
        {
            return new CreateSLARequestDTO
            {
                DepartmentId = Guid.Parse("3fa85f64-5717-4562-b3fc-2c963f66afa6"),
                Priorities = new List<CreateSLAPriority>
                {
                    new CreateSLAPriority
                    {
                        Priority = TicketPriority.Low,
                        ResponseTimeMinutes = 60,
                        ResolutionTimeMinutes = 1440
                    },
                    new CreateSLAPriority
                    {
                        Priority = TicketPriority.Medium,
                        ResponseTimeMinutes = 30,
                        ResolutionTimeMinutes = 720
                    },
                    new CreateSLAPriority
                    {
                        Priority = TicketPriority.High,
                        ResponseTimeMinutes = 15,
                        ResolutionTimeMinutes = 240
                    },
                    new CreateSLAPriority
                    {
                        Priority = TicketPriority.Urgent,
                        ResponseTimeMinutes = 5,
                        ResolutionTimeMinutes = 60
                    }
                }
            };
        }
    }
}