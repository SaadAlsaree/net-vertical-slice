using __RootNamespace__.Data.Entities.__Category__;
using __RootNamespace__.Features.__Entity__Feature.Create__Entity__;
using __RootNamespace__.Features.__Entity__Feature.Update__Entity__;
using __RootNamespace__.Features.__Entity__Feature.GetById__Entity__;
using __RootNamespace__.Features.__Entity__Feature.GetList__Entity__;
using Mapster;

namespace __RootNamespace__.Mappings;

public class __Entity__MappingConfig : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Create__Entity__Command, __Entity__>();

        config.NewConfig<Update__Entity__Command, __Entity__>()
            .Ignore(dest => dest.Id)
            .Ignore(dest => dest.CreatedAt);

        config.NewConfig<__Entity__, GetById__Entity__ViewModel>();

        config.NewConfig<__Entity__, GetList__Entity__ViewModel>();
    }
}
