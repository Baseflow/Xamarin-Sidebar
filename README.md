Xamarin-Sidebar
============================

A slideout navigation control for Xamarin iOS applications.

This is partly based on two other good slideout menu implementation that each didn't 
provide me quite what I was looking for. Those two are 
[FlyoutNavigation](https://github.com/Clancey/FlyoutNavigation)
and
[SlideoutNavigation](https://github.com/thedillonb/MonoTouch.SlideoutNavigation/).

`Xamarin-Sidebar` allows you to provide one UIViewController to be used as a content view
and another to be used as a menu. When you open the menu the content view will slide over
to reveal the provided menu UIViewController.

To set it up just create a SidebarController, passing in your content and menu controllers,
and set it as your applications RootViewController


