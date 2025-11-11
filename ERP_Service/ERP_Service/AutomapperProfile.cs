using AutoMapper;

namespace ERP_Service
{
    public class AutomapperProfile : Profile
    {
        public AutomapperProfile()
        {
            CreateMap<Client, ClientDto>();
            CreateMap<CreateClientDto, Client>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<Technician, TechnicianDto>();
            CreateMap<CreateTechnicianDto, Technician>()
                .ForMember(dest => dest.Id, opt => opt.Ignore());

            CreateMap<WorkOrder, WorkOrderDto>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));
              

            CreateMap<CreateWorkOrderDto, WorkOrder>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.ErrorMessage, opt => opt.Ignore());
        }

        
    }
}
