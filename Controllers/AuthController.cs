using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using autoease_backend.Data.Models;
using autoease_backend.Models.DTOs;
using autoease_backend.Services;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.AspNetCore.Authorization;

namespace autoease_backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthController(UserManager<User> userManager, SignInManager<User> signInManager, IConfiguration configuration, IEmailService emailService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _emailService = emailService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.Phone,
                Role = "Customer" // Force Customer role for public registration
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(VerifyEmail), "Auth", new { token, email = user.Email }, Request.Scheme);

                string emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 40px auto; background: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); }}
                            .header {{ text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eeeeee; }}
                            .header h2 {{ color: #333333; margin: 0; }}
                            .content {{ padding: 20px 0; color: #555555; line-height: 1.6; text-align: center; }}
                            .btn {{ display: inline-block; padding: 12px 25px; margin: 20px 0; background-color: #4CAF50; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; transition: background-color 0.3s; }}
                            .btn:hover {{ background-color: #45a049; }}
                            .footer {{ text-align: center; font-size: 12px; color: #999999; padding-top: 20px; border-top: 1px solid #eeeeee; }}
                            .link-text {{ font-size: 12px; color: #007bff; word-break: break-all; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Welcome to AutoEase!</h2>
                            </div>
                            <div class='content'>
                                <p>Hi {user.Name},</p>
                                <p>Thank you for registering. Please confirm your email address to activate your account.</p>
                                <a href='{confirmationLink}' class='btn'>Confirm Email Address</a>
                                <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                <p class='link-text'>{confirmationLink}</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; {DateTime.UtcNow.Year} AutoEase. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(user.Email, "Confirm your email", emailBody);

                return Ok(new { message = "Registration successful. Please check your email to verify your account." });
            }

            return BadRequest(result.Errors);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("admin/register-user")]
        public async Task<IActionResult> RegisterByAdmin([FromBody] RegisterDto model)
        {
            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.Phone,
                Role = model.Role // Admin can set the role
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                var confirmationLink = Url.Action(nameof(VerifyEmail), "Auth", new { token, email = user.Email }, Request.Scheme);

                string emailBody = $@"
                    <!DOCTYPE html>
                    <html>
                    <head>
                        <style>
                            body {{ font-family: Arial, sans-serif; background-color: #f4f4f4; margin: 0; padding: 0; }}
                            .container {{ max-width: 600px; margin: 40px auto; background: #ffffff; padding: 30px; border-radius: 8px; box-shadow: 0 4px 8px rgba(0,0,0,0.05); }}
                            .header {{ text-align: center; padding-bottom: 20px; border-bottom: 1px solid #eeeeee; }}
                            .header h2 {{ color: #333333; margin: 0; }}
                            .content {{ padding: 20px 0; color: #555555; line-height: 1.6; text-align: center; }}
                            .btn {{ display: inline-block; padding: 12px 25px; margin: 20px 0; background-color: #4CAF50; color: #ffffff !important; text-decoration: none; border-radius: 5px; font-weight: bold; font-size: 16px; transition: background-color 0.3s; }}
                            .btn:hover {{ background-color: #45a049; }}
                            .footer {{ text-align: center; font-size: 12px; color: #999999; padding-top: 20px; border-top: 1px solid #eeeeee; }}
                            .link-text {{ font-size: 12px; color: #007bff; word-break: break-all; }}
                        </style>
                    </head>
                    <body>
                        <div class='container'>
                            <div class='header'>
                                <h2>Welcome to AutoEase!</h2>
                            </div>
                            <div class='content'>
                                <p>Hi {user.Name},</p>
                                <p>Your account has been created by an admin. Please confirm your email address to activate your account and get started.</p>
                                <a href='{confirmationLink}' class='btn'>Confirm Email Address</a>
                                <p>If the button doesn't work, copy and paste this link into your browser:</p>
                                <p class='link-text'>{confirmationLink}</p>
                            </div>
                            <div class='footer'>
                                <p>&copy; {DateTime.UtcNow.Year} AutoEase. All rights reserved.</p>
                            </div>
                        </div>
                    </body>
                    </html>";

                await _emailService.SendEmailAsync(user.Email, "Confirm your account", emailBody);

                return Ok(new { message = "User registered successfully by admin." });
            }

            return BadRequest(result.Errors);
        }

        [HttpPost("secret-admin-register")]
        public async Task<IActionResult> RegisterSecretAdmin([FromBody] RegisterDto model, [FromQuery] string secretKey)
        {
            var expectedKey = _configuration["AdminSecretKey"];
            if (string.IsNullOrEmpty(expectedKey) || secretKey != expectedKey)
            {
                return Unauthorized("Invalid secret key.");
            }

            var user = new User
            {
                UserName = model.Email,
                Email = model.Email,
                Name = model.Name,
                PhoneNumber = model.Phone,
                Role = "Admin"
            };

            var result = await _userManager.CreateAsync(user, model.Password);

            if (result.Succeeded)
            {
                // Auto-confirm email for secret admin to simplify setup
                var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
                await _userManager.ConfirmEmailAsync(user, token);

                return Ok(new { message = "Admin registered successfully and email auto-verified." });
            }

            return BadRequest(result.Errors);
        }

        [HttpGet("verify-email")]
        public async Task<IActionResult> VerifyEmail(string token, string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user == null) return BadRequest("User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            if (result.Succeeded) return Ok("Email verified successfully");

            return BadRequest("Email verification failed");
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.Email);
            if (user == null) return Unauthorized("Invalid credentials or email not verified");

            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Unauthorized("Please verify your email to log in");

            var result = await _signInManager.CheckPasswordSignInAsync(user, model.Password, false);
            if (!result.Succeeded) return Unauthorized("Invalid credentials");

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["Jwt:Key"] ?? "default_secret_key_needs_to_be_long_enough_for_sha256");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                    new Claim(JwtRegisteredClaimNames.Email, user.Email!),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim(ClaimTypes.Name, user.Name)
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);

            return Ok(new
            {
                token = tokenHandler.WriteToken(token)
            });
        }
    }
}