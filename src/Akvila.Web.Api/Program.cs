using Akvila.Web.Api.Core.Extensions;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Akvila.WebApi.Tests")]
WebApplication.CreateBuilder(args)
    .RegisterServices()
    .Build()
    .RegisterServices()
    .Run();
