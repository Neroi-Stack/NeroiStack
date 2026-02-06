using Android.App;
using Android.Runtime;
using System;

namespace NeroiStack.Android;

[Application]
public class Application : global::Android.App.Application
{
    public Application(IntPtr handle, JniHandleOwnership transfer)
        : base(handle, transfer)
    {
    }
}
