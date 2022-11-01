using Api.Models;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using Common;
using System;
using DAL;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Api.Configs;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using Microsoft.IdentityModel.Tokens;
using System.Data;

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

        public async Task CreateUser(CreateUserModel model)
        {
            var dbUser = _mapper.Map<DAL.Entities.User>(model);
            await _context.Users.AddAsync(dbUser);

            await _context.SaveChangesAsync();
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

        private TokenModel GenerateTokens(DAL.Entities.User user)
        {
            var dtNow = DateTime.Now;

            var jwt = new JwtSecurityToken(
                issuer: _config.Issuer,
                audience: _config.Audience,
                notBefore: dtNow,
                claims: new Claim[] {
            new Claim("displayName", user.Name),
            new Claim("id",user.Id.ToString())
            },
                expires: dtNow.AddMinutes(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);

            var refresh = new JwtSecurityToken(
                notBefore: dtNow,
                claims: new Claim[] {
            new Claim("id",user.Id.ToString())
            },
                expires: dtNow.AddHours(_config.LifeTime),
                signingCredentials: new SigningCredentials(_config.SymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
                );
            var encodedRefresh = new JwtSecurityTokenHandler().WriteToken(refresh);

            return new TokenModel(encodedJwt, encodedRefresh);
        }

        public async Task<TokenModel> GetToken(string login, string password)
        {
            var user = await GetUserByCredentials(login, password);

            return GenerateTokens(user);
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

            if (principal.Claims.FirstOrDefault(x => x.Type == "id")?.Value is String userIdString && Guid.TryParse(userIdString, out var userId))
            {
                var user = await GetUserById(userId);
                return GenerateTokens(user);
            }
            else
            {
                throw new SecurityTokenException("Invalid token");
            }
        }
    }
}
