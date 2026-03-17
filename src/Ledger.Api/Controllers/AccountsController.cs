using Ledger.Application.Accounts.Commands;
using Ledger.Application.Accounts.DTOs;
using Ledger.Application.Accounts.Queries;
using Ledger.Application.Common;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<AccountDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var result = await mediator.Send(new GetAllAccountsQuery(), ct);
        return Ok(ApiResponse<IReadOnlyList<AccountDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken ct)
    {
        var result = await mediator.Send(new GetAccountByIdQuery(id), ct);
        return result is null
            ? NotFound(ApiResponse<AccountDto>.Fail($"Account {id} not found."))
            : Ok(ApiResponse<AccountDto>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateAccountRequest request, CancellationToken ct)
    {
        var command = new CreateAccountCommand(
            request.Name,
            request.Institution,
            request.AccountType,
            request.Currency);

        var result = await mediator.Send(command, ct);
        return CreatedAtAction(nameof(GetById), new { id = result.Id },
            ApiResponse<AccountDto>.Ok(result));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse<AccountDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateAccountRequest request, CancellationToken ct)
    {
        var command = new UpdateAccountCommand(id, request.Name, request.Institution, request.Currency, request.IsActive);
        var result = await mediator.Send(command, ct);
        return result is null
            ? NotFound(ApiResponse<AccountDto>.Fail($"Account {id} not found."))
            : Ok(ApiResponse<AccountDto>.Ok(result));
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var success = await mediator.Send(new DeleteAccountCommand(id), ct);
        return success
            ? Ok(ApiResponse.Ok())
            : NotFound(ApiResponse.Fail($"Account {id} not found."));
    }
}
