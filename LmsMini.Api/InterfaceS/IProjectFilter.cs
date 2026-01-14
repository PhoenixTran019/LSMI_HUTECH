using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.InterfaceS
{
    public interface IProjectFilter
    {
        void Apply(OpenApiSchema schema, SchemaFilterContext context);
    }
}
