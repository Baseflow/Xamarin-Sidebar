Xamarin-Sidebar
============================

A slideout navigation control for Xamarin iOS applications.

*If you're using Sidebar Navigation send me a link to your app and I'll happily post it here.*

![Menu Open](https://raw.githubusercontent.com/jdehlin/Xamarin-Sidebar/master/Screenshots/Transition.png)

This is partly based on two other good slideout menu implementation that each didn't 
provide quite what I was looking for. Those two are 
[FlyoutNavigation](https://github.com/Clancey/FlyoutNavigation)
and
[SlideoutNavigation](https://github.com/thedillonb/MonoTouch.SlideoutNavigation/).

`Xamarin-Sidebar` allows you to provide one UIViewController to be used as a content view
and another to be used as a menu. When you open the menu the content view will slide over
to reveal the provided menu UIViewController.

To set it up just create a root UIViewController and a SidebarController, passing in your 
content and menu controllers.

`RootViewController.cs`
```csharp
using SidebarNavigation;
...
public partial class RootViewController : UIViewController
{
	// the sidebar controller for the app
	public SidebarController SidebarController { get; private set; }

	public override void ViewDidLoad()
	{
		base.ViewDidLoad();

		// create a slideout navigation controller with the top navigation controller and the menu view controller
		SidebarController = new SidebarController(this, new ContentController(), new SideMenuController());
	}
}
...
```

`AppDelegate.cs`
```csharp
...
public override bool FinishedLaunching(UIApplication app, NSDictionary options)
{
	window = new UIWindow(UIScreen.MainScreen.Bounds);
	
	// set our root view controller with the sidebar menu as the apps root view controller
	window.RootViewController = new RootViewController();
	
	window.MakeKeyAndVisible();
	return true;
}
...
```

In the content controller you can add a button to open the slideout menu.

```csharp
menuButton.TouchUpInside += (sender, e) => {
	SidebarController.ToggleMenu();
};
```

In the side menu controller you can add buttons to change the content view.

```csharp
otherContentButton.TouchUpInside += (sender, e) => {
	SidebarController.ChangeContentView(new OtherContentController());
};
```

Additional options include hiding the shadow, setting the menu width, and placing the menu 
on the left.

```csharp
SidebarController.HasShadowing = false;
SidebarController.MenuWidth = 220;
SidebarController.MenuLocation = SidebarController.MenuLocations.Left;
```

See the sample projects included in the source for more details.