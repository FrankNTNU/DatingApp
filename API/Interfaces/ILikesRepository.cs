using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.DTOs;
using API.Entities;
using API.Helpers;

namespace API.Interfaces
{
    public interface ILikesRepository
    {
        Task<UserLike> GetUserLike(int sourceUserId, int likedUserId);
        Task<AppUser> GetUserWithLikes(int userId);
        /// <summary> Get a user and a list of users he has liked. </summary>
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likedParams);
    }
}