using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Rest.Proxy.V1;
using Twilio.Rest.Proxy.V1.Service;
using Twilio.Rest.Proxy.V1.Service.Session;
using Twilio.Rest.Proxy.V1.Service.Session.Participant;

namespace APITwilio.Controllers
{
    [ApiController]
    [Route("[controller]/[Action]")]
    public class HomeController : ControllerBase
    {
        string accountSid = "ACxxxxxxxxxxxxxxxxxxxxxxxxx";
        string authToken = "dcxxxxxxxxxxxxxxxxxxxxxxxxxx";
        string twilioNumberSID = "PNxxxxxxxxxxxxxxxxxxxx";
        string twilioNumber = "+31111111";
        string participantA_number = "+333111222";
        string participantB_number = "+333111555";
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
       
        }
       

    [HttpGet]
    public IEnumerable<PhoneNumberResource> GetProxyNumberPool(string serviceID)
    {

      
            TwilioClient.Init(accountSid, authToken);
            var phoneNumbers = PhoneNumberResource.Read(
                pathServiceSid: serviceID,
                limit: 20
            );

            return phoneNumbers;
        }


    [HttpPost]
        public string CreateProxyService()
        {

            try
            {
                TwilioClient.Init(accountSid, authToken);
                Guid n = Guid.NewGuid();

                var service = ServiceResource.Create(uniqueName: n.ToString());

                //add a twilio number to pool
                var phoneNumber1 = PhoneNumberResource.Create(
                                   sid: twilioNumberSID,
                                   pathServiceSid: service.Sid);


                //create a proxy session in the service
                var session = SessionResource.Create(
                               uniqueName: n.ToString(),
                               pathServiceSid: service.Sid,
                               mode: "message-only");

                //add 2 participants to the session
                var participantA = ParticipantResource.Create(
                                   friendlyName: "A",
                                   identifier: participantA_number,
                                   pathServiceSid: service.Sid,
                                   pathSessionSid: session.Sid);

                var participantB = ParticipantResource.Create(
                                   friendlyName: "B",
                                   identifier: participantB_number,
                                   pathServiceSid: service.Sid,
                                   pathSessionSid: session.Sid);

                //notify both users about the session created
                var messageInteractionA = MessageInteractionResource.Create(
                                        body: "reply to this message to  chat to B!",
                                        pathServiceSid: service.Sid,
                                        pathSessionSid: session.Sid,
                                        pathParticipantSid: participantA.Sid);


                var messageInteractionB = MessageInteractionResource.Create(
                                        body: "reply to this message to chat to A !",
                                        pathServiceSid: service.Sid,
                                        pathSessionSid: session.Sid,
                                        pathParticipantSid: participantB.Sid
                                    );
            }
            catch (Exception ex)
            {
                return "failed";
            }

            return "success";
        }

        [HttpPost]
        public string SendMessageFrom_A_to_B()
        {
            try {
                TwilioClient.Init(accountSid, authToken);
                var message = MessageResource.Create(
                body: "Hi B this is a message  from   A",
                from: new Twilio.Types.PhoneNumber(participantA_number),
                to: new Twilio.Types.PhoneNumber(twilioNumber) 
                );
            }
            catch (Exception ex)
            {
                return "failed";
            }

            return "success";
        }

        [HttpPost]
        public string SendMessageFrom_B_to_A()
        {
            TwilioClient.Init(accountSid, authToken);
            try
            {
                var message = MessageResource.Create(
                    body: "Hi A this is a message  from   B",
                    from: new Twilio.Types.PhoneNumber(participantB_number),
                    to: new Twilio.Types.PhoneNumber(twilioNumber)  
                    );
            }
            catch (Exception ex)
            {
                return "failed";
            }

            return "success";
        }

    }
}
