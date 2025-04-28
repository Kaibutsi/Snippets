public static class YarpExtensions
{
    public static void AddYarp(this WebApplicationBuilder builder, Dictionary<string, Rule> rules)
    {
        var index = 0;
        var routes = rules.Select(pair =>
        {
            var (key, value) = pair;
            var result = new RouteConfig
            {
                ClusterId = $"cluster-{index}",
                RouteId   = $"id-{index++}",
                Match     = new() { Path = key }
            };

            if (value.Remove != null)
                result = result.WithTransformPathRemovePrefix(value.Remove);
            
            return result;
        }).ToArray();

        index = 0;
        var configs = rules.Values.Select(value => new ClusterConfig
        {
            ClusterId = $"cluster-{index}",
            Destinations = new Dictionary<string, DestinationConfig>(StringComparer.OrdinalIgnoreCase)
            {
                [$"id-{index++}"] = new() { Address = value }
            }
        }).ToArray();

        builder.Services.AddReverseProxy().LoadFromMemory(routes, configs);
    }

    public static void UseYarp(this WebApplication app, Func<HttpContext, Func<Task>, Task> callback)
    {
        app.MapReverseProxy(pipeline => pipeline.Use(callback));
    }

    public static void UseYarp(this WebApplication app, Func<HttpRequest, bool> predicate)
    {
        app.UseYarp(async (context, next) =>
        {
            if (predicate(context.Request))
                await next();
        });
    }

    public record Rule(string Path)
    {
        public string Remove;

        public static implicit operator string(Rule rule) => rule.Path;
        public static implicit operator Rule(string path) => new(path);
    }
}