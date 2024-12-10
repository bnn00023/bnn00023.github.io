using AOP.Mediator.Commands;
using AOP.Services;
using MediatR;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AOP.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FooController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IBarService _barService;

        public FooController(IMediator mediator, IBarService barService)
        {
            _mediator = mediator;
            _barService = barService;
        }

        [HttpGet("MediatR")]
        public Task<int> MediatR(string text)
        {
            return _mediator.Send(new FooCommand() { Text = text });
        }

        [HttpGet("BarService")]
        public Task BarService(string text)
        {
            return _barService.DoSomeThing(text);
        }
    }
}
