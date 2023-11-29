using CommunityToolkit.Maui;

namespace Synegit.Maui.DateTimePicker;

public static class AppBuilderExtensions
{
    public static MauiAppBuilder ConfigureSGDateTimePicker(this MauiAppBuilder builder)
    {
#if DEBUG
        return builder.UseMauiCommunityToolkit();
#else
        return builder.UseMauiCommunityToolkit(options =>
             {
                 options.SetShouldSuppressExceptionsInConverters(true);
                 options.SetShouldSuppressExceptionsInBehaviors(true);
                 options.SetShouldSuppressExceptionsInAnimations(true);
             });
#endif
    }
}
