public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration config)
{
    services.AddDbContext<StoreDbContext>(o =>
        o.UseSqlServer(config.GetConnectionString("DefaultConnection")));

    services.AddDbContext<IdentityStoreDbContext>(o =>
        o.UseSqlServer(config.GetConnectionString("IdentityConnection")));

    services.AddScoped<IIdentityUnitOfWork, IdentityUnitOfWork>();
    services.AddScoped<ITokenService, TokenServices>();
    services.AddScoped<IAuthenticationService, AuthenticationService>();
    services.AddScoped<IProductService, ProductService>();
    services.AddScoped<IBasketService, BasketService>();

    services.AddScoped<IServiceManager, ServiceManager>();

    services.AddHttpContextAccessor();
    services.AddScoped<IClientIpProvider, ClientIpProvider>();

    services.AddIdentity<User, IdentityRole>(o =>
    {
        o.User.RequireUniqueEmail = true;
        o.Password.RequiredLength = 6;
    })
    .AddEntityFrameworkStores<IdentityStoreDbContext>();

    return services;
}
