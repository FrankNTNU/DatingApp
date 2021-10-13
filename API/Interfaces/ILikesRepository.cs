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
        /// <summary>
        /// Get the liked users of a particular user.
        /// </summary>
        /// <param name="userId">The Id of the current user.</param>
        /// <returns>A list of liked users.</returns>
        Task<AppUser> GetUserWithLikes(int userId);
  
        /// <summary>
        /// Get a list of users either (1) (likeParams.predicate = "liked") whom a user has liked or (2) (predicate = "likedBy") who have liked a particular user. 
        /// </summary>
        /// <param name="likedParams">Parameters of type LikeParams.</param>
        /// <returns>A paginated list of LikeDtos.</returns>
        Task<PagedList<LikeDto>> GetUserLikes(LikesParams likedParams);
    }
}