using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Ticketing.Application.DTOs.Rating;

namespace Ticketing.Application.Features.Rating.Commands;

public record RatingCreationCommand(RatingCreationDTO request) : IRequest<bool>;