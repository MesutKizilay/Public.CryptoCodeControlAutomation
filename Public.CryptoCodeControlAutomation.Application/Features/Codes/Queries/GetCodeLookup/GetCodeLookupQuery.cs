using Core.CrossCuttingConcerns.Exceptions.Types;
using CryptoCodeControlAutomation.Application.Services.Repositories;
using MediatR;

namespace CryptoCodeControlAutomation.Application.Features.Codes.Queries.GetCodeLookup
{
    public class GetCodeLookupQuery : IRequest<GetCodeLookupDto?>
    {
        public string Code { get; set; } = string.Empty;

        public class GetCodeLookupQueryHandler : IRequestHandler<GetCodeLookupQuery, GetCodeLookupDto?>
        {
            private readonly ICodeRepository _codeRepository;

            public GetCodeLookupQueryHandler(ICodeRepository codeRepository)
            {
                _codeRepository = codeRepository;
            }

            public async Task<GetCodeLookupDto?> Handle(GetCodeLookupQuery request, CancellationToken cancellationToken)
            {
                if (string.IsNullOrWhiteSpace(request.Code))
                    throw new BusinessException("Kod boş olamaz.");

                return await _codeRepository.GetCodeLookup(request.Code, cancellationToken);
            }
        }
    }
}
