using Ledger.Application.Common;
using Ledger.Application.Import.Commands;
using Ledger.Application.Import.DTOs;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ImportController(IMediator mediator) : ControllerBase
{
    /// <summary>
    /// The Bifrost — parse a CSV and preview the transactions before importing.
    /// </summary>
    [HttpPost("preview")]
    [ProducesResponseType(typeof(ApiResponse<ImportPreviewDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Preview(
        [FromQuery] Guid accountId,
        IFormFile file,
        CancellationToken ct)
    {
        if (file is null || file.Length == 0)
            return BadRequest(ApiResponse<ImportPreviewDto>.Fail("No file provided."));

        if (!file.FileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase))
            return BadRequest(ApiResponse<ImportPreviewDto>.Fail("Only CSV files are accepted through the Bifrost."));

        await using var stream = file.OpenReadStream();
        var result = await mediator.Send(
            new ParseCsvCommand(accountId, file.FileName, stream), ct);

        return Ok(ApiResponse<ImportPreviewDto>.Ok(result));
    }

    /// <summary>
    /// Confirm and persist a previously parsed CSV import.
    /// </summary>
    [HttpPost("confirm")]
    [ProducesResponseType(typeof(ApiResponse<ConfirmImportDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Confirm([FromBody] ConfirmImportCommand command, CancellationToken ct)
    {
        if (command.Rows.Count == 0)
            return BadRequest(ApiResponse<ConfirmImportDto>.Fail("No rows to import."));

        var result = await mediator.Send(command, ct);
        return Ok(ApiResponse<ConfirmImportDto>.Ok(result));
    }
}
