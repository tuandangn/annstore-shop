using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Annstore.Application.Infrastructure;
using Annstore.Application.Models.Customers;
using Annstore.Auth.Entities;
using Annstore.Application.Infrastructure.Messages;

namespace Annstore.Application.Services.Customers
{
    [Serializable]
    public sealed class AccountService : IAccountService
    {
        private readonly SignInManager<Account> _signInManager;
        private readonly UserManager<Account> _userManager;

        public AccountService(UserManager<Account> userManager, SignInManager<Account> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<AppResponse<Account>> LoginAsync(AppRequest<LoginModel> request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            var loginModel = request.Data;
            var user = await _userManager.FindByEmailAsync(loginModel.Email).ConfigureAwait(false);
            if (user == null || !await _userManager.CheckPasswordAsync(user, loginModel.Password))
                return AppResponse.ErrorResult<Account>(PublicMessages.User.AccountOrPasswordIsIncorrect);

            //*TODO*
            var lockOnFailure = false;
            var signInResult = await _signInManager.PasswordSignInAsync(user, loginModel.Password, loginModel.Remember, lockOnFailure).ConfigureAwait(false);

            if (signInResult.Succeeded)
                return AppResponse.SuccessResult(user);
            return AppResponse.ErrorResult<Account>(PublicMessages.User.SignInFailed);
        }
    }
}
