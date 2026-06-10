using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Persistence.Contexts;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Core.Persistence.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System.Data;

namespace CryptoCodeControlAutomation.Persistence.Repositories
{
    public class SalesOrderItemRepository : EfRepositoryBase<SalesOrderItem, CryptoContext>, ISalesOrderItemRepository
    {
        public SalesOrderItemRepository(CryptoContext context) : base(context)
        {
        }

        public async Task Delete2(long salesOrderItemId, CancellationToken cancellationToken)
        {
            await Context.Database.ExecuteSqlRawAsync("""
                                                           DECLARE @SalesOrderItemId BIGINT = {0};
                                                           DECLARE @BatchSize INT = 20000;
                                                           
                                                           WHILE 1=1
                                                           BEGIN
                                                               DELETE TOP (@BatchSize)
                                                               FROM cz.Codes
                                                               WHERE SalesOrderItemId = @SalesOrderItemId;
                                                           
                                                               IF @@ROWCOUNT = 0 BREAK;
                                                           END
                                                           
                                                           DELETE FROM cz.SalesOrderItems
                                                           WHERE SalesOrderItemId = @SalesOrderItemId;
                                                           """, salesOrderItemId);
        }

        public async Task Delete3(long salesOrderItemId, CancellationToken cancellationToken)
        {
            var salesOrderItems = await Context.Codes.ToListAsync(cancellationToken);
            Context.Codes.RemoveRange(salesOrderItems);
            var x = await Context.SalesOrderItems.FirstOrDefaultAsync(s => s.SalesOrderItemId == salesOrderItemId);
            Context.SalesOrderItems.Remove(x!);
            await Context.SaveChangesAsync(cancellationToken);
        }

        public async Task Delete4(long salesOrderItemId, CancellationToken cancellationToken)
        {
            await Context.Codes
                .Where(c => c.SalesOrderItemId == salesOrderItemId)
                .ExecuteDeleteAsync(cancellationToken);

            //await Context.UploadJobs
            //    .Where(c => c.SalesOrderItemId == salesOrderItemId)
            //    .ExecuteDeleteAsync(cancellationToken);

            //await Context.SalesOrderItems
            //    .Where(s => s.SalesOrderItemId == salesOrderItemId)
            //    .ExecuteDeleteAsync(cancellationToken);
        }

        public async Task<long> ImportCodesBulkInsert(long salesOrderItemId, string filePath, int firstRow = 0, string fieldTerminator = ",", string rowTerminator = "0x0d0a", CancellationToken cancellationToken = default)
        {
            var salesOrderItemIdParam = new SqlParameter("@SalesOrderItemId", SqlDbType.BigInt) { Value = salesOrderItemId };
            var filePathParam = new SqlParameter("@FilePath", SqlDbType.VarChar, 512) { Value = filePath };
            var firstRowParam = new SqlParameter("@FirstRow", SqlDbType.Int) { Value = firstRow };
            var fieldTerminatorParam = new SqlParameter("@FieldTerminator", SqlDbType.VarChar, 10) { Value = fieldTerminator };
            var rowTerminatorParam = new SqlParameter("@RowTerminator", SqlDbType.VarChar, 10) { Value = rowTerminator };
            var uploadJobIdParam = new SqlParameter("@UploadJobId", SqlDbType.BigInt) { Direction = ParameterDirection.Output };

            const string sql = """
                                EXEC cz.sp_ImportCodes_BulkInsert
                                     @SalesOrderItemId,
                                     @FilePath,
                                     @FirstRow,
                                     @FieldTerminator,
                                     @RowTerminator,
                                     @UploadJobId OUTPUT
                                """;

            await Context.Database.ExecuteSqlRawAsync(sql, new object[] { salesOrderItemIdParam, filePathParam, firstRowParam, fieldTerminatorParam, rowTerminatorParam, uploadJobIdParam }, cancellationToken);

            return uploadJobIdParam.Value is long id ? id : Convert.ToInt64(uploadJobIdParam.Value);
        }

