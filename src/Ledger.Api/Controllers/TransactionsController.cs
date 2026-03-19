using Ledger.Application.Common;
using Ledger.Application.Transactions.Commands;
using Ledger.Application.Transactions.DTOs;
using Ledger.Application.Transactions.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Ledger.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TransactionsController(IMediator mediator) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PaginatedResult<TransactionDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? accountId,
        [FromQuery] string? category,
        [FromQuery] DateTimeOffset? from,
        [FromQuery] DateTimeOffset? to,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25,
        CancellationToken ct = default)
    {
        var filter = new TransactionFilterDto(accountId, category, from, to, search, page, pageSize);
        var result = await mediator.Send(new GetTransactionsQuery(filter), ct);
        return Ok(ApiResponse<PaginatedResult<TransactionDto>>.Ok(result));
    }

    [HttpPost]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status201Created)]
    public async Task<IActionResult> Create([FromBody] CreateTransactionCommand command, CancellationToken ct)
    {
        var result = await mediator.Send(command, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<TransactionDto>.Ok(result));
    }

    [HttpPatch("{id:guid}/category")]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<TransactionDto>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCategory(
        Guid id,
        [FromBody] UpdateCategoryRequest body,
        CancellationToken ct)
    {
        var result = await mediator.Send(new UpdateTransactionCategoryCommand(id, body.Category), ct);
        return Ok(ApiResponse<TransactionDto>.Ok(result));
    }
}

public record UpdateCategoryRequest(string Category);
