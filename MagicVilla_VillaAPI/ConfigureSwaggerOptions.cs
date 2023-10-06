using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace MagicVilla_VillaAPI
{
    public class ConfigureSwaggerOptions : IConfigureOptions<SwaggerGenOptions>
    {
        private readonly IApiVersionDescriptionProvider _apiVersionDescriptionProvider;

        public ConfigureSwaggerOptions(IApiVersionDescriptionProvider apiVersionDescriptionProvider)
        {
            _apiVersionDescriptionProvider = apiVersionDescriptionProvider;
        }

        public void Configure(SwaggerGenOptions options)
        {
            

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Scheme = "Bearer",
                //BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Name = "Authorization",
                Description = "Bearer Authentication with JWT Token",
                //Type = SecuritySchemeType.Http
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement()
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference{Type = ReferenceType.SecurityScheme, Id="Bearer"},
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });

            foreach (var desc in _apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                // creating a swagger document for our specified version 
                // I created swagger documentation for v1
                options.SwaggerDoc(desc.GroupName, new OpenApiInfo
                {
                    Version = desc.ApiVersion.ToString(),
                    Title = $"Magic Villa {desc.ApiVersion}",
                    Description = "API to manage Villa",
                    TermsOfService = new Uri("https://www.postman.com/"),
                    Contact = new OpenApiContact
                    {
                        Name = "insidecode",
                        Url = new Uri("https://github.com/insidecode-dev")
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Example License",
                        Url = new Uri("https://www.google.com/search?q=license&oq=license&aqs=chrome..69i57.3695j0j7&sourceid=chrome&ie=UTF-8")
                    }
                });
            }

            // creating a swagger document for our specified version 
            // I created swagger documentation for v1
            //options.SwaggerDoc("v1", new OpenApiInfo
            //{
            //    Version = "v1.0",
            //    Title = "Magic Villa V1",
            //    Description = "API to manage Villa",
            //    TermsOfService = new Uri("https://www.postman.com/"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "insidecode",
            //        Url = new Uri("https://github.com/insidecode-dev")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://www.google.com/search?q=license&oq=license&aqs=chrome..69i57.3695j0j7&sourceid=chrome&ie=UTF-8")
            //    }
            //});

            // I created swagger documentation for v2
            //options.SwaggerDoc("v2", new OpenApiInfo
            //{

            //    Version = "v2.0",
            //    Title = "Magic Villa V2",
            //    Description = "API to manage Villa",
            //    TermsOfService = new Uri("https://www.postman.com/"),
            //    Contact = new OpenApiContact
            //    {
            //        Name = "insidecode",
            //        Url = new Uri("https://github.com/insidecode-dev")
            //    },
            //    License = new OpenApiLicense
            //    {
            //        Name = "Example License",
            //        Url = new Uri("https://www.google.com/search?q=license&oq=license&aqs=chrome..69i57.3695j0j7&sourceid=chrome&ie=UTF-8")
            //    }
            //});
        }
    }
}
