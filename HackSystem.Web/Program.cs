using System;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Threading.Tasks;
using Blazored.LocalStorage;
using HackSystem.Web.Authentication.Extensions;
using HackSystem.Web.Common;
using HackSystem.Web.Configurations;
using HackSystem.Web.CookieStorage;
using HackSystem.Web.Services.Authentication;
using HackSystem.Web.Services.API.Program;
using HackSystem.Web.Services.Program;
using HackSystem.Common;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using HackSystem.Web.Services.API.Authentication;

namespace HackSystem.Web
{
    public static class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            var apiConfiguration = builder.Configuration.GetSection("APIConfiguration").Get<APIConfiguration>();

            builder.RootComponents.Add<App>("#app");

            builder
                .InitConfiguration()
                .InitService(apiConfiguration)
                .RegisterServices(apiConfiguration)
                .InitAuthorizationPolicy();

            await builder.Build().RunAsync();
        }

        public static WebAssemblyHostBuilder InitService(this WebAssemblyHostBuilder builder, APIConfiguration apiConfiguration)
        {
            builder.Services
                .AddLogging()
                .AddBlazoredLocalStorage()
                .AddCookieStorage()
                .AddAuthorizationCore()
                .AddHackSystemAuthentication(options =>
                {
                    options.AnonymousState.User.Claims.Append(new Claim(ClaimTypes.Name, "Anonymous"));
                    options.AuthenticationURL = apiConfiguration.APIURL;
                    options.TokenExpiryInMinutes = apiConfiguration.TokenExpiryInMinutes;
                    options.AuthenticationScheme = WebCommonSense.AuthenticationScheme;
                    options.AuthenticationType = WebCommonSense.AuthenticationType;
                    options.AuthTokenName = WebCommonSense.AuthTokenName;
                    options.ExpiryClaimType = WebCommonSense.ExpiryClaimType;
                })
                .AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .AddHttpClient();

            return builder;
        }

        /// <summary>
        /// 初始化配置
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        public static WebAssemblyHostBuilder InitConfiguration(this WebAssemblyHostBuilder builder)
        {
            builder.Services.Configure<APIConfiguration>(builder.Configuration.GetSection("APIConfiguration"));

            return builder;
        }

        /// <summary>
        /// 注册服务
        /// </summary>
        /// <param name="builder"></param>
        /// <returns></returns>
        /// <remarks>可以有更优雅的方式外置这些代码，需要注意 Servcide 需要的 Options 的传递</remarks>
        public static WebAssemblyHostBuilder RegisterServices(this WebAssemblyHostBuilder builder, APIConfiguration apiConfiguration)
        {
            builder.Services.AddHttpClient<IAuthenticationService, AuthenticationService>(httpClient => httpClient.BaseAddress = new Uri(apiConfiguration.APIURL));
            builder.Services.AddHttpClient<IBasicProgramService, BasicProgramService>(httpClient => httpClient.BaseAddress = new Uri(apiConfiguration.APIURL));

            return builder;
        }

        /// <summary>
        /// 配置授权策略
        /// </summary>
        /// <param name="builder"></param>
        /// <remarks>
        /// 需要满足策略的所有需求才可以获得此策略，
        /// 可以在 Authorize 特性的 Policy 验证需要的策略
        /// </remarks>
        /// <returns></returns>
        public static WebAssemblyHostBuilder InitAuthorizationPolicy(this WebAssemblyHostBuilder builder)
        {
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy(WebCommonSense.AuthorizationPolicy.HackerPolicy, policy =>
                {
                    policy.RequireRole(CommonSense.Roles.HackerRole);
                });
                options.AddPolicy(WebCommonSense.AuthorizationPolicy.ProfessionalHackerPolicy, policy =>
                {
                    policy.RequireRole(CommonSense.Roles.HackerRole);
                    policy.RequireClaim(CommonSense.Claims.ProfessionalClaim, "true", "TRUE", "True");
                });
                options.AddPolicy(WebCommonSense.AuthorizationPolicy.LeonPolicy, policy =>
                {
                    policy.RequireUserName("Leon");
                });
            });

            return builder;
        }
    }
}
