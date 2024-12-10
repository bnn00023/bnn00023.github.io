using MediatR;

namespace AOP.Mediator.Behaviors
{
    public class ValidateBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse> where TRequest : notnull
    {
        private readonly IValidator<TRequest> _validator;

        public ValidateBehavior(IValidator<TRequest> validator)
        {
            _validator = validator;
        }

        public Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
        {
            var exception = _validator.Validate(request);
            if (exception is not null)
                throw exception;

            return next();
        }
    }


}
