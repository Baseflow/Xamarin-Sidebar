using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NavigationSample
{
	public partial class IntroController : BaseController
	{
		public IntroController() : base(null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			View.BackgroundColor = UIColor.White;

			var title = new UILabel(new RectangleF(0, 80, 320, 30));
			title.Font = UIFont.SystemFontOfSize(24.0f);
			title.TextAlignment = UITextAlignment.Center;
			title.TextColor = UIColor.Blue;
			title.Text = "Sidebar Navigation";

			var body = new UILabel(new RectangleF(50, 120, 220, 100));
			body.Font = UIFont.SystemFontOfSize(12.0f);
			body.TextAlignment = UITextAlignment.Center;
			body.Lines = 0;
			body.Text = @"This is the intro view controller. 
Click the button in the bar to open the menu to switch controllers.

You can also drag the menu open from the right side of the screen";

			View.Add(title);
			View.Add(body);
		}
	}
}

