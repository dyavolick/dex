using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace dex_webapp.Services
{
    public class AbiProvider
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly AbiLocation _options;

        public AbiProvider(IOptions<AbiLocation> options, IHostingEnvironment hostingEnvironment)
        {
            _hostingEnvironment = hostingEnvironment;
            _options = options.Value;
        }

        public async Task<string> GetCrowdsaleAbiAsync()
        {
            var appDir = _hostingEnvironment.ContentRootPath;
            var filePath = Path.Combine(appDir, _options.Folder, _options.EtherDeltaABIFileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("Cannot load file containing ABI", filePath);
            }

            return await File.ReadAllTextAsync(filePath, Encoding.UTF8);
        }


    }
}
