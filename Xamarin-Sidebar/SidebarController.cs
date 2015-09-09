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

		#region Private Fields

		private Sidebar _sidebar;

		private bool _openWhenRotated = false;




		private event UITouchEventArgs _shouldReceiveTouch;

		#endregion

		#region Public Properties

		/// <summary>
		/// The view shown in the content area.
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
		/// Disables all open/close actions when set to true.
		/// </summary>
		public bool Disabled {
			get { return _sidebar.Disabled; }
			set { _sidebar.Disabled = value; }
		}
			
		/// <summary>
		/// Determines if the status bar should be made static.
		/// </summary>
		/// <value>True to make the status bar static, false to make it move with the content area.</value>
		public bool StatusBarMoves {
			get { return _sidebar.StatusBarMoves; }
			set { _sidebar.StatusBarMoves = value; }
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
		/// This Event will be called when the Side Menu is Opened/closed( at the end of the animation)
		/// The Event Arg is a Boolean = isOpen.
		/// </summary>
		public event EventHandler<bool> StateChangeHandler;
			

		/// <summary>
		/// The UIView of the content view controller.
		/// </summary>
		private UIView _contentAreaView
		{
			get
			{
				if (ContentAreaController == null)
					return null;
				return ContentAreaController.View;
			}
		}

		/// <summary>
		/// The UIView of the menu view controller.
		/// </summary>
		private UIView _menuAreaView
		{
			get
			{
				if (MenuAreaController == null)
					return null;
				return MenuAreaController.View;
			}
		}


		/// <summary>
		/// Required contructor.
		/// </summary>
		public SidebarController(IntPtr handle) : base(handle)
		{
		}

		/// <summary>
		/// Contructor.
		/// </summary>
		/// <param name="contentAreaController">
		/// The view controller for the content area.
		/// </param>
		/// <param name="navigationAreaController">
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
			if (_contentAreaView != null)
			{
				_contentAreaView.RemoveFromSuperview();

				// Ensure the old gesture recognizers are removed from the view to prevent usability conflicts
				// especially if this view is re shown at a later time.
				if (_sidebar.TapGesture != null && IsOpen)
					_contentAreaView.RemoveGestureRecognizer (_sidebar.TapGesture);
				if (_sidebar.PanGesture != null)
					_contentAreaView.RemoveGestureRecognizer (_sidebar.PanGesture); 
			}
			if (_contentAreaView != null)
				_contentAreaView.RemoveFromSuperview();
            if (ContentAreaController != null)
                ContentAreaController.RemoveFromParentViewController ();
            
			_sidebar.ContentViewController = newContentView;
			SetVisibleContentView();
			_sidebar.CloseMenu();
			// setup a tap gesture to close the menu on root view tap
			_sidebar.TapGesture = new UITapGestureRecognizer ();
			_sidebar.TapGesture.AddTarget (() => _sidebar.CloseMenu());
			_sidebar.TapGesture.NumberOfTapsRequired = 1;
			_sidebar.PanGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			_sidebar.PanGesture.AddTarget (() => _sidebar.Pan());
			_contentAreaView.AddGestureRecognizer(_sidebar.PanGesture);
		}
			
		/// <summary>
		/// Replaces the menu area view controller with the specified view controller.
		/// </summary>
		/// <param name="newContentView">
		/// New menu view.
		/// </param>
		public void ChangeMenuView(UIViewController newMenuView) {
			if (_menuAreaView != null)
				_menuAreaView.RemoveFromSuperview();
			if (MenuAreaController != null)
				MenuAreaController.RemoveFromParentViewController ();

			_sidebar.MenuViewController = newMenuView;
			SetVisibleMenuView();
		}

		#endregion

		#region Private Methods

		/// <summary>
		/// Places the root view on top of the navigation view.
		/// </summary>
		private void SetVisibleContentView()
		{
			if(!StatusBarMoves)
				UIApplication.SharedApplication.SetStatusBarHidden(false, UIStatusBarAnimation.Fade);

			RectangleF frame = View.Bounds;
			if (IsOpen)
				frame.X = MenuWidth;

			SetViewSize();
			SetLocation(frame);

			View.AddSubview(_contentAreaView);
			AddChildViewController(ContentAreaController);
		}

		/// <summary>
		/// Sets the size of the root view.
		/// </summary>
		private void SetViewSize()
		{
			RectangleF frame = View.Bounds;
			if (_contentAreaView.Bounds == frame)
				return;
			_contentAreaView.Bounds = frame;
		}

		/// <summary>
		/// Sets the location of the root view.
		/// </summary>
		/// <param name="frame">Frame.</param>
		private void SetLocation(RectangleF frame)
		{
			frame.Y = 0;
			_contentAreaView.Layer.AnchorPoint = new PointF (.5f, .5f);

			// exit if we're already at the desired location
			if (_contentAreaView.Frame.Location == frame.Location)
				return;

			frame.Size = _contentAreaView.Frame.Size;

			// set the root views cetner
			var center = new PointF(frame.Left + frame.Width / 2,
				frame.Top + frame.Height / 2);
			_contentAreaView.Center = center;

			// if x is greater than 0 then position the status view
			if (Math.Abs(frame.X - 0) > float.Epsilon)
			{
				
			}
		}

		/// <summary>
		/// Places the root view on top of the navigation view.
		/// </summary>
		private void SetVisibleMenuView()
		{
			RectangleF navigationFrame;
			navigationFrame = MenuAreaController.View.Frame;
			navigationFrame.X = navigationFrame.Width - MenuWidth;
			navigationFrame.Width = MenuWidth;
			MenuAreaController.View.Frame = navigationFrame;
			View.AddSubview(_menuAreaView);
			View.SendSubviewToBack(_menuAreaView);
		}









		private void AttachSidebarControllerToRootController(UIViewController rootViewController) {
			rootViewController.AddChildViewController(this);
			rootViewController.View.AddSubview(this.View);
			this.DidMoveToParentViewController(rootViewController);
		}

		/// <summary>
		/// Hides the static status bar when the close animation completes
		/// </summary>
		[Export("animationEnded")]
		private void HideComplete()
		{
			_sidebar.HideStatusBarImage();
		}

		/// <summary>
		/// Should the receive touch.
		/// </summary>
		internal bool ShouldReceiveTouch(UIGestureRecognizer gesture, UITouch touch)
		{
			if (_shouldReceiveTouch != null)
				return _shouldReceiveTouch(gesture, touch);
			return true;
		}

		#endregion

		private class SlideoutPanDelegate : UIGestureRecognizerDelegate
		{
			public override bool ShouldReceiveTouch (UIGestureRecognizer recognizer, UITouch touch)
			{
				return true;
			}
		}

		public override void ViewDidLayoutSubviews()
		{
			base.ViewDidLayoutSubviews();
			RectangleF navigationFrame = View.Bounds;

			if (MenuLocation == MenuLocations.Right) {
				navigationFrame.X = navigationFrame.Width - MenuWidth;
			} else if (MenuLocation == MenuLocations.Left) {
				navigationFrame.X = 0;
			}
			navigationFrame.Width = MenuWidth;
			if (MenuAreaController.View.Frame != navigationFrame)
				MenuAreaController.View.Frame = navigationFrame;
		}

		public override void ViewWillAppear(bool animated)
		{
			RectangleF navigationFrame = MenuAreaController.View.Frame;
			if (MenuLocation == MenuLocations.Right) {
				navigationFrame.X = navigationFrame.Width - MenuWidth;
			} else if (MenuLocation == MenuLocations.Left) {
				navigationFrame.X = 0;
			}
			navigationFrame.Width = MenuWidth;
			navigationFrame.Location = PointF.Empty;
			MenuAreaController.View.Frame = navigationFrame;
			View.SetNeedsLayout();
			base.ViewWillAppear(animated);
		}

		public override void WillRotate(UIInterfaceOrientation toInterfaceOrientation, double duration)
		{
			base.WillRotate(toInterfaceOrientation, duration);
			if (IsOpen)
				_openWhenRotated = true;
			_sidebar.CloseMenu(false);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			if (_openWhenRotated && ReopenOnRotate)
				_sidebar.OpenMenu();
			_openWhenRotated = false;
		}
	}
}

