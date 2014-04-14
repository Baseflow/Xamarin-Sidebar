using System;
using System.Drawing;
using MonoTouch.Foundation;
using MonoTouch.UIKit;

namespace NavigationSample
{
	public partial class NavigationController : UINavigationController
	{
		public NavigationController() : base((string)null, null)
		{
		}

		public override void ViewDidLoad()
		{
			base.ViewDidLoad();
			
			// Perform any additional setup after loading the view, typically from a nib.
		}
	}
}

