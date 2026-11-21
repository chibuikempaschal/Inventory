using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace StockPilot.Infrastructure.Services
{
    public interface IDbAdminInitializer
    {

        Task SeedAsync(IConfiguration configuration, CancellationToken cancellationToken = default);

    }
}