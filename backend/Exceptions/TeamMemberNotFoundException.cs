namespace backend.Exceptions;

public class TeamMemberNotFoundException : Exception
{
    public TeamMemberNotFoundException(string message) : base(message)
    {
    }
}
