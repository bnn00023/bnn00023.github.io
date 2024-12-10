
namespace AOP.Mediator.Commands
{
    public class FooCommandValidator : IValidator<FooCommand>
    {
        public Exception? Validate(FooCommand value)
        {
            if (string.IsNullOrWhiteSpace(value.Text))
                return new ArgumentException("Text is required");
            return null;
        }
    }
}
