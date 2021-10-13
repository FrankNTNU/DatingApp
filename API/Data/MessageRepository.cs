using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class MessageRepository : IMessageRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;
        public MessageRepository(DataContext context, IMapper mapper)
        {
            _mapper = mapper;
            _context = context;
        }

        public void AddGroup(Group group)
        {
            _context.Groups.Add(group);
        }

        public void AddMessage(Message message)
        {
            _context.Messages.Add(message);
        }

        public void DeleteMessage(Message message)
        {
            _context.Messages.Remove(message);
        }

        public async Task<Connection> GetConnection(string connectionId)
        {
            return await _context.Connections.FindAsync(connectionId);
        }
        // get the group and its connections
        public async Task<Group> GetGroupForConnection(string connectionId)
        {
            return await _context.Groups.Include(c => c.Connections)
                                        .Where(c => c.Connections.Any(x => x.ConnectionId == connectionId))
                                        .FirstOrDefaultAsync();
        }

        public async Task<Message> GetMessage(int Id)
        {
            return await _context.Messages
                .Include(u => u.Sender)
                .Include(u => u.Recipient)
                .SingleOrDefaultAsync(x => x.Id == Id);
        }

        public async Task<Group> GetMessageGroup(string groupName)
        {
            return await _context.Groups.Include(x => x.Connections)
                                        .FirstOrDefaultAsync(x => x.Name == groupName);
        }

        public async Task<PagedList<MessageDto>> GetMessagesForUser(MessageParams messageParams)
        {
            var query = _context.Messages
                .OrderByDescending(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .AsQueryable();

            query = messageParams.Container switch
            {
                "Inbox" => query.Where(u => u.RecipientUsername == messageParams.Username
                                            && !u.RecipientDeleted),
                "Outbox" => query.Where(u => u.SenderUsername == messageParams.Username
                                             && !u.SenderDeleted),
                _ => query.Where(u => u.RecipientUsername == messageParams.Username
                                      && u.DateRead == null
                                      && u.RecipientDeleted) // "Unread" case
            };
            
            return await PagedList<MessageDto>.CreateAsync(query, messageParams.PageNumber, messageParams.PageSize);
        }
        // get the conversation (the current user and the other person)
        public async Task<IEnumerable<MessageDto>> GetMessageThread(string currentUsername, string recipientUsername)
        {
            // get the messages (and their photos) between two users
            var messages = await _context.Messages
                .Where(m =>
                    // get the messages only for the current user AND that the current user has not deleted
                    m.Recipient.UserName == currentUsername && !m.RecipientDeleted &&
                     // and get the messages that have been sent by the other person (recipient)
                     m.SenderUsername == recipientUsername ||
                    // or messages that have been sent by the current user AND that the current user has not deleted 
                    m.Sender.UserName == currentUsername && !m.SenderDeleted &&
                    // and get the messages that are for the other person (recipient)
                    m.Recipient.UserName == recipientUsername
                )
                .OrderBy(m => m.MessageSent)
                .ProjectTo<MessageDto>(_mapper.ConfigurationProvider)
                .ToListAsync();

            // mark the unread messages that are for this current user
            var unreadMessages = messages
                // only mark the messages that are for the current user
                .Where(m => m.DateRead == null && m.RecipientUsername == currentUsername)
                .ToList();
            // check to see if there is any unread messages
            if (unreadMessages.Any())
            {
                // loop through those unread messages
                foreach (var message in unreadMessages)
                {
                    message.DateRead = DateTime.UtcNow; // mark the local date time
                }
            }
            return messages;
        }

        public void RemoveConnection(Connection connection)
        {
            _context.Connections.Remove(connection);
        }
    }
}