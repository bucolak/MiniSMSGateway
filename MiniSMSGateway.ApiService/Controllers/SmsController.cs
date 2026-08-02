using Microsoft.AspNetCore.Mvc;
using MiniSMSGateway.ApiService.Data;
using MiniSMSGateway.ApiService.DTO;
using MiniSMSGateway.ApiService.Models;
using MiniSMSGateway.ApiService.Services;

namespace MiniSMSGateway.ApiService.Controllers
{
    [ApiController]
    [Route("api/sms")]
    public class SmsController : ControllerBase
    {
        private readonly ISmsService _smsService;

        public SmsController(ISmsService smsService)
        {
            _smsService = smsService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendSms(SendSmsRequest request)
        {
            if (!Request.Headers.TryGetValue("ApiKey", out var apiKey) || string.IsNullOrWhiteSpace(apiKey))
                return Unauthorized("ApiKey is not found!");
            var response = await _smsService.SendSms(request, apiKey.ToString());
            if (response is null)
                return Unauthorized("Invalid ApiKey!");
            return Ok(response);
        }

        [HttpGet("status/{id}")]
        public async Task<IActionResult> GetStatus(int id)
        {
            var response = await _smsService.GetStatus(id);
            if (response is null)
                return NotFound("Message not found!");
            return Ok(response);
        }

    }
}

/*
 https://claude.ai/share/030d0cca-49a4-4569-8817-200e611fe735
 */