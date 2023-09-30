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

            endpoints.MapPost("/", CompleteChatAsync);
            endpoints.MapPut("/{chatId}", EditChatAsync);
            endpoints.MapDelete("/{chatId}", DeleteChatAsync);
            endpoints.MapGet("/{chatId}", GetChatAsync);
            endpoints.MapGet("/", GetChatsAsync);
        }

        public IAsyncEnumerable<ChatCompletionModel> CompleteChatAsync([FromServices] IChatService chatService, [FromBody] ChatCompletionForm form)
        {
            return chatService.CompleteChatAsync(form);
        }

        public async Task<IResult> EditChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId, [FromBody] EditChatForm form)
        {
            form.Id = chatId;
            return Results.Ok(await chatService.EditChatAsync(form));
        }

        public async Task<IResult> DeleteChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {
            var form = new DeleteChatForm() { Id = chatId };
            await chatService.DeleteChatAsync(form);
            return Results.Ok();
        }

        public async Task<IResult> GetChatAsync([FromServices] IChatService chatService, [FromRoute] long chatId)
        {
            var form = new GetChatForm() { Id = chatId };
            return Results.Ok(await chatService.GetChatAsync(form));
        }

        public async Task<IResult> GetChatsAsync([FromServices] IChatService chatService, [AsParameters] ChatSearchCriteria searchCriteria, [FromQuery] long offset = 0, [FromQuery] int limit = 25)
        {
            return Results.Ok(await chatService.GetChatsAsync(searchCriteria, offset, limit));
        }
    }
}