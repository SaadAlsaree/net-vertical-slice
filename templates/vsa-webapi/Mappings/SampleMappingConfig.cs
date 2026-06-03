using VsaTemplate.Data.Entities;
using VsaTemplate.Features.SampleFeature.CreateSample;
using VsaTemplate.Features.SampleFeature.UpdateSample;
using VsaTemplate.Features.SampleFeature.GetByIdSample;
using VsaTemplate.Features.SampleFeature.GetListSample;
using Mapster;

namespace VsaTemplate.Mappings;

public class SampleMappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<CreateSampleCommand, Sample>();

        config.NewConfig<UpdateSampleCommand, Sample>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt);

        config.NewConfig<Sample, GetByIdSampleViewModel>();

        config.NewConfig<Sample, GetListSampleViewModel>();
    }
}
