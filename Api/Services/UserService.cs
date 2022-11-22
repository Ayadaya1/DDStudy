using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using DAL;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Api.Configs;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using DAL.Entities;
using Common.Enums;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Api.Models.Attaches;
using Api.Models.User;
using Api.Models.Auth;
using Common.Constants;

namespace Api.Services
{
    public class UserService
    {
        private readonly IMapper _mapper;
        private readonly DAL.DataContext _context;
        private readonly AuthConfig _config;

        public UserService(IMapper mapper, DataContext context, IOptions<AuthConfig> config)
        {
            _mapper = mapper;
            _context = context;
            _config = config.Value;
        }

        public async Task<bool> CheckUserExists(string email)
        {
            return await _context.Users.AnyAsync(x => x.Email.ToLower() == email.ToLower());
        }

        public async Task<Guid> CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<User>(model);
            var t = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();

            return t.Entity.Id;
        }
        public async Task ChangePassword(string newPassword,UserModel user)
        {
            var contextUser = await GetUserById(user.Id);

            if (contextUser != null)
            {
                var change = _mapper.Map<User>(contextUser);

                change.PasswordHash = HashHelper.GetHash(newPassword);
                _context.Update(change);

                await _context.SaveChangesAsync();
                /*contextUser.PasswordHash = HashHelper.GetHash(model.NewPassword);
                var push = _mapper.Map<DAL.Entities.User>(contextUser);
                await _context.Users.AddAsync(push);
                await _context.SaveChangesAsync();*/
            }
        }
        public async Task<List<UserModel>> GetUsers()
        {
            return _mapper.Map<List<UserModel>>(await _context.Users.Include(x=>x.Avatar).Include(x=>x.Subscribers).Include(x=>x.Subscriptions).AsNoTracking().ToListAsync());
        }

        private async Task<User> GetUserById(Guid id)
        {
            var user = await _context.Users.Include(x=>x.Avatar).Include(x=>x.Subscribers).Include(x=>x.Subscriptions).FirstOrDefaultAsync(x => x.Id == id);
            if (user == null)
            {
                throw new Exception("User not found");
            }
            return user;
        }
        public async Task<UserModel> GetUser(Guid id)
        {
            var user = await GetUserById(id);
           // if (user.Posts.Count == 0)
                //throw new Exception("No posts...");
            return _mapper.Map<UserModel>(user);
        }
        private async Task<User> GetUserByCredentials(string login, string password)
        {
            var user = await  _context.Users.FirstOrDefaultAsync(x => x.Email.ToLower() == login.ToLower());
            if(user == null)
            {
                throw new Exception("User not found");
            }
            if (!HashHelper.Verify(password, user.PasswordHash))
                throw new Exception("Wrong password");
            return user;
        }

