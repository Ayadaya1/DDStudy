using DAL;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

namespace Api
{
    public class TokenValidator : ISecurityTokenValidator
    {
        bool ISecurityTokenValidator.CanValidateToken => true;

        private int _maximumTokenSizeInBytes;
        int ISecurityTokenValidator.MaximumTokenSizeInBytes { get => _maximumTokenSizeInBytes; set => _maximumTokenSizeInBytes = value; }

        bool ISecurityTokenValidator.CanReadToken(string securityToken) => true;

        ClaimsPrincipal ISecurityTokenValidator.ValidateToken(string securityToken, TokenValidationParameters validationParameters, out SecurityToken validatedToken)
        {
            throw new NotImplementedException();
        }
    }
}
