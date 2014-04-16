Xamarin-Sidebar
============================

A slideout navigation control for Xamarin iOS applications.

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
using XamarinSidebar;
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
	
	// set out root view controller with the sidebar menu as the apps root view controller
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

If you want to push controllers onto the stack when a menu option is selected you can
set your content controller to be a UINavigationController. You would then set the button
click action in the side menu controller to push to that navigation controller.

```csharp 
otherContentButton.TouchUpInside += (sender, e) => {
	NavController.PushViewController(new OtherContentController(), false);
	SidebarController.CloseMenu();
};
```

See the sample projects included in the source for more implementation details.