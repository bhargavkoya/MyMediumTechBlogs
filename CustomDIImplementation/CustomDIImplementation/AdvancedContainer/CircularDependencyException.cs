namespace CustomDIImplementation.AdvancedContainer
{
    // Exception for circular dependencies
    public class CircularDependencyException : Exception
    {
        public CircularDependencyException(string message) : base(message) { }
    }
}
