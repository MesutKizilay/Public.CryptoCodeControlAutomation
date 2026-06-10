using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Microsoft.Extensions.Logging;
using System.Diagnostics;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Rules;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Delete
{
    public class DeleteSalesOrderItemCommand : IRequest
    {
        public long Id { get; set; }

        public class DeleteSalesOrderItemCommandHandler : IRequestHandler<DeleteSalesOrderItemCommand>
        {
            private readonly ISalesOrderItemRepository _salesOrderItemRepository;
            private readonly IUploadJobRepository _uploadJobRepository;
            private readonly ILogger<DeleteSalesOrderItemCommandHandler> _logger;
            private readonly SalesOrderItemBusinessRules _salesOrderItemBusinessRules;

            public DeleteSalesOrderItemCommandHandler(ISalesOrderItemRepository salesOrderItemRepository, ILogger<DeleteSalesOrderItemCommandHandler> logger, SalesOrderItemBusinessRules salesOrderItemBusinessRules, IUploadJobRepository uploadJobRepository)
            {
                _salesOrderItemRepository = salesOrderItemRepository;
                _logger = logger;
                _salesOrderItemBusinessRules = salesOrderItemBusinessRules;
                _uploadJobRepository = uploadJobRepository;
            }

            public async Task Handle(DeleteSalesOrderItemCommand request, CancellationToken cancellationToken)
            {
                await _salesOrderItemBusinessRules.WasUploadJobImported(request.Id);
                await _salesOrderItemBusinessRules.AreThereProducedCodes(request.Id);


                var salesOrderItem = await _salesOrderItemRepository.Get(b => b.SalesOrderItemId == request.Id, cancellationToken: cancellationToken);
                if (salesOrderItem == null) return;

                Stopwatch stopwatch = Stopwatch.StartNew();
                //await _salesOrderItemRepository.Delete(entity, cancellationToken);
                //await _salesOrderItemRepository.Delete2(request.Id, cancellationToken);
                await _salesOrderItemRepository.Delete4(request.Id, cancellationToken);
                stopwatch.Stop();

                salesOrderItem.IsOpen = false;
                salesOrderItem.Status = SalesOrderItemStatus.Cancelled;
                await _salesOrderItemRepository.Update(salesOrderItem);

                var uploadJob = await _uploadJobRepository.Get(b => b.SalesOrderItemId == request.Id, cancellationToken: cancellationToken);
                uploadJob.Status = UploadJobStatus.Deleted;
                await _uploadJobRepository.Update(uploadJob);

                _logger.LogWarning("SalesOrderItem with Id {Id} deleted in {ElapsedMilliseconds} s", request.Id, stopwatch.Elapsed.TotalSeconds);
            }
        }
    }
}