using AutoMapper;

namespace Discount.Grpc.Mapper
{
    public class AutoMapperConfiguration
    {
        public static MapperConfiguration RegisterMappings()
        {
            return new MapperConfiguration(cfg =>
            {
                cfg.AddProfile(new DiscountProfile());
            });
        }

    }
}
