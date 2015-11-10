using System;
using Xamarin.Forms.Platform.iOS;
using Xamarin.Forms;
using UIKit;
using System.Collections.Generic;
using System.Linq;

[assembly:ExportRenderer(typeof(XamarinFormsSample.IntroPage), typeof(XamarinFormsSample.iOS.IntroController))]

namespace XamarinFormsSample.iOS
{
	public class IntroController : PageRenderer
	{
		public override void ViewDidLoad()
		{
			base.ViewDidLoad();


		}

		public override void ViewWillAppear(bool animated)
		{
			base.ViewWillAppear(animated);

			var navigationItem = this.NavigationController.TopViewController.NavigationItem;
			navigationItem.SetRightBarButtonItem(
				new UIBarButtonItem(UIImage.FromBundle("threelines")
					, UIBarButtonItemStyle.Plain
					, (sender,args) => {
						//SidebarController.ToggleMenu();
					}), true);
		}
	}
}

