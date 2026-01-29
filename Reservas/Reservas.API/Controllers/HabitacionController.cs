using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Habitaciones.Commands;
using Reservas.Application.Features.Habitaciones.Queries;
using System.Threading.Tasks;

namespace Reservas.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class HabitacionController : ControllerBase
    {
        private readonly IMediator _mediator;

        public HabitacionController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _mediator.Send(new GetAllHabitacionesQuery());
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var result = await _mediator.Send(new GetHabitacionByIdQuery { HabitacionId = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        [HttpGet("hotel/{hotelId}")]
        public async Task<IActionResult> GetByHotelId(int hotelId)
        {
            var result = await _mediator.Send(new GetHabitacionesByHotelIdQuery { HotelId = hotelId });
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateHabitacionDto habitacionDto)
        {
            var result = await _mediator.Send(new CreateHabitacionCommand { Habitacion = habitacionDto });
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHabitacionCommand command)
        {
            if (id != command.HabitacionId)
            {
                return BadRequest("El ID de la habitación no coincide con el ID de la solicitud.");
            }

            var result = await _mediator.Send(command);
            return result.IsSuccess ? Ok(result.Data) : BadRequest(result.ErrorMessage);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteHabitacionCommand { HabitacionId = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }
    }
}
