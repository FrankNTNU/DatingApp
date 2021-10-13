using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class MessageParams : PaginationParams
    {
        public string Username { get; set; }
        /// <summary>
        /// Inbox / Outbox / Unread
        /// </summary>
        public string Container { get; set; } = "Unread";
        
    }
}