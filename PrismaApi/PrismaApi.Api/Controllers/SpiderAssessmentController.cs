using System;
using System.Collections.Generic;
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
    public class SpiderAssessmentController : PrismaBaseEntityController
    {
        private readonly IUserService _userService;
        private readonly ISpiderAssessmentService _spiderAssessmentService;

        public SpiderAssessmentController(
            AppDbContext dbContext,
            IUserService userService,
            ISpiderAssessmentService spiderAssessmentService) : base(dbContext)
        {
            _userService = userService;
            _spiderAssessmentService = spiderAssessmentService;
        }

        [HttpGet("spiderassessments/{id}")]
        public async Task<ActionResult<SpiderAssessmentOutgoingDto>> GetSpiderAssessment(Guid id)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.GetAsync(id, user);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> GetAllSpiderAssessments()
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.GetAllAsync(user);
            return Ok(result);
        }

        [HttpPost("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> CreateSpiderAssessment([FromBody] List<SpiderAssessmentIncomingDto> dtos)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.CreateAsync(dtos, user);
            return result != null && result.Count > 0 ? Ok(result) : NotFound();
        }

        [HttpPut("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> UpdateSpiderAssessment([FromBody] List<SpiderAssessmentIncomingDto> dtos)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            await _spiderAssessmentService.UpdateAsync(dtos, user);
            return NoContent();
        }

        [HttpDelete("spiderassessments")]
        public async Task<ActionResult> DeleteSpiderAssessment([FromBody] List<Guid> ids)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            await _spiderAssessmentService.DeleteAsync(ids, user);
            return NoContent();
        }
    }
}
