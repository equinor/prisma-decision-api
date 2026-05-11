namespace PrismaApi.Test.Fixture;

[CollectionDefinition(nameof(PrismaCollection), DisableParallelization = true)]
public class PrismaCollection : ICollectionFixture<PrismaApiFixture>;
