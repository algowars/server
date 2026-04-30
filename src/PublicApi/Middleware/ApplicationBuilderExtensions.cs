namespace PublicApi.Middleware;

public static class ApplicationBuilderExtensions
{
    public static IApplicationBuilder UseSecurityHeaders(this IApplicationBuilder app)
    {
        app.UseXContentTypeOptions();
        app.UseReferrerPolicy(opts => opts.NoReferrer());
        app.UseXXssProtection(options => options.EnabledWithBlockMode());
        app.UseXfo(options => options.Deny());
        app.UseCsp(options =>
            options.DefaultSources(s => s.Self()).StyleSources(s => s.Self().UnsafeInline())
        );

        return app;
    }
}