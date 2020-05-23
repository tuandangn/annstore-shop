using Annstore.Core.Entities.Users;
using Annstore.Web.Infrastructure;
using Annstore.Web.Models.Accounts;
using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace Annstore.Web.Services.Accounts
{
    [Serializable]
    public sealed class AccountService : IAccountService
    {
        private readonly SignInManager<AppUser> _signInManager;
        private readonly UserManager<AppUser> _userManager;

        public AccountService(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppResponse<AppUser>> LoginAsync(AppRequest<LoginModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var loginModel = request.Data;
            var user = await _userManager.FindByEmailAsync(loginModel.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
                return AppResponse.ErrorResult<AppUser>(Messages.User.AccountOrPasswordIsIncorrect);

            //*TODO*
            var lockOnFailure = false;
            var signInResult = await _signInManager.PasswordSignInAsync(user, loginModel.Password, loginModel.Remember, lockOnFailure);

            if(signInResult.Succeeded)
                return AppResponse.SuccessResult(user);
            return AppResponse.ErrorResult<AppUser>(Messages.User.SignInFailed);
        }
    }
}
