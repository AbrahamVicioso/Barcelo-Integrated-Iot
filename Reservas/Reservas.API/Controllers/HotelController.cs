using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Hoteles.Queries;
using System.Threading.Tasks;

namespace Reservas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HotelController(IMediator mediator)
        {
            this._mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> getAll()
        {
            var result = await _mediator.Send(new GetAllHotelesQuery());
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }
    }
}
