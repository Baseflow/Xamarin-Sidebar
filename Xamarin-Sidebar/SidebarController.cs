using System;
using System.Drawing;

#if __UNIFIED__
using UIKit;
using Foundation;
using CoreGraphics;

using RectangleF = CoreGraphics.CGRect;
using SizeF = CoreGraphics.CGSize;
using PointF = CoreGraphics.CGPoint;
#else
using MonoTouch.UIKit;
using MonoTouch.Foundation;
using MonoTouch.CoreGraphics;

using nfloat = global::System.Single;
using nint = global::System.Int32;
using nuint = global::System.UInt32;
#endif

namespace SidebarNavigation
{
	public class SidebarController : UIViewController {

		private Sidebar _sidebar;


		/// <summary>
		/// Required contructor.
		/// </summary>
		public SidebarController(IntPtr handle) : base(handle)
		{
		}

		/// <summary>
		/// Contructor.
		/// </summary>
		/// <param name="rootViewController">
		/// The view controller that the Sidebar is being added to.
		/// </param>
		/// <param name="contentViewController">
		/// The view controller for the content area.
		/// </param>
		/// <param name="navigationViewController">
		/// The view controller for the side menu.
		/// </param>
		public SidebarController(
			UIViewController rootViewController, 
			UIViewController contentViewController, 
			UIViewController menuViewController)
		{
			_sidebar = new Sidebar(rootViewController, contentViewController, menuViewController);

			_sidebar.StateChangeHandler += (sender, e) => { 
				if (StateChangeHandler != null)
					StateChangeHandler.Invoke(sender, e); 
			};

			ChangeMenuView(menuViewController);
			ChangeContentView(contentViewController);

			AttachSidebarControllerToRootController(rootViewController);
		}


		/// <summary>
		/// This Event will be called when the Sidebar menu is Opened/Closed (at the end of the animation).
		/// The Event Arg is a Boolean = isOpen.
		/// </summary>
		public event EventHandler<bool> StateChangeHandler;


		/// <summary>
		/// The view controller shown in the content area.
		/// </summary>
		public UIViewController ContentAreaController { get { return _sidebar.ContentViewController; } }

		/// <summary>
		/// The view controller for the side menu.
		/// This is what will be shown when the menu is displayed.
		/// </summary>
		public UIViewController MenuAreaController { get { return _sidebar.MenuViewController; } }


        /// <summary>
		/// Determines the percent of width to complete slide action.
		/// </summary>
		public float FlingPercentage {
			get { return _sidebar.FlingPercentage; }
			set { _sidebar.FlingPercentage = value; }
		}
			
        /// <summary>
		/// Determines the minimum velocity considered a "fling" to complete slide action.
		/// </summary>
		public float FlingVelocity {
			get { return _sidebar.FlingVelocity; }
			set { _sidebar.FlingVelocity = value; }
		}

		/// <summary>
		/// Active area where the Pan gesture is intercepted.
		/// </summary>
		public float GestureActiveArea {
			get { return _sidebar.GestureActiveArea; }
			set { _sidebar.GestureActiveArea = value; }
		}

		/// <summary>
		/// Gets or sets a value indicating whether there should be shadowing effects on the content view.
		/// </summary>
		public bool HasShadowing {
			get { return _sidebar.HasShadowing; }
			set { _sidebar.HasShadowing = value; }
		}

		/// <summary>
		/// Determines if the menu should be reopened after the screen is roated.
		/// </summary>
		public bool ReopenOnRotate {
			get { return _sidebar.ReopenOnRotate; }
			set { _sidebar.ReopenOnRotate = value; }
		}

		/// <summary>
		/// Determines the width of the menu when open.
		/// </summary>
		public int MenuWidth {
			get { return _sidebar.MenuWidth; }
			set { _sidebar.MenuWidth = value; }
		}

		/// <summary>
		/// Determines if the menu is on the left or right of the screen.
		/// </summary>
		public MenuLocations MenuLocation {
			get { return _sidebar.MenuLocation; }
			set { _sidebar.MenuLocation = value; }
		}

		/// <summary>
		/// Disables all open/close actions when set to true.
		/// </summary>
		public bool Disabled {
			get { return _sidebar.Disabled; }
			set { _sidebar.Disabled = value; }
		}

