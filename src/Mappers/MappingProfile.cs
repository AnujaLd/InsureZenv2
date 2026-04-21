using AutoMapper;
using InsureZenv2.src.Models;
using InsureZenv2.src.DTOs;

namespace InsureZenv2.src.Mappers;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Claim mappings
        CreateMap<Claim, ClaimResponseDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()));

        CreateMap<Claim, ClaimDetailDto>()
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status.ToString()))
            .ForMember(dest => dest.MakerReview, opt => opt.MapFrom(src => src.MakerReview))
            .ForMember(dest => dest.CheckerReview, opt => opt.MapFrom(src => src.CheckerReview));

        CreateMap<ClaimIngestDto, Claim>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(_ => Guid.NewGuid()))
            .ForMember(dest => dest.ClaimNumber, opt => opt.MapFrom(_ => GenerateClaimNumber()))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(_ => ClaimStatus.Pending));

        // ClaimReview mappings
        CreateMap<ClaimReview, ClaimReviewDto>()
            .ForMember(dest => dest.ReviewedByUsername, opt => opt.MapFrom(src => src.ReviewedByUser!.Username))
            .ForMember(dest => dest.MakerRecommendation, opt => opt.MapFrom(src => src.MakerRecommendation.HasValue ? src.MakerRecommendation.ToString() : null))
            .ForMember(dest => dest.CheckerDecision, opt => opt.MapFrom(src => src.CheckerDecision.HasValue ? src.CheckerDecision.ToString() : null));

        // User mappings
        CreateMap<User, UserResponseDto>();

        // InsuranceCompany mappings
        CreateMap<InsuranceCompany, InsuranceCompanyDto>();
        CreateMap<CreateInsuranceCompanyDto, InsuranceCompany>();
    }

    private static string GenerateClaimNumber()
    {
        return $"CLM-{DateTime.UtcNow:yyyyMMdd}-{Guid.NewGuid().ToString().Substring(0, 8).ToUpper()}";
    }
}
