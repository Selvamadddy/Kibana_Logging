using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Kibana_Logging
{
    public interface ILoggerService
    {
        Task LogInformation(string message, object? data = null);

        Task LogWarning(string message, object? data = null);

        Task LogError(string message, Exception ex, object? data = null);
    }
}
