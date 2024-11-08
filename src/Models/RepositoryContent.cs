namespace Models;

public record RepositoryContent
{
    public List<Command?>? Commands { get; set; }

    public List<Variable?>? Variables { get; set; }
}
