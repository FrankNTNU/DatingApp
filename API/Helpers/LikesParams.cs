using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace API.Helpers
{
    public class LikesParams : PaginationParams
    {
        public int UserId { get; set; }
        /// <summary>
        /// liked / likedBy
        /// </summary>
        public string Predicate { get; set; }
    }
}