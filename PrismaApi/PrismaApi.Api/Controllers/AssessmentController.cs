using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Application.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure;

namespace PrismaApi.Api.Controllers
{
    [ApiController]
    [Route("")]
    public class AssessmentController : PrismaBaseEntityController
    {
        private readonly IUserService _userService;
        private readonly IAssessmentService _assessmentService;

        public AssessmentController(AppDbContext dbContext, IUserService userService, IAssessmentService assessmentService) : base(dbContext)
        {
            _userService = userService;
            _assessmentService = assessmentService;
        }
        [HttpGet("assessments/{id}")]
        public async Task<ActionResult<AssessmentOutgoingDto>> GetAssessments(Guid id)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _assessmentService.GetAsync(new List<Guid> { id }, user);
            return result != null && result.Count > 0 ? Ok(result[0]) : NotFound();
        }
        [HttpGet("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> GetAllAssessments()
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _assessmentService.GetAllAsync(user);
            return Ok(result);
        }

        [HttpPost("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> CreateAssessment([FromBody] List<AssessmentIncomingDto> dtos)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _assessmentService.CreateAsync(dtos, user);
            return result != null && result.Count > 0 ? Ok(result) : NotFound();

        }
        [HttpPut("assessments")]
        public async Task<ActionResult<List<AssessmentOutgoingDto>>> UpdateAssessment([FromBody] List<AssessmentIncomingDto> dtos)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            await _assessmentService.UpdateRangeAsync(dtos, user);
            return NoContent();

        }
    }


}
