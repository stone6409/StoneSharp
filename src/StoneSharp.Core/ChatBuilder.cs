// 修改 ChatBuilder.cs
using StoneSharp.Core.ChatMessages;
using StoneSharp.Core.ChatMessages.SemanticKernel;
using Microsoft.Extensions.DependencyInjection;

namespace StoneSharp.Core
{
    /// <summary>
    /// 服务集合构建器
    /// </summary>
    public class ChatBuilder
    {
        private readonly IServiceCollection _services;
        private ServiceProvider _serviceProvider;

        public ChatBuilder()
        {
            _services = new ServiceCollection();
        }

        public ServiceProvider Services 
        { 
            get 
            { 
                if (_serviceProvider == null)
                {
                    _serviceProvider = _services.BuildServiceProvider();
                }
                return _serviceProvider;
            } 
        }

        /// <summary>
        /// 添加单例服务
        /// </summary>
        public ChatBuilder AddSingleton<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// 添加单例服务实例
        /// </summary>
        public ChatBuilder AddSingleton<TService>(TService implementationInstance)
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), implementationInstance));
            return this;
        }

        /// <summary>
        /// 添加单例服务工厂
        /// </summary>
        public ChatBuilder AddSingleton<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// 添加单例服务（无实现类型）
        /// </summary>
        public ChatBuilder AddSingleton<TService>()
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), ServiceLifetime.Singleton));
            return this;
        }

        /// <summary>
        /// 添加作用域服务
        /// </summary>
        public ChatBuilder AddScoped<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Scoped));
            return this;
        }

        /// <summary>
        /// 添加作用域服务（无实现类型）
        /// </summary>
        public ChatBuilder AddScoped<TService>()
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), ServiceLifetime.Scoped));
            return this;
        }

        /// <summary>
        /// 添加作用域服务工厂
        /// </summary>
        public ChatBuilder AddScoped<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Scoped));
            return this;
        }

        /// <summary>
        /// 添加瞬态服务
        /// </summary>
        public ChatBuilder AddTransient<TService, TImplementation>()
            where TService : class
            where TImplementation : class, TService
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TImplementation), ServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// 添加瞬态服务（无实现类型）
        /// </summary>
        public ChatBuilder AddTransient<TService>()
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), typeof(TService), ServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// 添加瞬态服务工厂
        /// </summary>
        public ChatBuilder AddTransient<TService>(Func<IServiceProvider, TService> implementationFactory)
            where TService : class
        {
            _services.Add(new ServiceDescriptor(typeof(TService), implementationFactory, ServiceLifetime.Transient));
            return this;
        }

        /// <summary>
        /// 构建服务提供者
        /// </summary>
        public ServiceProvider BuildServiceProvider()
        {
            return Services;
        }

        /// <summary>
        /// 构建聊天服务
        /// </summary>
        public IChatMessageService Build()
        {
            // 确保服务提供者已构建
            var serviceProvider = Services;

            // 尝试获取 IChatService
            var chatMessageService = serviceProvider.GetService<IChatMessageService>();

            if (chatMessageService == null)
            {
                throw new InvalidOperationException(
                        "No IChatService implementation registered, Please register an IChatMessageService implementation.");
            }

            return chatMessageService;
        }
    }
}