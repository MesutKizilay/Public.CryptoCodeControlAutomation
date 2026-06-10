using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Services.Upload;
using Hangfire;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using System.Data;

namespace CryptoCodeControlAutomation.Application.Services.UploadJobService
{
    //[AutomaticRetry(Attempts = 3, OnAttemptsExceeded = AttemptsExceededAction.Fail)]
    public class UploadJobManager : IUploadJobService
    {
        private readonly IUploadJobRepository _uploadJobRepository;
        private readonly IConfiguration _configuration;
        private readonly ISalesOrderItemRepository _salesOrderItemRepository;


        public UploadJobManager(IUploadJobRepository uploadJobRepository, IConfiguration configuration, ISalesOrderItemRepository salesOrderItemRepository)
        {
            _uploadJobRepository = uploadJobRepository;
            _configuration = configuration;
            _salesOrderItemRepository = salesOrderItemRepository;
        }

        public async Task ProcessUpload(UploadJob uploadJob, SalesOrderItem salesOrderItem)
        {
            uploadJob.StartedAt = DateTime.Now;
            uploadJob.Status = UploadJobStatus.Importing;
            await _uploadJobRepository.Update(uploadJob);

            SqlTransaction? tx = null;
            try
            {
                if (!File.Exists(uploadJob.FilePath))
                    throw new FileNotFoundException("Upload file not found", uploadJob.FilePath);

                var lines = await File.ReadAllLinesAsync(uploadJob.FilePath);
                uploadJob.TotalRows = lines.Length;

                var table = new DataTable();
                table.Columns.Add("Code", typeof(string));
                table.Columns.Add("SalesOrderItemId", typeof(long));
                table.Columns.Add("PlannedOrderId", typeof(long));
                table.Columns.Add("StationId", typeof(int));
                table.Columns.Add("PackagingLevel", typeof(int));
                table.Columns.Add("Status", typeof(int));
                table.Columns.Add("AllocatedAt", typeof(DateTime));
                table.Columns.Add("ProducedAt", typeof(DateTime));
                table.Columns.Add("UpdatedAt", typeof(DateTime));

                foreach (var line in lines)
                {
                    var codeValue = line?.Trim();
                    if (string.IsNullOrEmpty(codeValue))
                        continue;

                    if (codeValue.Length > 1 && codeValue.StartsWith("\"") && codeValue.EndsWith("\""))
                    {
                        codeValue = codeValue.Substring(1, codeValue.Length - 2).Replace("\"\"", "\"");
                    }

                    var row = table.NewRow();
                    row["Code"] = codeValue;
                    row["SalesOrderItemId"] = uploadJob.SalesOrderItemId;
                    row["PlannedOrderId"] = DBNull.Value;
                    row["StationId"] = DBNull.Value;
                    row["PackagingLevel"] = DBNull.Value;
                    row["Status"] = CodeStatus.Available;
                    row["AllocatedAt"] = DBNull.Value;
                    row["ProducedAt"] = DBNull.Value;
                    row["UpdatedAt"] = DateTime.Now;
                    table.Rows.Add(row);
                }

                //await Task.Delay(10000);

                var cs = _configuration.GetConnectionString("MsSqlConnectionString");
                using var conn = new SqlConnection(cs);
                await conn.OpenAsync();

                tx = conn.BeginTransaction();

                using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, tx);
                bulk.DestinationTableName = "cz.Codes";
                bulk.BatchSize = 10000;
                bulk.BulkCopyTimeout = 300;

                bulk.ColumnMappings.Add("Code", "Code");
                bulk.ColumnMappings.Add("SalesOrderItemId", "SalesOrderItemId");
                bulk.ColumnMappings.Add("PlannedOrderId", "PlannedOrderId");
                bulk.ColumnMappings.Add("StationId", "StationId");
                bulk.ColumnMappings.Add("PackagingLevel", "PackagingLevel");
                bulk.ColumnMappings.Add("Status", "Status");
                bulk.ColumnMappings.Add("AllocatedAt", "AllocatedAt");
                bulk.ColumnMappings.Add("ProducedAt", "ProducedAt");
                bulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");

                await bulk.WriteToServerAsync(table);
                tx.Commit();

                uploadJob.InsertedRows = table.Rows.Count;
                uploadJob.Status = UploadJobStatus.Done;
                uploadJob.FinishedAt = DateTime.Now;
                await _uploadJobRepository.Update(uploadJob);

                salesOrderItem.RemainingUnitQty = table.Rows.Count;
                await _salesOrderItemRepository.Update(salesOrderItem);
            }
            catch (SqlException ex) when (ex.Number == 2601 || ex.Number == 2627)
            {
                //tx?.Rollback();
                uploadJob.Status = UploadJobStatus.Failed;
                uploadJob.ErrorText = "Daha önce yüklenmiş mükerrer kayıt bulundu.";
                uploadJob.FinishedAt = DateTime.Now;
                await _uploadJobRepository.Update(uploadJob);
                throw;
            }
            catch (Exception ex)
            {
                //tx?.Rollback();
                uploadJob.Status = UploadJobStatus.Failed;
                uploadJob.ErrorText = ex.Message;
                uploadJob.FinishedAt = DateTime.Now;
                await _uploadJobRepository.Update(uploadJob);
                throw;
            }
        }

