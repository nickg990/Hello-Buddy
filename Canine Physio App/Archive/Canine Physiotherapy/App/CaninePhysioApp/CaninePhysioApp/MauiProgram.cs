using CaninePhysioApp.Services;
using CaninePhysioApp.Views;
using CommunityToolkit.Maui;
using Microsoft.Extensions.Logging;

namespace CaninePhysioApp
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
                    fonts.AddFont("Montserrat-Regular.ttf", "MontserratRegular");
                    fonts.AddFont("Montserrat-SemiBold.ttf", "MontserratSemiBold");
                });

            // Register services
            builder.Services.AddSingleton<TextContentService>();
            builder.Services.AddSingleton<PhysioContentService>();

            // Register pages for DI
            builder.Services.AddTransient<InformationPage>();
            builder.Services.AddTransient<TermsConditionsPage>();
            builder.Services.AddTransient<MainExercisesPage>();
            builder.Services.AddTransient<ExerciseDetailPage>();

            // Remove underline from Entry on Android
#if ANDROID
            Microsoft.Maui.Handlers.EntryHandler.Mapper.AppendToMapping("NoUnderline", (handler, view) =>
            {
                handler.PlatformView.BackgroundTintList = 
                    Android.Content.Res.ColorStateList.ValueOf(Android.Graphics.Color.Transparent);
            });
#endif

#if DEBUG
            builder.Logging.AddDebug();
#endif

            return builder.Build();
        }
    }
}
