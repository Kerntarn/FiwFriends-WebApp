using FiwFriends.Models;
using FiwFriends.DTOs;
using FiwFriends.Data;
using System.Diagnostics.CodeAnalysis;
using Microsoft.EntityFrameworkCore;

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
        else if (typeof(T) == typeof(IEnumerable<IndexPost>)){
            var post = source as IQueryable<Post> ?? throw new InvalidCastException($"Mapping failed: {typeof(TSource)} is not Post");;
            return (T)(object) await Post2Index(post);
        }
        else if (typeof(T) == typeof(DetailPost)){
            var post = source as IQueryable<Post> ?? throw new InvalidCastException($"Mapping failed: {typeof(TSource)} is not Post");;
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

    async private Task<IEnumerable<IndexPost>> Post2Index(IQueryable<Post> post){
        var user = await _currentUser.GetCurrentUser();
        var indexPost = await post
                                .Select(p => new IndexPost {
                                    PostId = p.PostId,
                                    Activity = p.Activity,
                                    Description = p.Description,
                                    Location = p.Location,
                                    AppointmentTime = p.AppointmentTime.ToLocalTime(),
                                    ExpiredTime = p.ExpiredTime.ToLocalTime(),
                                    Owner = p.Owner,
                                    ParticipantsCount = p.Participants.Count(),
                                    IsFav = p.FavoritedBy.Any(u => u.Id == user.Id),
                                    Tags = p.Tags,
                                    Limit = p.Limit
                                    })
                                .ToListAsync();
        return indexPost;
    }

    async private Task<DetailPost> Post2Detail(IQueryable<Post> post){
        var user = await _currentUser.GetCurrentUser();
        var detailPost = await post
                                .Select(p => new DetailPost {
                                    PostId = p.PostId,
                                    Activity = p.Activity,
                                    Description = p.Description,
                                    Location = p.Location,
                                    AppointmentTime = p.AppointmentTime.ToLocalTime(),
                                    ExpiredTime = p.ExpiredTime.ToLocalTime(),
                                    Owner = p.Owner,
                                    ParticipantsCount = p.Participants.Count(),
                                    IsFav = p.FavoritedBy.Any(u => u.Id == user.Id),
                                    Tags = p.Tags,
                                    Participants = p.Participants.Select(j => j.User),
                                    Questions = _db.Questions.Where(q => q.PostId == p.PostId).Include(q => q.Answers.Where(a => a.Form.UserId == user.Id)).ToList(),
                                    Limit = p.Limit,
                                    IsJoined = p.Participants.Any(j => j.UserId == user.Id) || p.Forms.Any(f => f.UserId == user.Id && f.PostId == p.PostId) || p.OwnerId == user.Id
                                })
                                .FirstOrDefaultAsync();
        return detailPost ?? throw new Exception("Not Found");
    }

}