        // Eski akış (try/catch birleştirilmeden önceki hali) referans olsun diye eklendi.
        //public async Task ProcessUpload2(UploadJob uploadJob)
        //{
        //    uploadJob.StartedAt = DateTime.Now;
        //    uploadJob.Status = UploadJobStatus.Importing;
        //    await _uploadJobRepository.Update(uploadJob);

        //    try
        //    {
        //        if (!File.Exists(uploadJob.FilePath))
        //            throw new FileNotFoundException("Upload file not found", uploadJob.FilePath);

        //        var lines = await File.ReadAllLinesAsync(uploadJob.FilePath);
        //        uploadJob.TotalRows = lines.Length;

        //        var table = new DataTable();
        //        table.Columns.Add("Code", typeof(string));
        //        table.Columns.Add("SalesOrderItemId", typeof(long));
        //        table.Columns.Add("PlannedOrderId", typeof(long));
        //        table.Columns.Add("StationId", typeof(int));
        //        table.Columns.Add("PackagingLevel", typeof(int));
        //        table.Columns.Add("Status", typeof(int));
        //        table.Columns.Add("AllocatedAt", typeof(DateTime));
        //        table.Columns.Add("ProducedAt", typeof(DateTime));
        //        table.Columns.Add("UpdatedAt", typeof(DateTime));

        //        foreach (var line in lines)
        //        {
        //            var codeValue = line?.Trim();
        //            if (string.IsNullOrEmpty(codeValue))
        //                continue;

        //            var row = table.NewRow();
        //            row["Code"] = codeValue;
        //            row["SalesOrderItemId"] = uploadJob.SalesOrderItemId;
        //            row["PlannedOrderId"] = DBNull.Value;
        //            row["StationId"] = DBNull.Value;
        //            row["PackagingLevel"] = DBNull.Value;
        //            row["Status"] = CodeStatus.Available;
        //            row["AllocatedAt"] = DBNull.Value;
        //            row["ProducedAt"] = DBNull.Value;
        //            row["UpdatedAt"] = DateTime.Now;
        //            table.Rows.Add(row);
        //        }

        //        var cs = _configuration.GetConnectionString("MsSqlConnectionString");
        //        using var conn = new SqlConnection(cs);
        //        await conn.OpenAsync();

        //        //using var tx = conn.BeginTransaction();

        //        try
        //        {
        //            using var bulk = new SqlBulkCopy(conn, SqlBulkCopyOptions.CheckConstraints, null);
        //            bulk.DestinationTableName = "cz.Codes";
        //            bulk.BatchSize = 100;
        //            bulk.BulkCopyTimeout = 300;

        //            bulk.ColumnMappings.Add("Code", "Code");
        //            bulk.ColumnMappings.Add("SalesOrderItemId", "SalesOrderItemId");
        //            bulk.ColumnMappings.Add("PlannedOrderId", "PlannedOrderId");
        //            bulk.ColumnMappings.Add("StationId", "StationId");
        //            bulk.ColumnMappings.Add("PackagingLevel", "PackagingLevel");
        //            bulk.ColumnMappings.Add("Status", "Status");
        //            bulk.ColumnMappings.Add("AllocatedAt", "AllocatedAt");
        //            bulk.ColumnMappings.Add("ProducedAt", "ProducedAt");
        //            bulk.ColumnMappings.Add("UpdatedAt", "UpdatedAt");

        //            await bulk.WriteToServerAsync(table);
        //            //tx.Commit();
        //        }
        //        catch (Exception)
        //        {
        //            //tx.Rollback();
        //            throw;
        //        }

        //        uploadJob.InsertedRows = table.Rows.Count;
        //        uploadJob.Status = UploadJobStatus.Done;
        //        uploadJob.FinishedAt = DateTime.Now;
        //        await _uploadJobRepository.Update(uploadJob);
        //    }
        //    catch (Exception ex)
        //    {
        //        uploadJob.Status = UploadJobStatus.Failed;
        //        uploadJob.ErrorText = ex.Message;
        //        uploadJob.FinishedAt = DateTime.Now;
        //        await _uploadJobRepository.Update(uploadJob);
        //        throw;
        //    }
        //}
    }
}
