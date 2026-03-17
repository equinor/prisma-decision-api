using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace PrismaApi.Api.Controllers
{
    public class AssessmentWebSocketController : PrismaBaseController
    {
        public AssessmentWebSocketController()
        {
        }

        [HttpGet("/ws/assessment/{projectId}")]
        public async Task<IActionResult> GetAssessmentWebSocket(int projectId)
        {
            HttpContext.WebSockets.AcceptWebSocketAsync().Wait();
        }
    }
}
