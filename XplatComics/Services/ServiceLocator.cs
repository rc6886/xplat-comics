using System;

namespace XplatComics.Services;

public static class ServiceLocator
{
    public static IServiceProvider Services { get; set; } = null!;
}
