using MediatR;

namespace AOP.Mediator.Commands
{
    public class FooCommand : IRequest<int>
    {
        public string Text { get; set; } = string.Empty;
    }
}
