# Escc.Net

A .NET Standard code library for connecting to online resources.

The configuration options shown below are for .NET Framework applications and can be read using classes in the separate `Escc.Net.Configuration` package (also defined in this repository).

## Use a shared instance of HttpClient

Applications connecting to external resources should use a single shared instance of `HttpClient` (see 
[You're using HttpClient wrong and it is destabilizing your software](https://aspnetmonsters.com/2016/08/2016-08-27-httpclientwrong/)). In .NET Core you can use [HttpClientFactory](https://docs.microsoft.com/en-us/dotnet/standard/microservices-architecture/implement-resilient-applications/use-httpclientfactory-to-implement-resilient-http-requests). For .NET Framework applications, this project provides `HttpClientProvider`, which you can inject into your consumer class as `IHttpClientProvider`:

	public class ExampleClass
    {
        private static HttpClient _httpClient;

        public ExampleClass(IHttpClientProvider httpClientProvider)
        {
            if (httpClientProvider == null)
            {
                throw new ArgumentNullException(nameof(httpClientProvider));
            }

            if (_httpClient == null)
            {
                _httpClient = httpClientProvider.GetHttpClient();
            }
        }
    }

When you create an `HttpClientProvider` you can specify two arguments:

* `IProxyProvider` configures a proxy (see below). This is assumed to be consistent for the whole application, so event request using the `HttpClient` will use this proxy setting.
* `IWebApiCredentialsProvider` configures the credentials to connect with. This may be different between requests, so `HttpClientProvider` will maintain a separate `HttpClient` for each set of credentials. Any new implementations of `IWebApiCredentialsProvider` should ensure that they override the default equality behaviour to regard two instances with identical credentials as equal to each other.

For a typical request requiring a proxy and authentication configured in `web.config` or `app.config`:

	var proxyProvider = new ConfigurationProxyProvider();
	var credentialsProvider = new ConfigurationWebApiCredentialsProvider();
	var httpClientProvider = new HttpClientProvider(proxyProvider, credentialsProvider);

	var example = new ExampleClass(httpClientProvider);

## Configure a proxy server for web requests

When making web requests, it's common to need a proxy server when running in one environment and not to need one when running in another. The `IProxyProvider` interface allows you to load those settings from anywhere. 

### From JSON (.NET Core)

	using Escc.Net;

	public void ConfigureServices(IServiceCollection services)
    {
		...
    	services.AddOptions();
	    services.Configure<ConfigurationSettings>(options => Configuration.GetSection("Escc.Net").Bind(options));
    	services.AddScoped<IProxyProvider, ProxyFromConfiguration>();
		...
	}

Inject the dependency and use it:

    public ExampleClass(IProxyProvider proxyProvider)
	{
		// This is just an example - in production you should use IHttpClientFactory for web requests
		var exampleRequest = WebRequest.Create(new Uri("http://www.example.org"));
		exampleRequest.Proxy = proxyProvider.CreateProxy();
	}

Configuration settings would typically be in `appsettings.json`:

	{
	  "Escc.Net": {
	    "Proxy": {
	      "Server": "http://127.0.0.1",
	      "User": "domain\\user",
	      "Password": "password"
	    }
	  }
	}

### From web.config or app.config (.NET Framework)

`ConfigurationProxyProvider` lets you load those settings from `web.config` or `app.config`.

	var proxyProvider = new ConfigurationProxyProvider();
	var httpClientProvider = new HttpClientProvider(proxyProvider);

`ConfigurationProxyProvider` loads its proxy settings from `web.config` or `app.config`:

	<configuration>
		<configSections>
			<sectionGroup name="Escc.Net">
				<section name="Proxy" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
			</sectionGroup>
		</configSections>
		<Escc.Net>
			<Proxy>
				<add key="Server" value="http://127.0.0.1" />
				<add key="User" value="domain\user" />
				<add key="Password" value="password" />
			</Proxy>
		</Escc.Net>
	<configuration>

## Request data from a Web API

When you request data from or post data to a .NET Web API you need to configure the web request and handle deserialisation of the returned object to the expected return type. The `WebApiClient` class handles that for you.

Note that `WebApiClient` does not use `HttpClient`, so your application may be more efficient if you write your code using `HttpClient` instead. `WebApiClient` is likely to be removed in future versions of `Escc.Net`.

### From JSON (.NET Core)

	using Escc.Net;

	public void ConfigureServices(IServiceCollection services)
    {
		...
    	services.AddOptions();
	    services.Configure<ConfigurationSettings>(options => Configuration.GetSection("Escc.Net").Bind(options));
    	services.AddTransient<IWebApiClient, WebApiClient>();
    	services.AddScoped<IWebApiCredentialsProvider, WebApiCredentialsFromConfiguration>();
		...
	}

Inject the dependency and use it:

    public ExampleClass(IWebApiClient api)
	{
	    return api.Get<ReturnType>(new Uri("http://example.org/ExampleApi/ExampleMethod"));
	}

Configuration settings would typically be in `appsettings.json`:

	{
	  "Escc.Net": {
	    "WebApi": {
	      "User": "domain\\user",
	      "Password": "password"
	    }
	  }
	}

### From web.config or app.config (.NET Framework)

    var api = new WebApiClient(new ConfigurationWebApiCredentialsProvider());
    return api.Get<ReturnType>(new Uri("http://example.org/ExampleApi/ExampleMethod"));

`ConfigurationWebApiCredentialsProvider` loads its authentication settings from `web.config` or `app.config`:

  	<configuration>
		<configSections>
			<sectionGroup name="Escc.Net">
				<section name="WebApi" type="System.Configuration.NameValueSectionHandler, System, Version=1.0.5000.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" />
			</sectionGroup>
		</configSections>
		<Escc.Net>
			<WebApi>
				<add key="User" value="domain\user" />
				<add key="Password" value="password" />
			</WebApi>
		</Escc.Net>
	<configuration>

## Convert ASMX web service proxy objects to their original type

When you connect to an ASMX web service and call a method that returns anything other than a simple type (`int`, `string` and so on), .NET creates a proxy version of the original class. This has all of the properties of the original class, but sometimes you need the methods too. `WebServiceProxyConverter` allows you to convert between the two types by serialising one type to XML and deserialising it as the other.

    var proxyObject = webservice.ExampleWebMethod(); 
	var converter = new WebServiceProxyConverter<ProxyObjectType, OriginalObjectType>("http://tempuri.org/");
	var originalType = converter.ConvertProxyToOriginalType(proxyObject);

You can also pass in the XML namespace of the data object, if that is different to the namespace of the web service.

The `IProxyObjectConverter` interface allows you to replace `WebServiceProxyConverter` with another implementation.