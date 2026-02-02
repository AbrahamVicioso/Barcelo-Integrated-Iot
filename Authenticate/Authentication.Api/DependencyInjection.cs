using Authentication.Api.Contracts;
using Authentication.Api.Data;
using Authentication.Api.Services;
using Authentication.Api.UseCases.Commands.LoginUser;
using Authentication.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Authentication.Api
{
    static class DependencyInjection
    {
        public static IServiceCollection AddServicesDependency(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AuthenticationDbContext>(opt =>
            {
                opt.UseSqlServer("name=DefaultConnection");
            });

            services.AddScoped<IJwtGenerator, JwtGenerator>();

            // Configure Kafka Producer
            var kafkaConfig = new KafkaProducerConfig();
            configuration.GetSection("KafkaProducer").Bind(kafkaConfig);
            services.AddSingleton(kafkaConfig);
            services.AddSingleton<IKafkaProducerService, KafkaProducerService>();

            return services;
        }
    }
}
