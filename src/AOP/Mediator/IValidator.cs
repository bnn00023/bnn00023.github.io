namespace AOP.Mediator
{
    // <summary>
    /// Interface for validating objects of type <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type of object to validate.</typeparam>
    public interface IValidator<T>
    {
        /// <summary>
        /// Validates the specified value.
        /// </summary>
        /// <param name="value">The value to validate.</param>
        /// <returns>An <see cref="Exception"/> if validation fails; otherwise, <c>null</c>.</returns>
        public Exception? Validate(T value);
    }
}
