using Marketplace.Application.Catalog.Commands.CreateBanner;
using Marketplace.Application.Catalog.Commands.UpdateBanner;
using Marketplace.Application.Catalog.Commands.DeleteBanner;
using Marketplace.Application.Catalog.Queries.GetBanners;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers
{
    public class BannersController : ApiControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> GetBanners([FromQuery] bool onlyActive = true)
        {
            var query = new GetBannersQuery { OnlyActive = onlyActive };
            return HandleResult(await Sender.Send(query));
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> CreateBanner(CreateBannerCommand command)
        {
            return HandleResult(await Sender.Send(command));
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> UpdateBanner(Guid id, UpdateBannerCommand command)
        {
            if (id != command.Id)
                return BadRequest("Id mismatch.");

            return HandleResult(await Sender.Send(command));
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "SuperAdmin,SystemAdmin")]
        public async Task<IActionResult> DeleteBanner(Guid id)
        {
            var command = new DeleteBannerCommand { Id = id };
            return HandleResult(await Sender.Send(command));
        }
    }
}
