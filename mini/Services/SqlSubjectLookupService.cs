using LmsMini.WebApp.ApiClients.Dto;
using Microsoft.Data.SqlClient;

namespace LmsMini.WebApp.Services
{
    public class SqlSubjectLookupService : ISubjectLookupService
    {
        private readonly string _cs;

        public SqlSubjectLookupService(IConfiguration config)
        {
            _cs = config.GetConnectionString("LmsConnection")
                  ?? throw new InvalidOperationException("Missing ConnectionStrings:LmsConnection");
        }

        public async Task<List<LookupItemDto>> GetSubjectsAsync()
        {
            var list = new List<LookupItemDto>();

            await using var conn = new SqlConnection(_cs);
            await conn.OpenAsync();

            await using var cmd = conn.CreateCommand();
            cmd.CommandText = @"SELECT SubID, SubName FROM Subjects ORDER BY SubName";

            await using var rd = await cmd.ExecuteReaderAsync();
            while (await rd.ReadAsync())
            {
                list.Add(new LookupItemDto
                {
                    Id = rd.GetString(0),
                    Name = rd.GetString(1)
                });
            }

            return list;
        }
    }
}
