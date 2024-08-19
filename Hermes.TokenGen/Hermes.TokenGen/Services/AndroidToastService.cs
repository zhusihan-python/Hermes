using Android.Content;
using Android.Widget;

namespace Hermes.TokenGen.Services;

public class AndroidToastService : ToastService
{
#pragma warning disable CA1416
    private readonly Context? _context = Android.App.Application.Context;

    protected override void ShowToast(string title, string message)
    {
        var context = Android.App.Application.Context;
        Toast.MakeText(context, title, ToastLength.Short)!.Show();
    }
#pragma warning restore CA1416
}