using Api.Models;
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
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            var t = await _context.Users.AddAsync(dbUser);
            await _context.SaveChangesAsync();

            return t.Entity.Id;
        }
        public async Task ChangePassword(string newPassword,UserModel user)
        {
            var contextUser = await GetUserById(user.Id);

            if (contextUser != null)
            {
                var change = _mapper.Map<DAL.Entities.User>(contextUser);

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
            return await _context.Users.AsNoTracking().ProjectTo<UserModel>(_mapper.ConfigurationProvider).ToListAsync();
        }

        private async Task<DAL.Entities.User> GetUserById(Guid id)
        {
            var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == id);
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
        private async Task<DAL.Entities.User> GetUserByCredentials(string login, string password)
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

        private TokenModel GenerateTokens(DAL.Entities.UserSession session)
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

        public async Task<UserSession> GetSessinById(Guid id)
        {
            var session = await _context.UserSessions.FirstOrDefaultAsync(x => x.Id == id);
            if(session == null)
            {
                throw new Exception("Session not found");
            }    
            return session;
        }

        private async Task<UserSession> GetSessinByRefreshToken(Guid id)
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
                var session = await GetSessinByRefreshToken(refreshId);
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

        public async Task<AttachModel> GetUserAvatar(Guid userId)
        {
            var user = await _context.Users.Include(x => x.Avatar).FirstOrDefaultAsync(x => x.Id == userId);
            if (user != null)
            {
                var attach = _mapper.Map<AttachModel>(user.Avatar);
                return attach;
            }
            else
            {
                throw new Exception("User is null");
            }
        }

        
    }
}
