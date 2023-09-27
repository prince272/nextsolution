using Microsoft.AspNetCore.Mvc;
using NextSolution.Core.Models.Chats;
using NextSolution.Core.Services;

namespace NextSolution.WebApi.Endpoints
{
    public class ChatEndpoints : Shared.Endpoints
    {
        public ChatEndpoints(IEndpointRouteBuilder endpointRouteBuilder)
            : base(endpointRouteBuilder)
        {
        }

        public override void Configure()
        {
            var endpoints = MapGroup("/chats");

            endpoints.MapPost("/", CreateChatAsync);
            endpoints.MapPut("/{chatId}", EditChatAsync);
            endpoints.MapDelete("/{chatId}", DeleteChatAsync);
            endpoints.MapGet("/{chatId}", GetChatAsync);
            endpoints.MapGet("/", GetManyChatsAsync);
        }

        public async Task<IResult> CreateChatAsync([FromServices] IChatService chatService, [FromBody] CreateChatForm form)
        {
            return Results.Ok(await chatService.CreateAsync(form));
        }

        public async Task<IResult> EditChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId, [FromBody] EditChatForm form)
        {
            form.Id = chatId;
            return Results.Ok(await chatService.EditAsync(form));
        }

        public async Task<IResult> DeleteChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {
            var form = new DeleteChatForm() { Id = chatId };
            await chatService.DeleteAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> GetChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {
            var form = new GetChatForm() { Id = chatId };
            return Results.Ok(await chatService.GetAsync(form));
        }

        public async Task<IResult> GetManyChatsAsync([FromServices] IChatService chatService, [AsParameters] ChatSearchParams searchParams, [FromQuery] long offset = 0, [FromQuery] int limit = 25)
        {
            return Results.Ok(await chatService.GetManyAsync(searchParams, offset, limit));
        }
    }
}