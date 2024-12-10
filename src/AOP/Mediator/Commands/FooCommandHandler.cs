using MediatR;

namespace AOP.Mediator.Commands
{
    public class FooCommandHandler : IRequestHandler<FooCommand, int>
    {
        public Task<int> Handle(FooCommand request, CancellationToken cancellationToken)
        {
            return decimal.TryParse(request.Text, out var result) ? Task.FromResult((int)result) : Task.FromResult(0);
        }
    }
}
