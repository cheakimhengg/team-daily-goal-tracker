namespace backend.Exceptions;

public class GoalNotFoundException : Exception
{
    public GoalNotFoundException(string message) : base(message)
    {
    }
}
