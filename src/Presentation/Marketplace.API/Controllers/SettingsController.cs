using Marketplace.Application.Admin.Commands.UpdateSiteSetting;
using Marketplace.Application.Admin.Queries.GetSiteSettings;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Marketplace.API.Controllers;

public class SettingsController : ApiControllerBase
{
    [HttpGet]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> GetSettings(CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new GetSiteSettingsQuery(), cancellationToken);
        return HandleResult(result);
    }

    [HttpPut]
    [Authorize(Roles = "SuperAdmin,Admin")]
    public async Task<IActionResult> UpdateSetting([FromBody] UpdateSettingRequest request, CancellationToken cancellationToken)
    {
        var result = await Sender.Send(new UpdateSiteSettingCommand(request.Key, request.Value), cancellationToken);
        return HandleResult(result);
    }
}

public record UpdateSettingRequest(string Key, string Value);
