global using System.Collections.Immutable;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Localization;
global using Microsoft.Extensions.Logging;
global using Microsoft.Extensions.Options;
global using CapstoneSpaceInvader.Models;
global using CapstoneSpaceInvader.Presentation;
global using CapstoneSpaceInvader.Services.Endpoints;
global using Uno.Extensions.Http.Kiota;
global using ApplicationExecutionState = Windows.ApplicationModel.Activation.ApplicationExecutionState;

[assembly: Uno.Extensions.Reactive.Config.BindableGenerationTool(3)]