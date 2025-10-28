﻿using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using erp_backend.Data;
using erp_backend.Services;
using IronPdf;

var builder = WebApplication.CreateBuilder(args);

// Cấu hình IronPDF License Key từ appsettings.json
var ironPdfLicenseKey = builder.Configuration["IronPdf:LicenseKey"];
if (!string.IsNullOrEmpty(ironPdfLicenseKey))
{
	IronPdf.License.LicenseKey = ironPdfLicenseKey;
}

// Add services to the container.
builder.Services.AddControllers();

// Add Entity Framework and PostgreSQL
builder.Services.AddDbContext<ApplicationDbContext>(options =>
	options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add JWT Authentication services
var jwtKey = builder.Configuration["Jwt:Key"];
var key = Encoding.UTF8.GetBytes(jwtKey);

builder.Services.AddAuthentication(options =>
{
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = true,
		ValidateLifetime = true,
		ValidateIssuerSigningKey = true,
		ValidIssuer = builder.Configuration["Jwt:Issuer"],
		ValidAudience = builder.Configuration["Jwt:Audience"],
		IssuerSigningKey = new SymmetricSecurityKey(
			Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"] ?? "YourSecureKeyWithAtLeast32Characters"))
	};

	options.Events = new JwtBearerEvents
	{
		OnMessageReceived = context =>
		{
			// Allow authentication from query string for SignalR
			var accessToken = context.Request.Query["access_token"];
			var path = context.HttpContext.Request.Path;
			if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/hubs"))
			{
				context.Token = accessToken;
			}
			return Task.CompletedTask;
		}
	};
});

// Register JwtService
builder.Services.AddScoped<JwtService>();

// Add Email Service
builder.Services.AddScoped<IEmailService, EmailService>();

// Add Authorization
builder.Services.AddAuthorization();

// Add CORS policy
builder.Services.AddCors(options =>
{
	options.AddPolicy("AllowReactApp", policy =>
	{
		policy.WithOrigins("http://localhost:3000", "http://localhost:5173", "http://localhost:4200") // React dev server ports
			  .AllowAnyMethod()
			  .AllowAnyHeader()
			  .AllowCredentials();
	});
});

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
	c.SwaggerDoc("v1", new OpenApiInfo { Title = "ERP Backend API", Version = "v1" });

	// Add JWT Authentication to Swagger
	c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
	{
		Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
		Name = "Authorization",
		In = ParameterLocation.Header,
		Type = SecuritySchemeType.ApiKey,
		Scheme = "Bearer"
	});

	c.AddSecurityRequirement(new OpenApiSecurityRequirement
	{
		{
			new OpenApiSecurityScheme
			{
				Reference = new OpenApiReference
				{
					Type = ReferenceType.SecurityScheme,
					Id = "Bearer"
				}
			},
			new string[] {}
		}
	});
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

// Use CORS - PHẢI ĐẶT TRƯỚC UseAuthorization
app.UseCors("AllowReactApp");

app.UseHttpsRedirection();

// Add Authentication & Authorization middleware - QUAN TRỌNG!
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();