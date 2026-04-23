using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.OpenApi;
using Microsoft.OpenApi;

namespace Api.OpenApi;

public static class OpenApiConfiguration
{
    public static void Configure(OpenApiOptions options)
    {
        options.AddDocumentTransformer((document, context, cancellationToken) =>
        {
            document.Components ??= new OpenApiComponents();
            document.Components.SecuritySchemes ??= new Dictionary<string, IOpenApiSecurityScheme>();

            void AddBearerScheme(string name, string description) =>
                document.Components!.SecuritySchemes[name] = new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    Description = description
                };

            AddBearerScheme("BearerAdmin", "Admin JWT token");
            AddBearerScheme("BearerStudent", "Student JWT token");
            AddBearerScheme("BearerStaff", "Staff JWT token");

            document.Security = new[] { "BearerAdmin", "BearerStudent", "BearerStaff" }
                .Select(id => new OpenApiSecurityRequirement
                {
                    [new OpenApiSecuritySchemeReference(id)] = new List<string>()
                })
                .ToList();

            return Task.CompletedTask;
        });

        options.AddOperationTransformer((operation, context, cancellationToken) =>
        {
            if (context.Description.ActionDescriptor.EndpointMetadata.OfType<AllowAnonymousAttribute>().Any())
                operation.Security = null;

            return Task.CompletedTask;
        });
    }
}
