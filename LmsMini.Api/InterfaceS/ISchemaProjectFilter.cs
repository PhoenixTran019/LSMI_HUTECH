using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.InterfaceS
{
    public interface ISchemaProjectFilter
    {
        void Apply(OpenApiSchema schema, SchemaFilterContext context);
    }
}
