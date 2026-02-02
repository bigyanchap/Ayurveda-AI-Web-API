using Microsoft.Extensions.Options;
using Supabase;

namespace Ayurveda_AI_Backend.Infrastructure.Supabase;

public interface ISupabaseClientProvider
{
    Client Client { get; }
}

public class SupabaseClientProvider : ISupabaseClientProvider
{
    public SupabaseClientProvider(IOptions<SupabaseOptions> options)
    {
        var config = options.Value;
        Client = new Client(config.Url, config.ApiKey);
    }

    public Client Client { get; }
}
