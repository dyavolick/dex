using dex_webapp.Data;
using dex_webapp.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;


namespace dex_webapp.Services
{
    public class ParametersService
    {
        private readonly ApplicationDbContext _context;
        private readonly EthereumSettings _ethSettings;

        public ParametersService(ApplicationDbContext context, IOptions<EthereumSettings> ethSettings)
        {
            _context = context;
            _ethSettings = ethSettings.Value;
        }

        public async Task<string> GetParameterAsync(string parameterName)
        {
            var parameter = await _context.Parameters.FirstOrDefaultAsync(p => p.Key == parameterName);
            return parameter?.Value;
        }

        public async Task SetParameterAsync(string parameterName, string value, bool save)
        {
            var parameter = await _context.Parameters.FirstOrDefaultAsync(p => p.Key == parameterName);
            if (parameter == null)
            {
                parameter = new ServiceParameter { Key = parameterName };
                await _context.Parameters.AddAsync(parameter);
            }

            parameter.Value = value;

            if (save)
            {
                await _context.SaveChangesAsync();
            }
        }

        public async Task<long> GetLatestScannedBlockAsync(string latestScannedBlock)
        {
            var val = await GetParameterAsync(latestScannedBlock);

            return !string.IsNullOrEmpty(val) ? Convert.ToInt64(val) : 
                _ethSettings.EthereumSearchStartBlockNumber;
        }

        public async Task SetLatestScannedBlockAsync(string latestScannedBlock, long blockNumber, bool save = false)
        {
            await SetParameterAsync(latestScannedBlock, Convert.ToString(blockNumber), save);
        }
    }
}
