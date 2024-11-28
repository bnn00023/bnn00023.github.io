using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Configure.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TestController : ControllerBase
    {
        private readonly IOptions<TestOption> _options;
        private readonly IOptionsSnapshot<TestOption> _optionsSnapshot;
        private readonly IOptionsMonitor<TestOption> _optionsMonitor;
        private readonly MemorySource _memorySource;

        public TestController(IOptions<TestOption> options, IOptionsSnapshot<TestOption> optionsSnapshot, IOptionsMonitor<TestOption> optionsMonitor, MemorySource memorySource)
        {
            _options = options;
            _optionsSnapshot = optionsSnapshot;
            _optionsMonitor = optionsMonitor;
            _memorySource = memorySource;
        }

        // GET api/test/options
        [HttpGet("options")]
        public ActionResult<TestOption> GetOptions()
        {
            return Ok(_options.Value);
        }

        // GET api/test/optionsSnapshot
        [HttpGet("optionsSnapshot")]
        public ActionResult<TestOption> GetOptionsSnapshot()
        {
            return Ok(_optionsSnapshot.Value);
        }

        // GET api/test/optionsMonitor
        [HttpGet("optionsMonitor")]
        public ActionResult<TestOption> GetOptionsMonitor()
        {
            return Ok(_optionsMonitor.CurrentValue);
        }

        [HttpPut("Change")]
        public Task ChangeText(string text)
        {
            _memorySource.Chnage(text);
            return Task.CompletedTask;
        }
    }
}