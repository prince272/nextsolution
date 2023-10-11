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

            endpoints.MapPost("/", AddChatAsync)
                .RequireAuthorization();

            endpoints.MapPut("/{chatId}", EditChatAsync)
                .RequireAuthorization();

            endpoints.MapDelete("/{chatId}", DeleteChatAsync)
                .RequireAuthorization();

            endpoints.MapDelete("/", DeleteAllChatsAsync)
                .RequireAuthorization();

            endpoints.MapGet("/{chatId}", GetChatAsync)
                .RequireAuthorization();

            endpoints.MapGet("/", GetChatsAsync)
                .RequireAuthorization();

            endpoints.MapGet("/{chatId}/messages", GetMessagesAsync)
                .RequireAuthorization();
        }

        public IAsyncEnumerable<ChatStreamModel> AddChatAsync([FromServices] IChatService chatService, [FromBody] AddChatForm form)
        {
            return chatService.AddChatAsync(form);
        }

        public async Task<IResult> EditChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId, [FromBody] EditChatForm form)
        {
            return Results.Ok(await chatService.EditChatAsync(chatId, form));
        }

        public async Task<IResult> DeleteChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {      
            await chatService.DeleteChatAsync(chatId);
            return Results.Ok();
        }

        public async Task<IResult> DeleteAllChatsAsync([FromServices] IChatService chatService)
        {
            await chatService.DeleteAllChatsAsync();
            return Results.Ok();
        }

        public async Task<IResult> GetChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {
            return Results.Ok(await chatService.GetChatAsync(chatId));
        }

        public async Task<IResult> GetChatsAsync([FromServices] IChatService chatService, [AsParameters] ChatCriteria criteria, [FromQuery] long offset = 0, [FromQuery] int limit = 25)
        {
            return Results.Ok(await chatService.GetChatsAsync(criteria, offset, limit));
        }

        public async Task<IResult> GetMessagesAsync([FromServices] IChatService chatService, [FromRoute] long chatId, [AsParameters] ChatMessageCriteria criteria, [FromQuery] long offset = 0, [FromQuery] int limit = 25)
        {
            return Results.Ok(await chatService.GetMessagesAsync(chatId, criteria, offset, limit));
        }
    }
}