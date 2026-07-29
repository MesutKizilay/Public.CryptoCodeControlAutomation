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
            var scrappedAt = DateTime.Now;

            return await Context.Codes.Where(c => ids.Contains(c.CodeId))
                                      .ExecuteUpdateAsync(setters => setters.SetProperty(c => c.Status, status)
                                                                            .SetProperty(c => c.UpdatedAt, scrappedAt)
                                                                            .SetProperty(c => c.ScrapAt, scrappedAt),
                                                                             cancellationToken);
        }

        public async Task<int> UpdateRecoverCodes(List<long> ids, CodeStatus status, int shelfLifeValue, byte shelfLifeUnit, CancellationToken cancellationToken = default)
        {
            var recoveredAt = DateTime.Now;
            var query = Context.Codes.Where(c => ids.Contains(c.CodeId) && c.Status != CodeStatus.ProducedOk);

            return shelfLifeUnit switch
            {
                0 => await query.ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.Status, status)
                                      .SetProperty(c => c.ProducedAt, recoveredAt)
                                      .SetProperty(c => c.ShiftDate, recoveredAt.AddHours(-8).Date)
                                      .SetProperty(c => c.ExpirationDate, recoveredAt.AddHours(-8).Date.AddDays(shelfLifeValue))
                                      .SetProperty(c => c.UpdatedAt, recoveredAt)
                                      .SetProperty(c => c.RecoverAt, recoveredAt),
                    cancellationToken),
                1 => await query.ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.Status, status)
                                      .SetProperty(c => c.ProducedAt, recoveredAt)
                                      .SetProperty(c => c.ShiftDate, recoveredAt.AddHours(-8).Date)
                                      .SetProperty(c => c.ExpirationDate, recoveredAt.AddHours(-8).Date.AddDays(shelfLifeValue * 7))
                                      .SetProperty(c => c.UpdatedAt, recoveredAt)
                                      .SetProperty(c => c.RecoverAt, recoveredAt),
                    cancellationToken),
                2 => await query.ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.Status, status)
                                      .SetProperty(c => c.ProducedAt, recoveredAt)
                                      .SetProperty(c => c.ShiftDate, recoveredAt.AddHours(-8).Date)
                                      .SetProperty(c => c.ExpirationDate, recoveredAt.AddHours(-8).Date.AddMonths(shelfLifeValue))
                                      .SetProperty(c => c.UpdatedAt, recoveredAt)
                                      .SetProperty(c => c.RecoverAt, recoveredAt),
                    cancellationToken),
                3 => await query.ExecuteUpdateAsync(
                    setters => setters.SetProperty(c => c.Status, status)
                                      .SetProperty(c => c.ProducedAt, recoveredAt)
                                      .SetProperty(c => c.ShiftDate, recoveredAt.AddHours(-8).Date)
                                      .SetProperty(c => c.ExpirationDate, recoveredAt.AddHours(-8).Date.AddYears(shelfLifeValue))
                                      .SetProperty(c => c.UpdatedAt, recoveredAt)
                                      .SetProperty(c => c.RecoverAt, recoveredAt),
                    cancellationToken),
                _ => throw new ArgumentOutOfRangeException(nameof(shelfLifeUnit), shelfLifeUnit, "Geçersiz raf ömrü birimi.")
            };
        }

        public async Task<int> ResetProduction(long? salesOrderItemId, long? plannedOrderId, CancellationToken cancellationToken = default)
        {
            var updatedAt = DateTime.Now;
            var query = Context.Codes.AsQueryable();

            if (salesOrderItemId.HasValue && salesOrderItemId.Value > 0)
            {
                query = query.Where(c => c.SalesOrderItemId == salesOrderItemId.Value);
            }

            if (plannedOrderId.HasValue && plannedOrderId.Value > 0)
            {
                query = query.Where(c => c.PlannedOrderId == plannedOrderId.Value);
            }

            var updatedCodeCount = await query.ExecuteUpdateAsync(
                setters => setters.SetProperty(c => c.Status, CodeStatus.Available)
                                  .SetProperty(c => c.PlannedOrderId, (long?)null)
                                  .SetProperty(c => c.StationId, (int?)null)
                                  .SetProperty(c => c.PackagingLevel, (byte?)null)
                                  .SetProperty(c => c.AllocatedAt, (DateTime?)null)
                                  .SetProperty(c => c.ProducedAt, (DateTime?)null)
                                  .SetProperty(c => c.ShiftDate, (DateTime?)null)
                                  .SetProperty(c => c.RecoverAt, (DateTime?)null)
                                  .SetProperty(c => c.UpdatedAt, updatedAt)
                                  .SetProperty(c => c.ExpirationDate, (DateTime?)null)
                                  .SetProperty(c => c.ScrapAt, (DateTime?)null)
                                  .SetProperty(c => c.IsScrapAllocated, false),
                cancellationToken);

            await Context.Database.ExecuteSqlInterpolatedAsync($"""                                                                
                                                                    UPDATE cz.PlannedOrderCodeCursor
                                                                    SET NextCodeId = NULL
                                                                    WHERE ({salesOrderItemId} IS NULL OR SalesOrderItemId = {salesOrderItemId})
                                                                    AND ({plannedOrderId} IS NULL OR PlannedOrderId = {plannedOrderId})                                                                                                                                    
                                                                """, cancellationToken);

            return updatedCodeCount;
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
