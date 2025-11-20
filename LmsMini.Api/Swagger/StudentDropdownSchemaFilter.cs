using LmsMini.Application.DTOs;
using LmsMini.Domain.Models;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace LmsMini.Api.Swagger
{
    public class StudentDropdownSchemaFilter : ISchemaFilter
    {
        private readonly IServiceScopeFactory _scopeFactory;

        public StudentDropdownSchemaFilter(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Tìm property trong schema theo PascalCase hoặc camelCase
        /// </summary>
        private OpenApiSchema? FindProperty(OpenApiSchema schema, string name)
        {
            // PascalCase
            if (schema.Properties.TryGetValue(name, out var prop))
                return prop;

            // camelCase
            var camel = char.ToLower(name[0]) + name.Substring(1);
            if (schema.Properties.TryGetValue(camel, out prop))
                return prop;

            return null; // không có → bỏ qua
        }

        /// <summary>
        /// Gán enum dropdown an toàn không crash
        /// </summary>
        private void SetEnum(OpenApiSchema schema, string propName, IEnumerable<string> values)
        {
            var prop = FindProperty(schema, propName);
            if (prop == null) return; // tránh lỗi KeyNotFoundException

            prop.Enum = values
                .Select(v => (IOpenApiAny)new OpenApiString(v))
                .ToList();
        }

        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            // Chỉ áp dụng cho CreateStudentDto
            if (context.Type != typeof(CreateStudentDto))
                return;

            if (schema?.Properties == null)
                return;

            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<LmsDbContext>();

            // Department List
            var departList = db.Departments
                .Select(d => $"{d.DepartId} | {d.DepartName}")
                .ToList();

            SetEnum(schema, "DepartID", departList);

            // Class List
            var classList = db.Classes
                .Select(c => $"{c.ClassId} | {c.ClassName}")
                .ToList();

            SetEnum(schema, "ClassID", classList);

            // Major List
            var majorList = db.Majors
                .Select(m => $"{m.MajorId} | {m.MajorName}")
                .ToList();

            SetEnum(schema, "StuMajor", majorList);
        }
    }
}
