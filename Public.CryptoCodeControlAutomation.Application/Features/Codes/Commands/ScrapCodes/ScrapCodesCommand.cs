using System;
using System.Linq;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using CryptoCodeControlAutomation.Domain.Enums;
using Core.CrossCuttingConcerns.Exceptions.Types;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.ScrapCodes
{
    public class ScrapCodesCommand : IRequest<ScrapCodesResponse>
    {
        public List<long> CodeIds { get; set; } = new();

        public class ScrapCodesCommandHandler : IRequestHandler<ScrapCodesCommand, ScrapCodesResponse>
        {
            private readonly ICodeRepository _codeRepository;

            public ScrapCodesCommandHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<ScrapCodesResponse> Handle(ScrapCodesCommand request, CancellationToken cancellationToken)
            {
                //if (request.CodeIds == null || request.CodeIds.Count == 0)
                //    throw new BusinessException("Code listesi bos olamaz.");
                //
                //var ids = request.CodeIds.Distinct().ToList();

                var updated = await _codeRepository.UpdateScrapCodes(request.CodeIds, CodeStatus.Scrap, cancellationToken);

                return new ScrapCodesResponse { UpdatedCount = updated };
            }
        }
    }
}
