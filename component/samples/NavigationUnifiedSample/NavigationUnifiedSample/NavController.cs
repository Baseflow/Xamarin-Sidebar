
using System;
using System.Drawing;

using Foundation;
using UIKit;

namespace NavigationUnifiedSample
{
	public partial class NavController : UINavigationController
	{
		public NavController() : base((string)null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();

			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

