/// <summary>
/// Represents an element within a repository, identified by its location and ID, and containing optional content.
/// </summary>
/// <param name="RepositoryLocation">The location of the repository where this element is stored.</param>
/// <param name="Id">A unique identifier for the repository element.</param>
/// <param name="Content">The content associated with the repository element, if any.</param>
public record RepositoryElement(RepositoryLocation RepositoryLocation, string Id, RepositoryElementContent? Content);