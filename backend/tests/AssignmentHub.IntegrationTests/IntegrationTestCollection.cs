namespace AssignmentHub.IntegrationTests;

/// <summary>
/// Ties every integration test class to a single shared <see cref="CustomWebApplicationFactory"/>
/// instance (xUnit constructs it once per collection, disposes it once after the whole collection
/// finishes) so the SQLite schema is created and seeded exactly once rather than per test class.
/// This class has no code — it only exists to carry the [CollectionDefinition] + ICollectionFixture
/// pairing that xUnit's convention-based discovery looks for.
/// </summary>
[CollectionDefinition("Integration")]
public class IntegrationTestCollection : ICollectionFixture<CustomWebApplicationFactory>
{
}
