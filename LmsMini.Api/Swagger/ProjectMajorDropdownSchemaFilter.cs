using LmsMini.Api.InterfaceS;
using LmsMini.Application.DTOs.Project;
using LmsMini.Domain.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.Swagger
{
    public class ProjectMajorDropdownSchemaFilter : ISchemaFilter
    {

        private readonly IServiceScopeFactory _scopeFactory;

        public ProjectMajorDropdownSchemaFilter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            if(context.Type != typeof(CreateProjectDto) || schema?.Properties == null)
                return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

            var majorNames = db.Majors
            .Select(m => m.MajorName) // CHỈ LẤY NAME
            .ToList();

            if (schema.Properties.TryGetValue("ProMajor", out var prop))
            {
                prop.Enum = majorNames
                    .Select(n => (IOpenApiAny)new OpenApiString(n))
                    .ToList();
            }

        }
    }
}
