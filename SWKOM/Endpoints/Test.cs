namespace SWKOM.Endpoints;

public static class Test
{
    public static IEndpointRouteBuilder MapTest(this IEndpointRouteBuilder builder)
    {
        builder.MapGet("/", () => new { data = "Hardcoded data!" });

        return builder;
    }
}
