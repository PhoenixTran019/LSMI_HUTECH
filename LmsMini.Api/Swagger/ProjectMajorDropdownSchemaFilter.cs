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

        private readonly LmsDbContext _context;

        public ProjectMajorDropdownSchemaFilter(LmsDbContext context)
        {
            _context = context;
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {

            if(context.Type == typeof(CreateProjectDto) || schema?.Properties == null)
                return;
            
                //Take List MajorId to fill dropdown
                var majorList = _context.Majors
                    .Select(m => m.MajorId)
                    .ToList();

                schema.Properties["ProMajor"].Enum = majorList
                    .Select(m => new OpenApiString(m))
                    .Cast<IOpenApiAny>()
                    .ToList();

                //Description with major name
                schema.Properties["ProMajor"].Description =
                    "Choose Major: " + string.Join(", ", majorList);
            

        }
    }
}
