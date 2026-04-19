using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Reservas.Application.DTOs;
using Reservas.Application.Features.Habitaciones.Commands;
using Reservas.Application.Features.Habitaciones.Queries;
using System.Threading.Tasks;
using Barcelo.Authorization.Shared;

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
        [HasPermission(Permissions.Habitaciones.Create)]
        public async Task<IActionResult> Create([FromBody] CreateHabitacionDto habitacionDto)
        {
            var result = await _mediator.Send(new CreateHabitacionCommand { Habitacion = habitacionDto });
            return result.IsSuccess ? CreatedAtAction(nameof(GetById), new { id = result.Data }, result.Data) : Conflict(result.ErrorMessage);
        }

        [HttpPut("{id}")]
        [HasPermission(Permissions.Habitaciones.Edit)]
        public async Task<IActionResult> Update(int id, [FromBody] UpdateHabitacionCommand command)
        {
            if (id != command.HabitacionId)
            {
                return BadRequest("El ID de la habitación no coincide con el ID de la solicitud.");
            }

            var result = await _mediator.Send(command);
            if (!result.IsSuccess)
                return result.ErrorMessage!.Contains("no encontrada") ? NotFound(result.ErrorMessage) : Conflict(result.ErrorMessage);
            return Ok(result.Data);
        }

        [HttpDelete("{id}")]
        [HasPermission(Permissions.Habitaciones.Delete)]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _mediator.Send(new DeleteHabitacionCommand { HabitacionId = id });
            return result.IsSuccess ? Ok(result.Data) : NotFound(result.ErrorMessage);
        }

        [Authorize]
        [HttpPost("{habitacionId}/unlock")]
        public async Task<IActionResult> UnlockDoorPersonal(int habitacionId)
        {
            var result = await _mediator.Send(new UnlockDoorPersonalCommand { HabitacionId = habitacionId });

            if (!result.IsSuccess)
                return result.IsNotFound
                    ? NotFound(new { error = result.ErrorMessage })
                    : BadRequest(new { error = result.ErrorMessage });

            return Ok(new { message = result.Data });
        }
    }
}
