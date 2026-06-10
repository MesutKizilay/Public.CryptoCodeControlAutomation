using AutoMapper;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Queries.GetList;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Create;
using CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Commands.Update;
using CryptoCodeControlAutomation.Application.Features.UploadJobs.Queries.GetBySalesOrderItemId;
using CryptoCodeControlAutomation.Domain.Entities;
using Core.Persistence.Paging;

namespace CryptoCodeControlAutomation.Application.Features.SalesOrderItems.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<SalesOrderItem, GetListSalesOrderItemDto>().ReverseMap();
            CreateMap<Paginate<SalesOrderItem>, Paginate<GetListSalesOrderItemDto>>().ReverseMap();
            CreateMap<CreateSalesOrderItemCommand, SalesOrderItem>();
            CreateMap<UpdateSalesOrderItemCommand, SalesOrderItem>();
        }
    }
}
