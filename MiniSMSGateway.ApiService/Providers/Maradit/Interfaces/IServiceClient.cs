using System;

namespace Maradit.Interfaces
{
	public interface IServiceClient : IDisposable
	{
		TResponse Send<TResponse>(object request);		
	
		void SendOneWay(object request);
	}
}
