using System.Collections.Concurrent;

namespace OurHeritage.Service.Implementations
{
    public static class ResetTokenStorage
    {
        public static ConcurrentDictionary<string, string> Tokens { get; } = new ConcurrentDictionary<string, string>();
    }
}
