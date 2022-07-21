using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using netServer.Dtos;
using netServer.Params;
using netServer.Properties;
using netServer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace netServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImapController : ControllerBase
    {
        private AppSettings settings;
        private Emailer emailer;
        EmailAcct[] hosts;

        public ImapController(IOptions<AppSettings> config)
        {
            settings = config.Value;
            emailer = new Emailer(settings);
            hosts = JsonFileReader.Read<EmailAcct[]>(@"./Email/hosts.json");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailDto>>> Get([FromQuery]EmailParams eparams)
        {
            if (eparams == null || string.IsNullOrEmpty(eparams.AccountName)) return BadRequest("Host name is unknown");

            try
            {
                var acct = hosts.SingleOrDefault(item => item.Email == eparams.AccountName);
                var msgs = await emailer.ConnectAndRetrieve(acct, false);
                List<EmailDto> res = new List<EmailDto>();
                foreach (EmailMessage msg in msgs)
                {
                    res.Add(EmailDto.FromMessage(msg));
                }
                return Ok(res);
            }
            catch (Exception ex) 
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
