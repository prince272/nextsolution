using FluentValidation;
using Humanizer;
using Microsoft.Extensions.DependencyInjection;
using NextSolution.Core.Models;
using NextSolution.Core.Utilities;
using System.Reflection;

namespace NextSolution.Core.Services
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddChatService(this IServiceCollection services)
        {
            services.AddScoped<IChatService, ChatService>();
            return services;
        }

        public static IServiceCollection AddUserService(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            return services;
        }
    }
}