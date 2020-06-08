using FileUploadAndValidation.Utils;
using Interswitch.Security.JwtToken;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace FileUploadApi.Utils
{
    public class PassportOauthMiddleware
    {
        private readonly ILogger<PassportOauthMiddleware> _logger;
        private readonly RequestDelegate _next;
        private readonly ITokenHandlerRepository _tokenHandlerRepository;

        public PassportOauthMiddleware(RequestDelegate next, ITokenHandlerRepository tokenHandlerRepository,
            ILogger<PassportOauthMiddleware> logger
        )
        {
            _tokenHandlerRepository = tokenHandlerRepository;
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (!context.Request.Headers.ContainsKey("Authorization"))
                {
                    context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                    return;
                }

                var authHeader = context.Request.Headers["Authorization"].ToString();
                var handler = _tokenHandlerRepository.GetHandler(authHeader);

                if (handler == null)
                    throw new Exception("Invalid access token");

                var tokenData = handler.GetTokenData(authHeader);
                context.User = tokenData.ClPrincipal;
                context.Request.Headers["userName"] = tokenData.UserName;
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError($">>>> Authentication failed {ex.Message}: {ex.StackTrace}");
                context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
            }
        }
    }


    public static class PassportOauthMiddlewareExtensions
    {
        public static IApplicationBuilder UsePassportOauthMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PassportOauthMiddleware>();
        }
    }

    public abstract class TokenHandler
    {
        public abstract TokenData GetTokenData(string token);
        public abstract bool CanHandleToken(string token);
    }

    public interface ITokenHandlerRepository
    {
        void Add(TokenHandler handler);
        TokenHandler GetHandler(string token);
    }

    public class TokenHandlerRepository : ITokenHandlerRepository
    {
        private readonly List<TokenHandler> _tokenHandlers = new List<TokenHandler>();

        private TokenHandlerRepository()
        {
        }

        public TokenHandlerRepository(IAppConfig config, ILoggerFactory logger)
        {
            Add(new JwtTokenHandler(config, logger));
        }

        public void Add(TokenHandler handler)
        {
            _tokenHandlers.Add(handler);
        }

        public TokenHandler GetHandler(string token)
        {
            return _tokenHandlers.FirstOrDefault(h => h.CanHandleToken(token));
        }
    }

    public class JwtTokenHandler : TokenHandler
    {
        private readonly IAppConfig _appConfiguration;
        private readonly ILogger _logger;

        public JwtTokenHandler(IAppConfig appConfiguration, ILoggerFactory logger)
        {
            _appConfiguration = appConfiguration;
            _logger = logger.CreateLogger(GetType());
        }

        public override bool CanHandleToken(string authHeader)
        {
            return authHeader.StartsWith("Bearer ");
        }

        public override TokenData GetTokenData(string authHeader)
        {
            if (!CanHandleToken(authHeader))
                throw new ArgumentException("Invalid access token");

            //var validateTokenIssuer = false;
            const string expectedIssuer = "";
            const bool validateTokenExpiryDate = true;

            var tokenValidationResult = new JwtTokenRsaValidator(_appConfiguration.PassportRsaPublicKey,
                    new JwtTokenValidationOptions { ValidateExpirationTime = validateTokenExpiryDate })
                .Validate(authHeader.Replace("Bearer ", ""), "qb-business-portal,passport".Split(','),
                    expectedIssuer);

            if (tokenValidationResult.Principal == null)
                throw new ArgumentException("Invalid access token");

            //var metaData = tokenValidationResult.Principal.Claims.FirstOrDefault(c => c.Type == "metadata")?.Value;
            var userName = tokenValidationResult.Principal.Claims.FirstOrDefault(c => c.Type == "user_name")?.Value;
            if (userName == null)
                throw new ArgumentException("Invalid access token, user not found");
            return new TokenData
            {
                ClPrincipal = tokenValidationResult.Principal,
                UserName = userName
            };
        }

        private void TryExtractJsonMetadata(string metaData, out TokenData tokenData)
        {
            if (string.IsNullOrWhiteSpace(metaData))
            {
                tokenData = null;
                return;
            }

            try
            {
                tokenData = JsonConvert.DeserializeObject<TokenData>(metaData);
            }
            catch (JsonSerializationException jsonSe)
            {
                tokenData = null;
                _logger.LogInformation(jsonSe.Message);
            }
        }
    }

    public class TokenData
    {
        public ClaimsPrincipal ClPrincipal { get; set; }
        public string UserName { get; set; }
        public string InstitutionCode { get; set; }
        public string InstitutionType { get; set; }
        public string CbnCode { get; set; }
    }
}