		/// <summary>
		/// Gets the current state of the menu.
		/// Setting this property will open/close the menu respectively.
		/// </summary>
		public bool IsOpen
		{
			get { return _sidebar.IsOpen; }
			set
			{
				_sidebar.IsOpen = value;
				if (_sidebar.IsOpen)
					CloseMenu();
				else
					OpenMenu();
			}
		}


		/// <summary>
		/// Toggles the menu open or closed.
		/// </summary>
		public void ToggleMenu()
		{
			if (IsOpen)
				_sidebar.CloseMenu();
			else
				_sidebar.OpenMenu();
		}

		/// <summary>
		/// Shows the slideout navigation menu.
		/// </summary>
		public void OpenMenu()
		{
			_sidebar.OpenMenu();
		}

		/// <summary>
		/// Hides the slideout navigation menu.
		/// </summary>
		public void CloseMenu(bool animate = true)
		{
			_sidebar.CloseMenu(animate);
		}
			
		/// <summary>
		/// Replaces the content area view controller with the specified view controller.
		/// </summary>
		/// <param name="newContentView">
		/// New content view.
		/// </param>
		public void ChangeContentView(UIViewController newContentView) {
			_sidebar.ChangeContentView(newContentView);
			AddContentViewToSidebar();
			CloseMenu();
		}
			
		/// <summary>
		/// Replaces the menu area view controller with the specified view controller.
		/// </summary>
		/// <param name="newMenuView">
		/// New menu view.
		/// </param>
		public void ChangeMenuView(UIViewController newMenuView) {
			_sidebar.ChangeMenuView(newMenuView);
			AddMenuViewToSidebar();
		}


		private void AddContentViewToSidebar()
		{
			
			SetContentViewBounds();
			SetContentViewPosition();
			View.AddSubview(ContentAreaController.View);
			AddChildViewController(ContentAreaController);
		}
			
		private void SetContentViewBounds()
		{
			var sidebarBounds = View.Bounds;
			if (ContentAreaController.View.Bounds.Equals(sidebarBounds))
				return;
			ContentAreaController.View.Bounds = sidebarBounds;
		}

		private void SetContentViewPosition()
		{
			var sidebarBounds = View.Bounds;
			if (IsOpen)
				sidebarBounds.X = MenuWidth;
			ContentAreaController.View.Layer.AnchorPoint = new PointF(.5f, .5f);
			if (ContentAreaController.View.Frame.Location.Equals(sidebarBounds.Location))
				return;
			var sidebarCenter = new PointF(sidebarBounds.Left + sidebarBounds.Width / 2, sidebarBounds.Top + sidebarBounds.Height / 2);
			ContentAreaController.View.Center = sidebarCenter;
		}
			
		private void AddMenuViewToSidebar()
		{
			SetMenuViewPosition();
			View.AddSubview(MenuAreaController.View);
			View.SendSubviewToBack(MenuAreaController.View);
		}

		private void SetMenuViewPosition() {
			var menuFrame = MenuAreaController.View.Frame;
			menuFrame.X = MenuLocation == MenuLocations.Left ? 0 : View.Frame.Width - MenuWidth;
			menuFrame.Width = MenuWidth;
            menuFrame.Height = View.Frame.Height;
			MenuAreaController.View.Frame = menuFrame;
		}

		private void AttachSidebarControllerToRootController(UIViewController rootViewController) {
			rootViewController.AddChildViewController(this);
			rootViewController.View.AddSubview(this.View);
			this.DidMoveToParentViewController(rootViewController);
		}


		/// <summary>
		/// Ensures that the menu view gets properly positioned.
		/// </summary>
		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			SetMenuViewPosition();
		}

		/// <summary>
		/// Ensures that the menu view gets properly positioned.
		/// </summary>
		public override void ViewWillAppear(bool animated)
		{
			View.SetNeedsLayout();
			base.ViewWillAppear(animated);
		}


		private bool _openWhenRotated = false;

		/// <summary>
		/// Overridden to handle reopening the menu after rotation.
		/// </summary>
		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
			if (IsOpen)
				_openWhenRotated = true;
			_sidebar.CloseMenu(false);
		}

		/// <summary>
		/// Overridden to handle reopening the menu after rotation.
		/// </summary>
		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			if (_openWhenRotated && ReopenOnRotate)
				_sidebar.OpenMenu();
			_openWhenRotated = false;
		}
	}
}

