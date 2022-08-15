using MailServer.Dtos;
using MailServer.Params;
using MailServer.Properties;
using MailServer.Utils;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MailServer.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ImapController : ControllerBase
    {
        private AppSettings settings;
        private Emailer emailer;
        private EmailAcct[] hosts;
        private IMemoryCache _cache;

        // Unable to resolve service for type 'Microsoft.Extensions.Caching.Memory.IMemoryCache' while attempting to activate 'MailServer.Controllers.ImapController'
        public ImapController(IOptions<AppSettings> config, IMemoryCache memoryCache)
        {
            settings = config.Value;
            emailer = new Emailer(settings);
            hosts = JsonFileReader.Read<EmailAcct[]>(@"./Email/hosts.json");
            _cache = memoryCache;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<EmailDto>>> Get([FromQuery]GetParams prms)
        {
            string checkMsg = GetParams.CheckParams(prms);
            if (checkMsg != null) return BadRequest(checkMsg);

            try
            {
                var msgs = await getMessages(prms.AccountName);

                List <EmailDto> res = new List<EmailDto>();
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

        [HttpDelete]
        [Consumes("application/json")]
        public async Task<ActionResult<string>> Delete()
        {
            var body = Request.Body;
            SelParams prms = null;
            
            using (var reader = new StreamReader(Request.Body, encoding: Encoding.UTF8, detectEncodingFromByteOrderMarks: false))
            {
                var bodyString = await reader.ReadToEndAsync();
                prms = JsonConvert.DeserializeObject<SelParams>(bodyString);
            }

            string checkMsg = SelParams.CheckParams(prms);
            if (checkMsg != null) return BadRequest(checkMsg);
            
            uint[] uids = prms.uids;
            try
            {
                var msgs = await getMessagesCached(prms.AccountName);
                await emailer.DeleteMessages(msgs.Where(msg => uids.Contains(msg.Uid.Id)).ToArray(), false);
                return Ok(string.Empty);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("markasread")]
        public async Task<ActionResult<string>> MarkAsRead(SelParams prms) 
        {
            string checkMsg = SelParams.CheckParams(prms);
            if (checkMsg != null) return BadRequest(checkMsg);

            uint[] uids = prms.uids;
            try
            {
                var msgs = await getMessagesCached(prms.AccountName);
                emailer.MarkMessageAsSeen(msgs.Where(msg => uids.Contains(msg.Uid.Id)).FirstOrDefault(), false);
                return Ok(string.Empty);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        private string getMessagesCacheKey(string accountName) 
        {
            return $"EmailMessages_{accountName}";
        }

        private async Task<List<EmailMessage>> getMessagesCached(string accountName) 
        {
            List<EmailMessage> msgs = null;
            string cachekey = getMessagesCacheKey(accountName);
            if (_cache.TryGetValue(cachekey, out List<EmailMessage> data))
            {
                msgs = data;
            }
            else
            {
                msgs = await getMessages(accountName);
                _cache.Set(cachekey, msgs);
            }
            return msgs;
        }

        private async Task<List<EmailMessage>> getMessages(string accountName) 
        {
            var acct = hosts.SingleOrDefault(item => item.Email == accountName);
            await emailer.ConnectAndRetrieve(acct, false);
            return emailer.GetMessages();
        }
    }
}
