/*
	Note: The preferred ServiceClients are in ServiceStack.Common.dll
	https://github.com/ServiceStack/ServiceStack/blob/master/src/ServiceStack.Common/ServiceClient.Web/XmlServiceClient.cs

	This is a dependency-free ServiceClient using the built-in .NET BCL Serializers.
*/

using System;
using System.IO;
using System.Net;
using System.Security.Authentication;
using Maradit.Interfaces;

namespace Maradit
{
	public class XmlServiceClient : IServiceClient
	{
		public XmlServiceClient(string baseUri)
		{
			this.BaseUri = baseUri;
		}

		public string BaseUri { get; set; }

		public TimeSpan? Timeout { get; set; }

        public bool Status { get; private set; }
        
        public string Error { get; private set; }

	    public T Send<T>(object request)
		{
			var xmlRequest = DataContractSerializer.Instance.Parse(request);
			var requestUri = this.BaseUri + "/" + request.GetType().Name;
			var client = WebRequest.Create(requestUri);

			try
			{
				client.Method = "POST";
				if (this.Timeout.HasValue)
				{
					client.Timeout = (int)this.Timeout.Value.TotalMilliseconds;
				}

				client.ContentType = "application/xml";
				using (var writer = new StreamWriter(client.GetRequestStream()))
				{
					writer.Write(xmlRequest);
				}

                var xml = new StreamReader(client.GetResponse().GetResponseStream()).ReadToEnd();
                var response = (T)DataContractDeserializer.Instance.Parse(xml, typeof(T));
                Status = true;

                return response;
			}
			catch (AuthenticationException ex)
			{
			    Status = false;
			    Error = ex.Message;
			    //throw WebRequestUtils.CreateCustomException(requestUri, ex) ?? ex;
			}
            catch(Exception ex)
            {
                Status = false;
                Error = ex.Message;
            }

	        return default(T);
		}

		public void SendOneWay(object request)
		{
			var xmlRequest = DataContractSerializer.Instance.Parse(request);
			var requestUri = this.BaseUri + "/" + request.GetType().Name;
			var client = WebRequest.Create(requestUri);

			try
			{
				if (this.Timeout.HasValue)
				{
					client.Timeout = (int)this.Timeout.Value.TotalMilliseconds;
				}

				client.Method = "POST";
				client.ContentType = "application/xml";
                client.Headers.Add("User-Agent", "Maradit/3.0");

				using (var writer = new StreamWriter(client.GetRequestStream()))
				{
					writer.Write(xmlRequest);
				}
			}
			catch (AuthenticationException ex)
			{
				throw WebRequestUtils.CreateCustomException(requestUri, ex) ?? ex;
			}
		}

		public void Dispose() { }
	}
}