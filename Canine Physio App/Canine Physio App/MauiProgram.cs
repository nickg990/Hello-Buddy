using Canine_Physio_App.Helpers;
using Canine_Physio_App.Services;
using Canine_Physio_App.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;
using Microsoft.Maui.Controls.Handlers.Compatibility;

namespace Canine_Physio_App
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkitMediaElement()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

            // Register platform-specific Shell renderers for tab reselection
#if ANDROID
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Shell, Canine_Physio_App.Platforms.Android.CustomShellRenderer>();
            });
#elif IOS
            builder.ConfigureMauiHandlers(handlers =>
            {
                handlers.AddHandler<Shell, Canine_Physio_App.Platforms.iOS.CustomShellRenderer>();
            });
#endif

            // Register services
            builder.Services.AddSingleton<ISafeAreaService, SafeAreaService>();
            builder.Services.AddSingleton<PhysioContentService>();
            builder.Services.AddSingleton<SessionStateService>();

            // Register helpers
            builder.Services.AddSingleton<TextContentLoader>();

            // Register pages
            builder.Services.AddTransient<InformationPage>();
            builder.Services.AddTransient<SettingsPage>();
            builder.Services.AddTransient<LoginPage>();
            builder.Services.AddTransient<ForgottenPasswordPage>();
            builder.Services.AddTransient<RegistrationPage>();
            builder.Services.AddTransient<TermsConditionsPage>();
            builder.Services.AddTransient<MainExercisesPage>();
            builder.Services.AddTransient<ExerciseDetailPage>();
            builder.Services.AddTransient<ExerciseProgressPage>();
            builder.Services.AddTransient<SkipToNextSessionPage>();
            builder.Services.AddTransient<ProgressTabPage>();

            // Centralised handler customisation: remove underline from Editor on Android
            Microsoft.Maui.Handlers.EditorHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
#if ANDROID
                handler.PlatformView.BackgroundTintList =
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
#endif
            });

#if DEBUG
    		builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
