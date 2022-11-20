using aspnetserver.Auth.Model;
using aspnetserver.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Data;

namespace aspnetserver.Controllers;
[ApiController]
//[AllowAnonymous]
[Route("api")]
public class AuthController : ControllerBase
{

    private readonly UserManager<WarehouseRestUser> _userManager;
    private readonly IAuthorizationService _authorizationService;
    private readonly IJwtTokenService _jwtTokenService;

    public AuthController(UserManager<WarehouseRestUser> userManager, IJwtTokenService jwtTokenService, IAuthorizationService authorizationService)
    {
        _userManager = userManager;
        _authorizationService = authorizationService;
        _jwtTokenService = jwtTokenService;
    }
    [HttpPost]
    [Route("register")]
    public async Task<IActionResult> Register(RegisterUserDto registerUserDto)
    {
        var user = await _userManager.FindByNameAsync(registerUserDto.UserName);
        if (user != null)
            return BadRequest("Request invalid.");

        var newUser = new WarehouseRestUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.UserName
        };
        var createUserResult = await _userManager.CreateAsync(newUser, registerUserDto.Password);
        if (!createUserResult.Succeeded)
            return BadRequest("Could not create a user." + $"{createUserResult}");

        await _userManager.AddToRoleAsync(newUser, WarehouseRoles.Worker);

        return CreatedAtAction(nameof(Register), new UserDto(newUser.Id, newUser.UserName, newUser.Email));
    }

    [HttpPost]
    [Route("register/manager")]
    [Authorize(Roles = WarehouseRoles.Admin)]
    public async Task<IActionResult> RegisterManager(RegisterUserDto registerUserDto)
    {
        var user = await _userManager.FindByNameAsync(registerUserDto.UserName);
        if (user != null)
            return BadRequest("Request invalid.");

        var newUser = new WarehouseRestUser
        {
            Email = registerUserDto.Email,
            UserName = registerUserDto.UserName
        };
        var createUserResult = await _userManager.CreateAsync(newUser, registerUserDto.Password);
        if (!createUserResult.Succeeded)
            return BadRequest("Could not create a user." + $"{createUserResult}");

        await _userManager.AddToRoleAsync(newUser, WarehouseRoles.Manager);

        return CreatedAtAction(nameof(Register), new UserDto(newUser.Id, newUser.UserName, newUser.Email));
    }

    [HttpPost]
    [Route("login")]
    public async Task<IActionResult> Login(LoginDto loginDto)
    {
        var user = await _userManager.FindByNameAsync(loginDto.UserName);
        if (user == null)
            return BadRequest("User name or password is invalid.");

        var isPasswordValid = await _userManager.CheckPasswordAsync(user, loginDto.Password);
        if (!isPasswordValid)
            return BadRequest("User name or password is invalid.");

        // valid user
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenService.CreateAccessToken(user.UserName, user.Id, roles);

        //var roles = await _userManager.GetRolesAsync();
        return Ok(new SuccessfulLoginDto(user.Id, user.UserName, roles, accessToken));
    }
}
