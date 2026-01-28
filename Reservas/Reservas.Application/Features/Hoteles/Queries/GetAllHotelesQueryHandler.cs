using AutoMapper;
using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using Reservas.Domain.Entities;
using Reservas.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Application.Features.Hoteles.Queries
{
    public class GetAllHotelesQueryHandler : IRequestHandler<GetAllHotelesQuery,Result<IEnumerable<HotelesDto>>>
    {
        private readonly IHotelRepository _hotelRepository;
        private readonly IMapper _mapper;

        public GetAllHotelesQueryHandler(IHotelRepository hotelRepository, IMapper mapper)
        {
            this._hotelRepository = hotelRepository;
            this._mapper = mapper;
        }

        public async Task<Result<IEnumerable<HotelesDto>>> Handle(GetAllHotelesQuery request, CancellationToken cancellationToken)
        {
            var hoteles = await _hotelRepository.getAll();
            var mappeds = hoteles.Select(hotel => _mapper.Map<HotelesDto>(hotel));
            return Result<IEnumerable<HotelesDto>>.Success(mappeds);
        }
    }
}
