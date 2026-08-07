using System.Collections.Generic;
using System.Runtime.Serialization;
using Maradit.Operations;
using Maradit.Types;

[assembly: ContractNamespace("http://schemas.maradit.net/api/types", ClrNamespace = "Maradit.Operations")]
[assembly: ContractNamespace("http://schemas.maradit.net/api/types", ClrNamespace = "Maradit.Types")]

namespace Maradit
{
    public class Messenger
    {
        private readonly string _username;
        private readonly string _password;
        private readonly XmlServiceClient _client;

        public Messenger(string username, string password)
        {
            _username = username;
            _password = password;
            _client = new XmlServiceClient("http://sgw.maradit.net/api/xml/syncreply");
        }

        public LoginResponse Login()
        {
            var request = new Login
                              {
                                  Credential = new Credential {Username = _username, Password = _password}
                              };

            var response = _client.Send<LoginResponse>(request);

            return _client.Status ? response : default(LoginResponse);
        }

        public GetBalanceResponse GetBalance()
        {
            var request = new GetBalance
                              {
                                  Credential = new Credential { Username = _username, Password = _password }
                              };

            var response = _client.Send<GetBalanceResponse>(request);

            return _client.Status ? response : default(GetBalanceResponse);
        }

        public GetSettingsResponse GetSettings()
        {
            var request = new GetSettings
                              {
                                  Credential = new Credential {Username = _username, Password = _password}
                              };

            var response = _client.Send<GetSettingsResponse>(request);

            return _client.Status ? response : default(GetSettingsResponse);
        }

        public CancelResponse Cancel(long messageId)
        {
            var request = new Cancel
                              {
                                  Credential = new Credential {Username = _username, Password = _password},
                                  MessageId = messageId
                              };

            var response = _client.Send<CancelResponse>(request);

            return _client.Status ? response : default(CancelResponse);
        }

        public QueryResponse Query(long messageId, string msisdn = "")
        {
            var request = new Query 
            {
                Credential = new Credential { Username = _username, Password = _password },
                MessageId = messageId,
                MSISDN = msisdn
            };

            var response = _client.Send<QueryResponse>(request);

            return _client.Status ? response : default(QueryResponse);
        }

        public QueryMultiResponse QueryMulti(DateRange dateRange)
        {
            var request = new QueryMulti
            {
                Credential = new Credential { Username = _username, Password = _password },
                Range = dateRange
            };

            var response = _client.Send<QueryMultiResponse>(request);

            return _client.Status ? response : default(QueryMultiResponse);
        }

        public QueryStatsResponse QueryStats()
        {
            var request = new QueryStats
            {
                Credential = new Credential { Username = _username, Password = _password }
            };

            var response = _client.Send<QueryStatsResponse>(request);

            return _client.Status ? response : default(QueryStatsResponse);
        }

        public ReceiveResponse Receive(DateRange dateRange, InboxState state, string recipient = "")
        {
            var request = new Receive
            {
                Credential = new Credential { Username = _username, Password = _password },
                Range = dateRange,
                Recipient = recipient,
                State = state
            };

            var response = _client.Send<ReceiveResponse>(request);

            return _client.Status ? response : default(ReceiveResponse);
        }

        public SubmitResponse Submit(string message, List<string> to, Header header, DataCoding dataCoding)
        {
            var request = new Submit
            {
                Credential = new Credential { Username = _username, Password = _password },
                Header = header,
                Message = message,
                To = to,
                DataCoding = dataCoding
            };

            var response = _client.Send<SubmitResponse>(request);

            return _client.Status ? response : default(SubmitResponse);
        }

        public SubmitMultiResponse SubmitMulti(List<Envelope> envelopes, Header header, DataCoding dataCoding)
        {
            var request = new SubmitMulti 
            {
                Credential = new Credential { Username = _username, Password = _password },
                Header = header,
                Envelopes = envelopes,
                DataCoding = dataCoding
            };

            var response = _client.Send<SubmitMultiResponse>(request);

            return _client.Status ? response : default(SubmitMultiResponse);
        }

        public SubmitDataResponse SubmitData(List<DataItem> dataItems , List<string> to, Header header)
        {
            var request = new SubmitData
            {
                Credential = new Credential { Username = _username, Password = _password },
                Header = header,
                Data = dataItems,
                To = to
            };

            var response = _client.Send<SubmitDataResponse>(request);

            return _client.Status ? response : default(SubmitDataResponse);
        }

        public SubmitDataMultiResponse SubmitDataMulti(List<DataEnvelope> envelopes, Header header)
        {
            var request = new SubmitDataMulti 
            {
                Credential = new Credential { Username = _username, Password = _password },
                Header = header,
                Envelopes = envelopes
            };

            var response = _client.Send<SubmitDataMultiResponse>(request);

            return _client.Status ? response : default(SubmitDataMultiResponse);
        }
    }
}
