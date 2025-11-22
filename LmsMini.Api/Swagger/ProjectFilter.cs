using LmsMini.Application.DTOs.ProjectClassroom;
using LmsMini.Domain.Models;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.Swagger
{
    public class ProjectFilter : ISchemaFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public ProjectFilter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }


        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(CreateProjectClassroomDto) || schema?.Properties == null)
                return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

            var projectName = db.Projects
                .Select(p => p.Title)
                .ToList();

            if (schema.Properties.TryGetValue("ProjectID", out var prop))
            {
                prop.Enum = projectName
                    .Select(p => (IOpenApiAny)new OpenApiString(n))
                    .ToList();
            }

        }

    }
}
