using System;
using System.Drawing;
using UIKit;
using Xamarin.Forms;

namespace XamarinFormsSample.iOS
{
	public class SideMenuController : UIViewController
	{
		// provide access to the sidebar controller to all inheriting controllers
		protected SidebarNavigation.SidebarController SidebarController { 
			get {
				return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.SidebarController;
			} 
		}

		// provide access to the sidebar controller to all inheriting controllers
		protected NavController NavController { 
			get {
				return (UIApplication.SharedApplication.Delegate as AppDelegate).RootViewController.NavController;
			} 
		}

		public override void ViewDidLoad()
		{
			View.BackgroundColor = UIColor.FromRGB(.9f, .9f, .9f);

			var title = new UILabel(new RectangleF(0, 50, 230, 20));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Menu";

			var body = new UILabel(new RectangleF(30, 70, 170, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the side menu. You can use any UIViewController to put whatever you want here!";

			var introButton = new UIButton(UIButtonType.System);
			introButton.Frame = new RectangleF(0, 180, 230, 20);
			introButton.SetTitle("Intro", UIControlState.Normal);
			introButton.TouchUpInside += (sender, e) => {
				NavController.PopToRootViewController(false);
				SidebarController.CloseMenu();
			};

			var contentButton = new UIButton(UIButtonType.System);
			contentButton.Frame = new RectangleF(0, 220, 230, 20);
			contentButton.SetTitle("Content", UIControlState.Normal);
			contentButton.TouchUpInside += (sender, e) => {
				NavController.PushViewController(new MainContentPage().CreateViewController(), false);
				SidebarController.CloseMenu();
			};

			View.Add(title);
			View.Add(body);
			View.Add(introButton);
			View.Add(contentButton);
		}
	}
}

