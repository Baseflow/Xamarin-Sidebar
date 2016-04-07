using System;
using System.Collections.Generic;
using System.Linq;

using Foundation;
using UIKit;
using XamarinFormsSample;

namespace XamarinFormsSample.iOS
{
	[Register("AppDelegate")]
	public partial class AppDelegate : global::Xamarin.Forms.Platform.iOS.FormsApplicationDelegate
	{
		private UIWindow _window;

		public RootViewController RootViewController { get { return UIApplication.SharedApplication.KeyWindow.RootViewController.ChildViewControllers[0] as RootViewController; } }

		public override bool FinishedLaunching(UIApplication app, NSDictionary options)
		{
			global::Xamarin.Forms.Forms.Init();

			// Code for starting up the Xamarin Test Cloud Agent
			#if ENABLE_TEST_CLOUD
			Xamarin.Calabash.Start();
			#endif

			LoadApplication(new App());

			return base.FinishedLaunching(app, options);
		}
	}
}

