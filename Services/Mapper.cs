using FiwFriends.Models;
using FiwFriends.DTOs;
using FiwFriends.Data;
using System.Diagnostics.CodeAnalysis;

namespace FiwFriends.Services;

public class MapperService{
    private readonly ApplicationDBContext _db;
    private readonly CurrentUserService _currentUser;
    public MapperService(ApplicationDBContext db, CurrentUserService currentUser){
        _db = db;
        _currentUser = currentUser;
    }
    
    async public Task<T> MapAsync<TSource, T>(TSource source){
        // Example mapping logic for different types of DTOs to Models
        if (typeof(T) == typeof(Post)){
            var postDTO = source as PostDTO ?? throw new InvalidCastException($"Mapping failed: {typeof(TSource)} is not PostDTO");;
            return (T)(object) await DTO2Post(postDTO);
        } 
        else if (typeof(T) == typeof(IndexPost)){
            var post = source as Post ?? throw new InvalidCastException($"Mapping failed: {typeof(TSource)} is not Post");;
            return (T)(object) await Post2Index(post);
        }
        else if (typeof(T) == typeof(DetailPost)){
            var post = source as Post ?? throw new InvalidCastException($"Mapping failed: {typeof(TSource)} is not Post");;
            return (T)(object) await Post2Detail(post);
        }
        else
        {
            throw new ArgumentException("Unsupported type", nameof(T));
        }
    }

    async private Task<Post> DTO2Post([NotNull] PostDTO post){
        var user = await _currentUser.GetCurrentUser();
        if ( user == null ){ throw new Exception("Just for not warning in MapAsync<PostDTO, Post>"); }
        return new Post {
            Activity = post.Activity,   
            Description = post.Description,
            Location = post.Location,
            ExpiredTime = post.ExpiredTime.ToUniversalTime(),
            AppointmentTime = post.AppointmentTime.ToUniversalTime(),
            OwnerId = user.Id,
            Tags = _db.Tags.Where(t => post.Tags.Select( dto => dto.Name.ToLower() ).Contains(t.Name.ToLower())).ToList(),   //Attach Tags
            Questions = post.Questions.Where( q => q.Content != null).Select(q => new Question{
                Content = q.Content ?? throw new Exception("Strange Content in Question, Look for mapper")
            }).ToList(),
            Limit = post.Limit
        };
    }

    async private Task<IndexPost> Post2Index(Post post){
        var user = await _currentUser.GetCurrentUser();

        return new IndexPost{
            PostId = post.PostId,
            Activity = post.Activity,
            Description = post.Description,
            Location = post.Location,
            AppointmentTime = post.AppointmentTime.ToLocalTime(),
            Owner = await _db.Users.FindAsync(post.OwnerId) ?? throw new Exception("Just for not warning in MapAsync<Post, IndexPost>"),
            ParticipantsCount = post.Participants.Count(),
            IsFav = post.FavoritedBy.Any(u => u.Id == user?.Id),
            Tags = post.Tags,
            Limit = post.Limit
        };
    }

    async private Task<DetailPost> Post2Detail(Post post){
        var user = await _currentUser.GetCurrentUser();
        return new DetailPost{
            PostId = post.PostId,
            Activity = post.Activity,
            Description = post.Description,
            Location = post.Location,
            AppointmentTime = post.AppointmentTime.ToLocalTime(),
            ExpiredTime = post.ExpiredTime.ToLocalTime(),
            Owner = await _db.Users.FindAsync(post.OwnerId) ?? throw new Exception("Just for not warning in MapAsync<Post, DetailPost>"),
            ParticipantsCount = post.Participants.Count(),
            IsFav = post.FavoritedBy.Any(u => u.Id == user?.Id),
            Tags = post.Tags,
            Participants = post.Participants.Select(j => j.User),
            Questions = post.Questions,
            Limit = post.Limit
        };
    }

}