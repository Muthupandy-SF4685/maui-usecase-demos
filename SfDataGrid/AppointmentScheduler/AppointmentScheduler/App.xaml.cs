using System;
using System.Collections.Generic;

namespace AppointmentScheduler;

public partial class App : Application
{
    const string LightThemePath = "Resources/Styles/Theme.Light.xaml";
    const string DarkThemePath = "Resources/Styles/Theme.Dark.xaml";

    public App()
    {
        InitializeComponent();

        // Load theme dictionaries early so pages can resolve md_sys_* resources during XAML parsing
        try
        {
            ApplyTheme(Application.Current?.RequestedTheme ?? AppTheme.Unspecified);
            if (Application.Current != null)
                Application.Current.RequestedThemeChanged += (s, e) => ApplyTheme(e.RequestedTheme);
        }
        catch
        {
            // ignore
        }
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        try
        {
            var idiom = Microsoft.Maui.Devices.DeviceInfo.Idiom;
            if (idiom == Microsoft.Maui.Devices.DeviceIdiom.Phone || idiom == Microsoft.Maui.Devices.DeviceIdiom.Tablet)
            {
                return new Window(new AppShell());
            }
        }
        catch
        {
            // If platform detection fails, fall back to desktop RootPage
        }

        return new Window(new RootPage());
    }

    void ApplyTheme(AppTheme theme)
    {
        try
        {
            var merged = Resources.MergedDictionaries;
            var toRemove = new List<ResourceDictionary>();
            foreach (var rd in merged)
            {
                if (rd.Source != null && (rd.Source.OriginalString.EndsWith("Theme.Light.xaml") || rd.Source.OriginalString.EndsWith("Theme.Dark.xaml")))
                {
                    toRemove.Add(rd);
                }
            }

            foreach (var rd in toRemove)
                merged.Remove(rd);

            var themePath = theme == AppTheme.Dark ? DarkThemePath : LightThemePath;
            var themeDict = new ResourceDictionary { Source = new Uri(themePath, UriKind.Relative) };
            Resources.MergedDictionaries.Add(themeDict);
        }
        catch (Exception)
        {
            // ignore
        }
    }

    // Public helper to set app theme (Light/Dark) and apply theme resources immediately
    public void SetAppTheme(AppTheme theme)
    {
        try
        {
            if (Application.Current != null)
            {
                Application.Current.UserAppTheme = theme;
            }

            // Use the explicit theme requested
            ApplyTheme(theme == AppTheme.Unspecified ? (Application.Current?.RequestedTheme ?? AppTheme.Unspecified) : theme);
        }
        catch
        {
            // ignore
        }
    }
}