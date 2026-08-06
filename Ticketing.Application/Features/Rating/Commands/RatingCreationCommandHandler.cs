using AutoMapper;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.Features.Rating.Commands;
using Ticketing.Application.Interfaces.Persistence;
using Ticketing.Application.Interfaces.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

namespace Ticketing.Application.Features.Rating.Commands
{
    public class RatingCreationCommandHandler : IRequestHandler<RatingCreationCommand, bool>
    {
        private ITicketRepository ticketrepository;
        private IRatingRepository ratingrepository;
        private ICurrentUserService currentuserservice;
        private IMapper mapper;

        public RatingCreationCommandHandler(IRatingRepository _ratingrepository, IMapper _mapper, ITicketRepository _ticketrepository, ICurrentUserService _currentuserservice)
        {
            mapper = _mapper;
            ratingrepository = _ratingrepository;
            ticketrepository = _ticketrepository;
            currentuserservice = _currentuserservice;
        }

        public async Task<bool> Handle(RatingCreationCommand request, CancellationToken cancellation)
        {
            var userId = currentuserservice.UserId;
            if (!userId.HasValue)
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            var ticket = await ticketrepository.GetByIdAsync(request.request.TicketId, cancellation);
            if (ticket is null)
            {
                throw new ArgumentException($"No ticket with id: {request.request.TicketId}");
            }
            if (ticket.CreatedById != userId.Value)
            {
                throw new InvalidOperationException("Only the person who opened the ticket can rate it");
            }
            if (ticket.Status != TicketStatus.Resolved && ticket.Status != TicketStatus.Closed)
            {
                throw new InvalidOperationException("Ticket has not been resolved");
            }
            var existingrating = await ratingrepository.GetByTicketId(request.request.TicketId, cancellation);
            if (existingrating is not null)
            {
                throw new ArgumentException($"This Ticket has already been rated");
            }
            var rating = mapper.Map<Ratings>(request.request);
            rating.UserId = userId.Value;
            await ratingrepository.AddAsync(rating, cancellation);
            await ratingrepository.SaveChangesAsync(cancellation);
            return true;
        }
    }
}
