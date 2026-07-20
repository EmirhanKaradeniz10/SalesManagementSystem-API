using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using SalesManagementSystem.API.DTOs.Auth;
using SalesManagementSystem.API.DTOs.Email;
using SalesManagementSystem.API.Exceptions;
using SalesManagementSystem.API.Services;
using SalesManagementSystem.API.Services.Auth;
using SalesManagementSystem.API.Services.Email;
using SalesManagementSystem.Data;
using SalesManagementSystem.Models;

namespace SalesManagementSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly TokenService _tokenService;
    private readonly PasswordService _passwordService;
    private readonly AppDbContext _context;
    private readonly EmailService _emailService;
    private readonly ILogger<AuthController> _logger;
    private readonly EmailValidationService _emailValidationService;

    public AuthController(TokenService tokenService,
                            PasswordService passwordService,
                            AppDbContext context,
                            EmailService emailService,
                            ILogger<AuthController> logger, 
                            EmailValidationService emailValidationService)
    {
        _tokenService = tokenService;
        _passwordService = passwordService;
        _context = context;
        _emailService = emailService;
        _logger = logger;
        _emailValidationService = emailValidationService;
    }

    /// <summary>
    /// Authenticates a user and returns access and refresh tokens.
    /// </summary>
    /// <param name="dto">User login credentials.</param>
    /// <returns>JWT access token and refresh token.</returns>
    /// <response code="200">Login successful.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Invalid username or password.</response>
    /// <response code="429">Too many login attempts.</response>
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> Login(LoginRequestDto dto)
    {
        // Authenticates user, checks lockout/status, and logs login attempts

        var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
        var userAgent = Request.Headers["User-Agent"].ToString();

        var user = _context.Users
            .FirstOrDefault(x => x.Username == dto.Username);

        //INACTIVE CHECK
        if (user != null && !user.IsActive)
        {
            throw new AppException("Account is inactive.", StatusCodes.Status403Forbidden);
        }

        // LOCKOUT CHECK
        if (user != null &&
            user.LockoutEnd != null &&
            user.LockoutEnd > DateTime.UtcNow)
        {
            LogLogin(
                dto.Username,
                false,
                "Account locked",
                ipAddress,
                userAgent);

            await _context.SaveChangesAsync();

            throw new AppException(
                "Account temporarily locked. Try again later.",
                StatusCodes.Status401Unauthorized);
        }

        // FAILED LOGIN
        if (user == null || !_passwordService.VerifyPassword(dto.Password, user.PasswordHash))
        {
            if (user != null)
            {
                user.FailedLoginAttempts++;

                if (user.FailedLoginAttempts >= 5)
                {
                    user.LockoutEnd = DateTime.UtcNow.AddMinutes(15);
                    user.FailedLoginAttempts = 0;
                }
            }

            LogLogin(
                dto.Username,
                false,
                "Invalid credentials",
                ipAddress,
                userAgent);

            await _context.SaveChangesAsync();

            throw new AppException(
                "Invalid credentials",
                StatusCodes.Status401Unauthorized);
        }

        // SUCCESS LOGIN
        user.FailedLoginAttempts = 0;
        user.LockoutEnd = null;

        var accessToken = _tokenService.CreateToken(user.Username, user.Role);
        var refreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        LogLogin(
            user.Username,
            true,
            null,
            ipAddress,
            userAgent);

        await _context.SaveChangesAsync();

        return Ok(new
        {
            accessToken,
            refreshToken
        });
    }

    // REFRESH TOKEN ENDPOINT

    /// <summary>
    /// Generates a new access token using a valid refresh token.
    /// </summary>
    /// <param name="dto">Refresh token information.</param>
    /// <returns>A new access token and refresh token.</returns>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Invalid or expired refresh token.</response>
    [HttpPost("refresh")]
    public IActionResult Refresh(TokenRequestDto dto)
    {
        // Validates refresh token and generates a new pair of JWT tokens

        var user = _context.Users
            .FirstOrDefault(x => x.RefreshToken == dto.RefreshToken);

        if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
        {
            throw new AppException("Invalid refresh token",
                                    StatusCodes.Status401Unauthorized);
        }

        var newAccessToken = _tokenService.CreateToken(user.Username, user.Role);
        var newRefreshToken = _tokenService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);

        _context.SaveChanges();

        return Ok(new
        {
            accessToken = newAccessToken,
            refreshToken = newRefreshToken
        });
    }


    /// <summary>
    /// Registers a new user account.
    /// </summary>
    /// <param name="dto">User registration information.</param>
    /// <returns>The newly created user.</returns>
    /// <response code="200">User registered successfully.</response>
    /// <response code="400">Validation failed or invalid request.</response>
    /// <response code="401">Authentication required.</response>
    /// <response code="403">Administrator privileges required.</response>
    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterRequestDto dto)
    {
        // Creates a new user, validates email/password, and sends a welcome mail ()

        if (!ModelState.IsValid)
        {
            var errors = string.Join(" | ",
                ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));

            throw new AppException(errors);
        }
        //PASSWORD POLICY. (8 or more character, at least 1 letter, at least 1 number.)

        if (dto.Password.Length < 8)
            throw new AppException("Password must be at least 8 characters long");

        if (!dto.Password.Any(char.IsLetter))
            throw new AppException("Password must contain at least one letter");

        if (!dto.Password.Any(char.IsDigit))
            throw new AppException("Password must contain at least one number");

        var existingUser = _context.Users
            .FirstOrDefault(x => x.Username == dto.Username);

        var existingEmail = _context.Users
            .FirstOrDefault(x => x.Email == dto.Email);

        //Username cant be same.
        if (existingUser != null)
            throw new AppException("User already exists");

        //Is email domain correct?
        if (!await _emailValidationService.DomainExistsAsync(dto.Email))
        {
            throw new AppException("Email domain does not exist.");
        }

        //Email cant be same.
        if (existingEmail != null)
            throw new AppException("Email is already registered");

        var customer = new Customer
        {
            Name = dto.Username
        };

        _context.Customers.Add(customer);
        await _context.SaveChangesAsync();

        var hashedPassword = _passwordService.HashPassword(dto.Password);

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = hashedPassword,
            Role = "User",
            CustomerId = customer.Id
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        /*
        Sends welcome email via Resend Free Plan (admin-only without custom domain) 
        and logs any failures using try-catch
        */
        try
        {
            await _emailService.SendAsync(new EmailRequestDto
            {
                To = dto.Email,
                Subject = "Welcome to Sales Management System",
                HtmlBody = $"""
            <h2>Welcome, {dto.Username}!</h2>

            <p>Your account has been created successfully.</p>

            <p>You can now log in and start using the system.</p>

            <br/>

            <p>Sales Management System</p>
            """
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Failed to send welcome email to {Email}",
                dto.Email);
        }

        return Ok(new
        {
            success = true,
            message = "User created",
            data = user.Id
        });
    }

    private void LogLogin(
        string username,
        bool isSuccess,
        string? failureReason,
        string? ipAddress,
        string userAgent)
        {
        // Records the login attempt details to the database for auditing

        _context.LoginAudits.Add(new LoginAudit
            {
                Username = username,
                IpAddress = ipAddress,
                IsSuccess = isSuccess,
                LoginTime = DateTime.UtcNow,
                UserAgent = userAgent,
                FailureReason = failureReason
            });
        }
}