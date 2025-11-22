using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.InterfaceS
{
    public interface ISchemaStudentFilter
    {
        void Apply(OpenApiSchema schema, SchemaFilterContext context);
    }
}
