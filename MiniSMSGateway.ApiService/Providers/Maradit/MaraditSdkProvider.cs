using Maradit;
using Maradit.Types;

namespace MiniSMSGateway.ApiService.Providers.Maradit
{
    public class MaraditSdkProvider : ISmsProvider
    {
        private Messenger _messenger;

        public MaraditSdkProvider(Messenger messenger)
        {
            _messenger = messenger;
        }

        public Task<bool> SendAsync(string phoneNumber, string content)
        {
            Header header = new Header()
            {
                From = "BALABAN"
            };
            List<string> to = new List<string>();
            to.Add(phoneNumber);

            var response = _messenger.Submit(content, to, header, DataCoding.UCS2);


            var success = response is not null
                          && response.Response is not null
                          && response.Response.Status.Code == 200;


            return Task.FromResult(success);
        }
    }
}
