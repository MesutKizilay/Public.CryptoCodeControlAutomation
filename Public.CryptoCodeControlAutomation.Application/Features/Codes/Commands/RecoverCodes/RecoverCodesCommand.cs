using System.Linq;
using MediatR;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Domain.Enums;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Commands.RecoverCodes
{
    public class RecoverCodesCommand : IRequest<RecoverCodesResponse>
    {
        public List<long> CodeIds { get; set; } = new();

        public class RecoverCodesCommandHandler : IRequestHandler<RecoverCodesCommand, RecoverCodesResponse>
        {
            private readonly ICodeRepository _codeRepository;

            public RecoverCodesCommandHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<RecoverCodesResponse> Handle(RecoverCodesCommand request, CancellationToken cancellationToken)
            {
                var updated = await _codeRepository.UpdateRecoverCodes(request.CodeIds,CodeStatus.ProducedOk, cancellationToken);

                return new RecoverCodesResponse { UpdatedCount = updated };
            }
        }
    }
}
