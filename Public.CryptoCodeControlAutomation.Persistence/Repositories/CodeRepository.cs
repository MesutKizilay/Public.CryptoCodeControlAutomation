using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeLookup;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Contexts;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using System.Data;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class CodeRepository : EfRepositoryBase<Code, CryptoContext>, ICodeRepository
    {
        public CodeRepository(CryptoContext context) : base(context)
        {

        }

        public async Task<int> UpdateScrapCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default)
        {
            return await Context.Codes.Where(c => ids.Contains(c.CodeId))
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Status, status)
                                                                            .SetProperty(c => c.UpdatedAt, DateTime.Now),
                                                                             cancellationToken);
        }

        public async Task<int> UpdateRecoverCodes(List<long> ids, CodeStatus status, CancellationToken cancellationToken = default)
        {
            return await Context.Codes.Where(c => ids.Contains(c.CodeId))
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Status, status)
                                                                            .SetProperty(c => c.UpdatedAt, DateTime.Now)
                                                                            .SetProperty(c => c.RecoverAt, DateTime.Now)
                                                                            .SetProperty(c => c.ShiftDate, c => c.AllocatedAt),
                                                                            cancellationToken);
        }

        public async Task<GetCodeLookupDto?> GetCodeLookup(string code, CancellationToken cancellationToken = default)
        {
            var connection = Context.Database.GetDbConnection();
            var shouldCloseConnection = connection.State != ConnectionState.Open;

            if (shouldCloseConnection)
                await connection.OpenAsync(cancellationToken);

            try
            {
                await using var command = connection.CreateCommand();
                command.CommandText = "cz.sp_CodeLookup";
                command.CommandType = CommandType.StoredProcedure;
                command.Parameters.Add(new SqlParameter("@Code", SqlDbType.VarChar, 128) { Value = code });

                await using var reader = await command.ExecuteReaderAsync(cancellationToken);
                if (!await reader.ReadAsync(cancellationToken))
                    return null;

                return new GetCodeLookupDto
                {
                    Code = reader.GetString(reader.GetOrdinal("Code")),
                    Status = (CodeStatus)Convert.ToByte(reader["Status"]),
                    PackagingLevel = GetNullableByte(reader["PackagingLevel"]),
                    StationId = GetNullableInt(reader["StationId"]),
                    StationCode = GetNullableString(reader["StationCode"]),
                    LineCode = GetNullableString(reader["LineCode"]),
                    PlannedOrderId = GetNullableLong(reader["PlannedOrderId"]),
                    PlannedOrderNo = GetNullableString(reader["PlannedOrderNo"]),
                    MaterialNo = GetNullableString(reader["MaterialNo"]),
                    PlannedOrderLine = GetNullableString(reader["PlannedOrderLine"]),
                    PlannedOrderStatus = reader["PlannedOrderStatus"] == DBNull.Value
                        ? null
                        : (PlannedOrderStatus)Convert.ToByte(reader["PlannedOrderStatus"]),
                    SalesOrderItemId = Convert.ToInt64(reader["SalesOrderItemId"]),
                    SalesOrderNo = reader.GetString(reader.GetOrdinal("SalesOrderNo")),
                    SalesItemNo = reader.GetString(reader.GetOrdinal("SalesItemNo")),
                    SalesMaterialNo = reader.GetString(reader.GetOrdinal("SalesMaterialNo")),
                    GTIN = GetNullableString(reader["GTIN"]),
                    AllocatedAt = GetNullableDateTime(reader["AllocatedAt"]),
                    ProducedAt = GetNullableDateTime(reader["ProducedAt"])
                };
            }
            finally
            {
                if (shouldCloseConnection)
                    await connection.CloseAsync();
            }
        }

        private static byte? GetNullableByte(object value)
        {
            return value == DBNull.Value ? null : Convert.ToByte(value);
        }

        private static int? GetNullableInt(object value)
        {
            return value == DBNull.Value ? null : Convert.ToInt32(value);
        }

        private static long? GetNullableLong(object value)
        {
            return value == DBNull.Value ? null : Convert.ToInt64(value);
        }

        private static string? GetNullableString(object value)
        {
            return value == DBNull.Value ? null : Convert.ToString(value);
        }

        private static DateTime? GetNullableDateTime(object value)
        {
            return value == DBNull.Value ? null : Convert.ToDateTime(value);
        }
    }
}
