using MediatR;
using Reservas.Application.Common;
using Reservas.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reservas.Application.Features.Hoteles.Queries
{
    public class GetAllHotelesQuery : IRequest<Result<IEnumerable<HotelesDto>>>
    {
    }
}
