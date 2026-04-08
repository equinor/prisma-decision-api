using Microsoft.AspNetCore.Mvc;
using PrismaApi.Application.Interfaces.Services;
using PrismaApi.Domain.Dtos;
using PrismaApi.Infrastructure.Context;

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
        public async Task<ActionResult<SpiderAssessmentOutgoingDto>> GetSpiderAssessment(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.GetAsync(id, user, ct);
            return result != null ? Ok(result) : NotFound();
        }

        [HttpGet("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> GetAllSpiderAssessments(CancellationToken ct = default)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.GetAllAsync(user, ct);
            return Ok(result);
        }

        [HttpPost("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> CreateSpiderAssessment([FromBody] List<SpiderAssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            var result = await _spiderAssessmentService.CreateAsync(dtos, user, ct);
            return result != null && result.Count > 0 ? Ok(result) : NotFound();
        }

        [HttpPut("spiderassessments")]
        public async Task<ActionResult<List<SpiderAssessmentOutgoingDto>>> UpdateSpiderAssessment([FromBody] List<SpiderAssessmentIncomingDto> dtos, CancellationToken ct = default)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            await _spiderAssessmentService.UpdateAsync(dtos, user, ct);
            return NoContent();
        }

        [HttpDelete("spiderassessments/{id:guid}")]
        public async Task<ActionResult> DeleteSpiderAssessment(Guid id, CancellationToken ct = default)
        {
            UserOutgoingDto user = await _userService.GetOrCreateUserFromGraphMeAsync(GetUserCacheKeyFromClaims());
            await _spiderAssessmentService.DeleteAsync(new List<Guid> { id }, user, ct);
            return NoContent();
        }
    }
}
