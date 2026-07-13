using System.Threading.Tasks;

namespace Deer_o_matic.Services;

public interface IInitialisable
{
    public Task Initialise();
}

public interface IApplicationInitialiser
{
    public Task InitialiseAll();
}

public class ApplicationInitialiser : IApplicationInitialiser
{
    private readonly IInitialisable[] toInitialise;

    public ApplicationInitialiser(IPoisonAreaManagerService poisonAreaManager, IDocPoisonAreaRetrievalService poisonAreaRetrieval)
    {
        toInitialise = [
            poisonAreaManager,
            poisonAreaRetrieval
        ];
    }

    public async Task InitialiseAll()
    {
        foreach (IInitialisable initialisable in toInitialise)
        {
            await initialisable.Initialise();
        }
    }
}