        public async Task<long> ActivateAndStartPlannedOrder(SalesOrderItem salesOrderItem, string lineCode = "HAT1", CancellationToken cancellationToken = default)
        {
            var executionStrategy = Context.Database.CreateExecutionStrategy();

            return await executionStrategy.ExecuteAsync(async () =>
            {
                await using var transaction = await Context.Database.BeginTransactionAsync(cancellationToken);

                try
                {
                    salesOrderItem.Status = SalesOrderItemStatus.Active;
                    salesOrderItem.IsOpen = true;
                    salesOrderItem.UpdatedAt = DateTime.Now;

                    Context.SalesOrderItems.Update(salesOrderItem);
                    await Context.SaveChangesAsync(cancellationToken);

                    var plannedOrderIdParam = new SqlParameter("@PlannedOrderId", SqlDbType.BigInt)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var salesOrderNoParam = new SqlParameter("@SalesOrderNo", SqlDbType.VarChar, 32)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var salesItemNoParam = new SqlParameter("@SalesItemNo", SqlDbType.VarChar, 16)
                    {
                        Direction = ParameterDirection.Output
                    };
                    var messageParam = new SqlParameter("@Message", SqlDbType.VarChar, 512)
                    {
                        Direction = ParameterDirection.Output
                    };

                    const string sql = """
                                       EXEC cz.sp_PlannedOrder_Start
                                            @PlannedOrderNo = @PlannedOrderNo,
                                            @MaterialNo = @MaterialNo,
                                            @TotalCaseQty = @TotalCaseQty,
                                            @TotalUnitQty = @TotalUnitQty,
                                            @LineCode = @LineCode,
                                            @P1Enabled = @P1Enabled,
                                            @P2Enabled = @P2Enabled,
                                            @P3Enabled = @P3Enabled,
                                            @P4Enabled = @P4Enabled,
                                            @PlannedOrderId = @PlannedOrderId OUTPUT,
                                            @SalesOrderNo = @SalesOrderNo OUTPUT,
                                            @SalesItemNo = @SalesItemNo OUTPUT,
                                            @Message = @Message OUTPUT
                                       """;

                    var parameters = new object[]
                    {
                        new SqlParameter("@PlannedOrderNo", SqlDbType.VarChar, 64) { Value = salesOrderItem.SalesOrderNo },
                        new SqlParameter("@MaterialNo", SqlDbType.VarChar, 64) { Value = salesOrderItem.MaterialNo },
                        new SqlParameter("@TotalCaseQty", SqlDbType.Int) { Value = (object?)salesOrderItem.SapCaseQty ?? DBNull.Value },
                        new SqlParameter("@TotalUnitQty", SqlDbType.Int) { Value = salesOrderItem.SapPlannedUnitQty },
                        new SqlParameter("@LineCode", SqlDbType.VarChar, 16) { Value = lineCode },
                        new SqlParameter("@P1Enabled", SqlDbType.Bit) { Value = true },
                        new SqlParameter("@P2Enabled", SqlDbType.Bit) { Value = true },
                        new SqlParameter("@P3Enabled", SqlDbType.Bit) { Value = true },
                        new SqlParameter("@P4Enabled", SqlDbType.Bit) { Value = true },
                        plannedOrderIdParam,
                        salesOrderNoParam,
                        salesItemNoParam,
                        messageParam
                    };

                    await Context.Database.ExecuteSqlRawAsync(sql, parameters, cancellationToken);
                    await transaction.CommitAsync(cancellationToken);

                    return Convert.ToInt64(plannedOrderIdParam.Value);
                }
                catch
                {
                    try
                    {
                        await transaction.RollbackAsync(cancellationToken);
                    }
                    catch (InvalidOperationException)
                    {
                        // The stored procedure may already have rolled back the transaction.
                    }

                    throw;
                }
            });
        }
    }
}
