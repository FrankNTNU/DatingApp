using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Extensions;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.SignalR;

namespace API.SignalR
{
    public class MessageHub : Hub
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IHubContext<PresenceHub> _presenceHub;
        private readonly PresenceTracker _tracker;

        public MessageHub(IUnitOfWork unitOfWork,
                          IMapper mapper,
                          IHubContext<PresenceHub> presenceHub,
                          PresenceTracker tracker)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _presenceHub = presenceHub;
            _tracker = tracker;
        }
        public override async Task OnConnectedAsync()
        {
            var httpContext = Context.GetHttpContext();
            // get the other user's name from the query string
            var otherUser = httpContext.Request.Query["user"].ToString();
            // create a group name by concatinating the current user and the other person's name
            var groupName = GetGroupName(Context.User.GetUsername(), otherUser);
            // and to the group specifying the connection Id and the group name
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            // add the group to the db
            var group = await AddToGroup(groupName);
            // invoke the method "UpdatedGroup" which only involved in certain group
            await Clients.Group(groupName).SendAsync("UpdatedGroup", group);
            // get the messages (conversation)
            var messages = await _unitOfWork.MessageRepository
            .GetMessageThread(Context.User.GetUsername(), otherUser);
            if (_unitOfWork.HasChanges()) await _unitOfWork.Complete();
            // only send the message to the caller
            await Clients.Caller.SendAsync("ReceiveMessageThread", messages);
        }
        public override async Task OnDisconnectedAsync(Exception exception)
        {
            var group = await RemoveFromMessageGroup();
            await Clients.Group(group.Name).SendAsync("UpdatedGroup", group);
            await base.OnDisconnectedAsync(exception);
        }
        public async Task SendMessage(CreateMessageDto createMessageDto)
        {
            var username = Context.User.GetUsername();
            if (username == createMessageDto.RecipientUsername.ToLower())
                throw new HubException("You cannot send messages to yourself.");

            var sender = await _unitOfWork.UserRepository.GetUserByUsernameAsync(username);

            var recipient = await _unitOfWork.UserRepository.GetUserByUsernameAsync(createMessageDto.RecipientUsername);
            if (recipient == null) throw new HubException("Not found user");

            var message = new Message
            {
                Sender = sender,
                Recipient = recipient,
                SenderUsername = sender.UserName,
                RecipientUsername = recipient.UserName,
                Content = createMessageDto.Content
            };
            var groupName = GetGroupName(sender.UserName, recipient.UserName);
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            if (group.Connections.Any(x => x.Username == recipient.UserName))
            {
                message.DateRead = DateTime.UtcNow;
            }
            else
            {
                var connections = await _tracker.GetConnectionsForUser(recipient.UserName);
                if (connections != null) // online but not in message group
                {
                    await _presenceHub.Clients
                        .Clients(connections)
                        .SendAsync("NewMessageReceived",
                                   new { username = sender.UserName, knownAs = sender.KnownAs });
                }
            }
            _unitOfWork.MessageRepository.AddMessage(message);

            if (await _unitOfWork.Complete())
            {
                await Clients.Group(groupName).SendAsync("NewMessage", _mapper.Map<MessageDto>(message));
            }
        }
        private string GetGroupName(string caller, string other)
        {
            var stringCompare = string.CompareOrdinal(caller, other) < 0;
            return stringCompare ? $"{caller}-{other}" : $"{other}-{caller}";
        }
        // add a new group by a group name to the database
        private async Task<Group> AddToGroup(string groupName)
        {
            var group = await _unitOfWork.MessageRepository.GetMessageGroup(groupName);
            var connection = new Connection(Context.ConnectionId, Context.User.GetUsername());
            if (group == null)
            {
                group = new Group(groupName);
                _unitOfWork.MessageRepository.AddGroup(group);
            }
            group.Connections.Add(connection);
            if (await _unitOfWork.Complete()) return group;

            throw new HubException("Failed to join group.");
        }
        private async Task<Group> RemoveFromMessageGroup()
        {
            // get the group and its connections
            var group = await _unitOfWork.MessageRepository.GetGroupForConnection(Context.ConnectionId);
            // get the current connection for the current user
            var connection = group.Connections.FirstOrDefault(x => x.ConnectionId == Context.ConnectionId);
            // remove the connection on the current user's side
            _unitOfWork.MessageRepository.RemoveConnection(connection);
            if (await _unitOfWork.Complete()) return group;
            throw new HubException("Failed to remove from group.");
        }
    }
}