        private TokenModel GenerateTokens(UserSession session)
        {
            var dtNow = DateTime.Now;
            if (session.User == null)
            {
                throw new Exception("magic");
            }
            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[] {
            new Claim("name", session.User.Name),
            new Claim("id",session.User.Id.ToString()),
            new Claim("sessionId",session.Id.ToString())
            },
                expires: dtNow.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[] {
            new Claim("refreshToken",session.RefreshToken.ToString())
            },
                expires: dtNow.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);
            return new TokenModel
            {
                AccessToken = encodedJwt,
                RefreshToken = encodedRefresh
            };
        }

        public async Task<UserSession> GetSessionById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if(session == null)
            {
                throw new Exception("Session not found");
            }    
            return session;
        }

        private async Task<UserSession> GetSessionByRefreshToken(Guid id)
        {
            var session = await _context.UserSessions.Include(x=>x.User).FirstOrDefaultAsync(x => x.RefreshToken == id);
            if (session == null)
            {
                throw new Exception("Session not found");
            }
            return session;
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredentials(login, password);

            var session = await _context.AddAsync(new DAL.Entities.UserSession { Created = DateTime.UtcNow, Id = Guid.NewGuid(), RefreshToken = Guid.NewGuid(), User = user });

            await _context.SaveChangesAsync();

            return GenerateTokens(session.Entity);
        }

        public async Task<TokenModel> GetTokenByRefreshToken(string refreshToken)
        {
            var validParam = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                ValidateLifetime = true,
                IssuerSigningKey = _config.SymmetricSecurityKey()
            };
            var principal = new JwtSecurityTokenHandler().ValidateToken(refreshToken, validParam, out var securityToken);

            if (securityToken is not JwtSecurityToken jwtToken || !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256,
                StringComparison.InvariantCultureIgnoreCase))
            {
                throw new SecurityTokenException("Invalid token");
            }

            if (principal.Claims.FirstOrDefault(x => x.Type == "refreshToken")?.Value is String refreshIdString && Guid.TryParse(refreshIdString, out var refreshId))
            {
                var session = await GetSessionByRefreshToken(refreshId);
                if(!session.IsActive)
                {
                    throw new Exception("Session is not active");
                }
                var user = session.User;
                session.RefreshToken = Guid.NewGuid();
                await _context.SaveChangesAsync();

                return GenerateTokens(session);
            }
            else
            {
                throw new SecurityTokenException("Invalid token");
            }
        }

        public async Task AddAvatarToUser(Guid userId, MetadataModel meta, string filePath)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user == null)
            {
                throw new Exception("The user is null");
            }
            else
            {
                var avatar = new Avatar
                {
                    //Id = Guid.NewGuid(),
                    Author = user,
                    Mimetype = meta.MimeType,
                    Name = meta.Name,
                    Size = meta.Size,
                    FilePath = filePath
                };
                user.Avatar = avatar;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<AttachModel> GetUserAvatar(Guid userId, Guid currentUserId)
        {
            var user = await _context.Users.Include(x => x.Avatar).Include(x=>x.PrivacySettings).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                AttachModel attach;
                if (user.PrivacySettings.AvatarAccess == Privacy.Everybody || await CheckSubscription(currentUserId, userId))
                    attach = _mapper.Map<AttachModel>(user.Avatar);
                else
                    attach = _mapper.Map < AttachModel > (await GetAttachById(DefaultResources.DefaultAvatarId));
                //Если пользователь не подписан на того, кто требует подписку для просмотра его аватара, то вместо аватара он видит "стандартный" аватар, предзагруженный на сервер.
                return attach!;
            }
            else
            {
                throw new Exception("User is null");
            }
        }

        public async Task Subscribe(Guid subscriberId, Guid targetId)
        {
            var subscriber = await _context.Users.Include(x => x.Subscriptions).FirstOrDefaultAsync(x => x.Id == subscriberId);
            var target = await _context.Users.Include(x => x.Subscribers).ThenInclude(y => y.Subscriber).FirstOrDefaultAsync(x => x.Id == targetId);
            if (target != null && subscriber != null)
            {
                if (target.Subscribers.FirstOrDefault(x => x.Subscriber.Id == subscriber.Id) == null)
                {
                    var subscription = new Subscription
                    {
                        Id = Guid.NewGuid(),
                        Subscriber = subscriber,
                        Target = target
                    };
                    target.Subscribers.Add(subscription);
                    subscriber.Subscriptions.Add(subscription);
                    await _context.Subscriptions.AddAsync(subscription);
                    await _context.SaveChangesAsync();
                }
                else
                    throw new Exception("Already subscribed to this user");
            }
            else
                throw new Exception("One of the users could't be found");
        }

        public async Task<List<UserModel>>GetUsersYouMightLike(Guid userId)
        {
            List<UserModel> users = new List<UserModel>();
            var user = await _context.Users.Include(x => x.Subscribers).Include(x => x.Subscriptions).ThenInclude(x => x.Target).ThenInclude(x => x.Subscriptions).ThenInclude(x => x.Target).FirstOrDefaultAsync(x => x.Id == userId);
            
            var a = user.Subscriptions.Select(x => x.Target.Subscriptions.Select(x=>x.Target).ToList());
            return _mapper.Map<List<UserModel>>(a);
        }

        public async Task<List<UserModel>> GetSubs(Guid userId)
        {
            var user = await _context.Users
                .Include(x => x.Subscriptions)
                .ThenInclude(y=>y.Target)
                .ThenInclude(z=>z.Subscribers)
                .Include(x=>x.Subscriptions)
                .ThenInclude(y=>y.Target)
                .ThenInclude(z=>z.Subscriptions)
                .Include(x=>x.Subscriptions)
                .ThenInclude(y=>y.Target)
                .ThenInclude(z=>z.Avatar)
                .FirstOrDefaultAsync(x => x.Id == userId);
            var subs = user!.Subscriptions.Select(x => x.Target).ToList();
            return _mapper.Map<List<UserModel>>(subs);
        }

        public async Task<List<UserModel>> GetSubbers(Guid userId)
        {
            var user = await _context.Users.Include(x=>x.Subscriptions)
                .Include(x => x.Subscribers)
                .ThenInclude(y => y.Subscriber)
                .ThenInclude(z=>z.Subscribers)
                .Include(x=>x.Subscribers)
                .ThenInclude(y=>y.Subscriber)
                .ThenInclude(z=>z.Subscriptions)
                .Include(x=>x.Subscribers)
                .ThenInclude(y=>y.Subscriber)
                .ThenInclude(z=>z.Avatar)
                .FirstOrDefaultAsync(x => x.Id == userId);
            var subbers = user!.Subscribers.Select(x => x.Subscriber).ToList();
            return _mapper.Map<List<UserModel>>(subbers);
        }

        public async Task<bool> CheckSubscription(Guid subscriberId, Guid targetId)
        {
            var subscriber = await _context.Users.Include(x => x.Subscriptions).FirstOrDefaultAsync(x => x.Id == subscriberId);
            var target = await _context.Users.Include(x => x.Subscribers).ThenInclude(y => y.Subscriber).FirstOrDefaultAsync(x => x.Id == targetId);
            if ( target == null)
                throw new Exception("One of the users is null");
            if (subscriberId == Guid.Empty||target.Subscribers.FirstOrDefault(x => x.Subscriber.Id == subscriber.Id) == null)
                return false;
            return true;
        }

        public async Task ChangePrivacySettings(Guid userId, ChangePrivacySettingsModel model)
        {
            var settings = await _context.PrivacySettings.Include(x => x.User).FirstOrDefaultAsync(x => x.UserId == userId);
            if (settings != null)
            {
                if (model.Validate())
                {
                    settings.AvatarAccess = model.AvatarAccess;
                    settings.PostAccess = model.PostAccess;
                    settings.MessageAccess = model.MessageAccess;
                    settings.CommentAccess = model.CommentAccess;
                    _context.Update(settings);
                }
                else
                    throw new Exception("Invalid privacy settings");
            }
            else //У каждого юзера должны быть настройки приватности, поэтому, если их по какой-то причине нет - мы их создаём.
            {
                var user = await _context.Users.Include(x => x.PrivacySettings).FirstOrDefaultAsync(x => x.Id == userId);
                if(user!=null)
                {
                    settings = new PrivacySettings()
                    {
                        User = user,
                        UserId = user.Id
                    };

                    await _context.PrivacySettings.AddAsync(settings);
                    user.PrivacySettings = settings;
                    await _context.SaveChangesAsync();
                    
                }
                else
                {
                    throw new Exception("Both user and his privacy settings haven't been found");
                }
            }
            await _context.SaveChangesAsync();
        }
        public async Task Unsubscribe(Guid userId, Guid targetId)
        {
            var sub = await _context.Subscriptions.Include(x => x.Subscriber).Include(x => x.Target).FirstOrDefaultAsync(x => x.Subscriber.Id == userId && x.Target.Id == targetId);
            if (sub == null)
                throw new Exception("Can't find the subscription");


            _context.Remove(sub);
            await _context.SaveChangesAsync();
        }
        public async Task<AttachModel> GetAttachById(Guid id)
        {
            var attach = await _context.Attaches.FirstOrDefaultAsync(x => x.Id == id);
            if (attach == null)
            {
                throw new Exception("Attach is null");
            }
            var mappedAttach = _mapper.Map<AttachModel>(attach);
            return mappedAttach;

        }

    }
}
