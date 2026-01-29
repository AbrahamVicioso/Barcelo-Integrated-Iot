using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Hoteles.Commands;
using Reservas.Application.Features.Hoteles.Queries;
using System.Threading.Tasks;

namespace Reservas.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HotelController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HotelController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllHotelesQuery());
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetHotelByIdQuery { HotelId = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHotelDto hotelDto)
        {
            var result = await _mediator.Send(new CreateHotelCommand { Hotel = hotelDto });
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHotelCommand command)
        {
            if (id != command.HotelId)
            {
                return BadRequest("El ID del hotel no coincide con el ID de la solicitud.");
            }

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteHotelCommand { HotelId = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }
    }
}
