using System;
using Xamarin.Forms;
using UIKit;

[assembly:ExportRenderer(typeof(XamarinFormsSample.NavPage), typeof(XamarinFormsSample.iOS.NavController))]

namespace XamarinFormsSample.iOS
{	
	public class NavController : UINavigationController
	{
		
	}
}

