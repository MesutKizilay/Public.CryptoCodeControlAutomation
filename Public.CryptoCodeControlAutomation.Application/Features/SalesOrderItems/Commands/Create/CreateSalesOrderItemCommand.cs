using AutoMapper;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Rules;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Entities;
using CryptoCodeControlAutomation.Domain.Enums;
using CryptoCodeControlAutomation.Persistence.Services.Upload;
using Hangfire;
using MediatR;
using Microsoft.AspNetCore.Http;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Create
{
    public class CreateSalesOrderItemCommand : IRequest<CreateSalesOrderItemResponse>
    {
        public string SalesOrderNo { get; set; } = string.Empty;
        public string SalesItemNo { get; set; } = string.Empty;
        public string MaterialNo { get; set; } = string.Empty;
        public string? GTIN { get; set; }
        public int SapPlannedUnitQty { get; set; }
        public int? SapCaseQty { get; set; }
        public DateTime? SapValidatedAt { get; set; }
        public IFormFile? File { get; set; }
        public string? UploadsBasePath { get; set; }

        public class CreateSalesOrderItemCommandHandler : IRequestHandler<CreateSalesOrderItemCommand, CreateSalesOrderItemResponse>
        {
            private readonly SalesOrderItemBusinessRules _salesOrderItemBusinessRules;
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IUploadJobRepository _uploadJobRepository;
            private readonly IUploadJobService _uploadJobService;
            private readonly IMapper _mapper;

            public CreateSalesOrderItemCommandHandler(ISalesOrderItemRepository repository, IUploadJobRepository uploadJobRepository, IMapper mapper, IUploadJobService uploadJobService, SalesOrderItemBusinessRules salesOrderItemBusinessRules)
            {
                _salesOrderItemRepository = repository;
                _uploadJobRepository = uploadJobRepository;
                _mapper = mapper;
                _uploadJobService = uploadJobService;
                _salesOrderItemBusinessRules = salesOrderItemBusinessRules;
            }

            public async Task<CreateSalesOrderItemResponse> Handle(CreateSalesOrderItemCommand request, CancellationToken cancellationToken)
            {
                var salesOrderNo = request.SalesOrderNo?.Trim() ?? string.Empty;
                var salesItemNo = string.IsNullOrWhiteSpace(request.SalesItemNo) ? "1" : request.SalesItemNo.Trim();
                await _salesOrderItemBusinessRules.SalesOrderNoAndSalesItemNoShouldBeUnique(salesOrderNo, salesItemNo);

                var salesOrderItem = _mapper.Map<SalesOrderItem>(request);
                salesOrderItem.SalesOrderNo = salesOrderNo;
                salesOrderItem.SalesItemNo = salesItemNo;
                salesOrderItem.CreatedAt = DateTime.Now;
                salesOrderItem.IsOpen = false;
                salesOrderItem.Status = SalesOrderItemStatus.Passive;
                salesOrderItem.ApprovalStatus = SalesOrderItemApprovalStatus.PendingApproval;
                //salesOrderItem.RemainingUnitQty = request.SapPlannedUnitQty;

                await _salesOrderItemRepository.Add(salesOrderItem, cancellationToken);

                long? uploadJobId = null;
                if (request.File != null && !string.IsNullOrEmpty(request.UploadsBasePath))
                {
                    var uploadsRoot = Path.Combine(request.UploadsBasePath, "SalesOrderItems", salesOrderItem.SalesOrderItemId.ToString());
                    Directory.CreateDirectory(uploadsRoot);
                    var fileName = DateTime.Now.ToString("ddMMyyyy_HH.mm") + "_" + Path.GetFileName(request.File.FileName);
                    var filePath = Path.Combine(uploadsRoot, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await request.File.CopyToAsync(stream, cancellationToken);
                    }

                    var uploadJob = new UploadJob
                    {
                        SalesOrderItemId = salesOrderItem.SalesOrderItemId,
                        FilePath = filePath,
                        Status = UploadJobStatus.New,
                        CreatedAt = DateTime.Now
                    };

                    await _uploadJobRepository.Add(uploadJob, cancellationToken);
                    uploadJobId = uploadJob.UploadJobId;

                    //try
                    //{
                    //filePath = "C:\\Temp\\codes_20260217161912.csv";
                    //await _salesOrderItemRepository.ImportCodesBulkInsert(salesOrderItem.SalesOrderItemId, filePath, cancellationToken: cancellationToken);

                    // Hangfire ile eski kullanım (şimdilik devre dışı):
                    BackgroundJob.Enqueue<IUploadJobService>(s => s.ProcessUpload(uploadJob,salesOrderItem));
                    //BackgroundJob.Enqueue(() => _uploadJobService.ProcessUpload(uploadJob));
                    //}
                    //catch (Exception)
                    //{
                    //    throw;
                    //}


                }

                return new CreateSalesOrderItemResponse
                {
                    SalesOrderItemId = salesOrderItem.SalesOrderItemId,
                    UploadJobId = uploadJobId
                };
            }

        }
    }
}
