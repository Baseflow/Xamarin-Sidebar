using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace Sample
{
	public partial class SideMenuController : BaseController
	{
		// the sidebar controller for the app
		SidebarNavigation.SidebarController _sidebarController;

		public SideMenuController() : base(null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.FromRGB(.9f, .9f, .9f);

			var title = new UILabel(new RectangleF(0, 50, 270, 20));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Menu";

			var body = new UILabel(new RectangleF(30, 70, 200, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the side menu. You can use any UIViewController to put whatever you want here!";

			var introButton = new UIButton(UIButtonType.System);
			introButton.Frame = new RectangleF(0, 180, 260, 20);
			introButton.SetTitle("Intro", UIControlState.Normal);
			introButton.TouchUpInside += (sender, e) => {
				SidebarController.ChangeContentView(new IntroController());
			};

			var contentButton = new UIButton(UIButtonType.System);
			contentButton.Frame = new RectangleF(0, 220, 260, 20);
			contentButton.SetTitle("Content", UIControlState.Normal);
			contentButton.TouchUpInside += (sender, e) => {
				SidebarController.ChangeContentView(new ContentController());
			};

			var disableButton = new UIButton(UIButtonType.System);
			disableButton.Frame = new RectangleF(0, 260, 260, 20);
			disableButton.SetTitle("Toggle Disabled", UIControlState.Normal);
			disableButton.TouchUpInside += (sender, e) => {
				SidebarController.Disabled = !SidebarController.Disabled;
			};

			View.Add(title);
			View.Add(body);
			View.Add(introButton);
			View.Add(contentButton);
			View.Add(disableButton);
		}
	}
}

