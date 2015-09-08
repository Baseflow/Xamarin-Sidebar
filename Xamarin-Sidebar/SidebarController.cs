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
	public class SidebarController : UIViewController
	{
		public const float DefaultFlingPercentage = 0.5f;
    	public const float DefaultFlingVelocity = 800f;
		public const int DefaultMenuWidth = 260;
		public const int DefaultGestureActiveArea = 50;

		public enum MenuLocations{
			Left = 1,
			Right
		}

		#region Private Fields

		private nfloat _slideSpeed = 0.2f;
		private bool _isIos7 = false;
		private bool _isOpen = false;
		private bool _shadowShown;
		private bool _openWhenRotated = false;

		// for swipe gesture
		private nfloat _panOriginX;
		private bool _ignorePan;

		// gesture recognizers
		private UITapGestureRecognizer _tapGesture;
		private UIPanGestureRecognizer _panGesture;

		private UIImageView _statusImage;

		private event UITouchEventArgs _shouldReceiveTouch;

		#endregion

		#region Public Properties

		/// <summary>
		/// The view shown in the content area.
		/// </summary>
		public UIViewController ContentAreaController { get; private set; }

        /// <summary>
        /// Determines the percent of width to complete slide action.
        /// </summary>
        public float FlingPercentage { get; set; }

        /// <summary>
        /// Determines the minimum velocity considered a "fling" to complete slide action.
        /// </summary>
        public float FlingVelocity { get; set; }

		/// <summary>
		/// Active area where the Pan gesture is intercepted.
		/// </summary>
		public float GestureActiveArea { get; set; }

		bool disabled;
		/// <summary>
		/// Disables all open/close actions when set to true.
		/// </summary>
		public bool Disabled {
			get {
				return disabled;
			}
			set {
				if (value) {
					_panGesture.Enabled = false;
					_tapGesture.Enabled = false;
				} else {
					_panGesture.Enabled = true;
					_tapGesture.Enabled = true;
				}
				disabled = value;
			}
		}

		/// <summary>
		/// The view controller for the side menu.
		/// This is what will be shown when the menu is displayed.
		/// </summary>
		public UIViewController MenuAreaController { get; private set; }

		/// <summary>
		/// Determines if the status bar should be made static.
		/// </summary>
		/// <value>True to make the status bar static, false to make it move with the content area.</value>
		public bool StatusBarMoves { get; set; }

		/// <summary>
		/// Gets or sets a value indicating whether there should be shadowing effects on the content view.
		/// </summary>
		public bool HasShadowing { get; set; }

		/// <summary>
		/// Determines if the menu should be reopened after the screen is roated.
		/// </summary>
		public bool ReopenOnRotate { get; set; }

		/// <summary>
		/// Determines the width of the menu when open.
		/// </summary>
		public int MenuWidth { get; set; }

		/// <summary>
		/// Determines if the menu is on the left or right of the screen.
		/// </summary>
		public MenuLocations MenuLocation { get; set; }

		/// <summary>
		/// This Event will be called when the Side Menu is Opened/closed( at the end of the animation)
		/// The Event Arg is a Boolean = isOpen.
		/// </summary>
		public event EventHandler<bool> StateChangeHandler;

		/// <summary>
		/// Gets the current state of the menu.
		/// Setting this property will open/close the menu respectively.
		/// </summary>
		public bool IsOpen
		{
			get { return _isOpen; }
			set
			{
				_isOpen = value;
				if (value)
					CloseMenu();
				else
					OpenMenu();
			}
		}
			
		#endregion

		#region Private Properties

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

		#endregion

		#region Constructors

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
		public SidebarController(UIViewController rootViewController, UIViewController contentAreaController, UIViewController navigationAreaController)
		{
			Initialize(contentAreaController, navigationAreaController);

			// handle wiring things up so events propogate properly
			rootViewController.AddChildViewController(this);
			rootViewController.View.AddSubview(this.View);
			this.DidMoveToParentViewController(rootViewController);
		}
			
		#endregion

		#region Public Methods

		/// <summary>
		/// Toggles the menu open or closed.
		/// </summary>
		public void ToggleMenu()
		{
			if (IsOpen)
				CloseMenu();
			else
				OpenMenu();
		}

		/// <summary>
		/// Shows the slideout navigation menu.
		/// </summary>
		public void OpenMenu()
		{
			if (IsOpen || Disabled)
				return;
			ShowShadow(5);
			var view = _contentAreaView;
			view.EndEditing(true);
			UIView.Animate(
				_slideSpeed, 
				0, 
				UIViewAnimationOptions.CurveEaseInOut,
				() => { 
					if(MenuLocation == MenuLocations.Right){
					view.Frame = new RectangleF (-MenuWidth, 0, view.Frame.Width, view.Frame.Height);
					}else if(MenuLocation == MenuLocations.Left){
					view.Frame = new RectangleF (MenuWidth, 0, view.Frame.Width, view.Frame.Height);
					}
				},
				() => {
					if (view.Subviews.Length > 0)
						view.Subviews[0].UserInteractionEnabled = false;
					view.AddGestureRecognizer(_tapGesture);
					_isOpen = true;
					if (StateChangeHandler != null) {
						StateChangeHandler.Invoke(this, _isOpen);
					}
				});
		}

		/// <summary>
		/// Hides the slideout navigation menu.
		/// </summary>
		public void CloseMenu(bool animate = true)
		{
			if (!IsOpen || Disabled)
				return;
			MenuAreaController.View.EndEditing(true);
			var view = _contentAreaView;
			#if __UNIFIED__
			Action animation;
			Action finished;
			#else
			NSAction animation;
			NSAction finished;
			#endif
			animation = () => { view.Frame = new RectangleF (0, 0, view.Frame.Width, view.Frame.Height); };
			finished = () => {
				if (view.Subviews.Length > 0)
					view.Subviews[0].UserInteractionEnabled = true;
				view.RemoveGestureRecognizer (_tapGesture);
				_isOpen = false;
				if (StateChangeHandler != null) {
					StateChangeHandler.Invoke(this, _isOpen);
				}
			};
			if (animate)
				UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut, animation, finished);
			else {
				// fire the animation results manually
				animation();
				finished();
			}
			HideShadow();
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
				if (_tapGesture != null && IsOpen)
					_contentAreaView.RemoveGestureRecognizer (_tapGesture);
				if (_panGesture != null)
					_contentAreaView.RemoveGestureRecognizer (_panGesture); 
			}
			if (_contentAreaView != null)
				_contentAreaView.RemoveFromSuperview();
            if (ContentAreaController != null)
                ContentAreaController.RemoveFromParentViewController ();
            
			ContentAreaController = newContentView;
			SetVisibleContentView();
			CloseMenu();
			// setup a tap gesture to close the menu on root view tap
			_tapGesture = new UITapGestureRecognizer ();
			_tapGesture.AddTarget (() => CloseMenu());
			_tapGesture.NumberOfTapsRequired = 1;
			_panGesture = new UIPanGestureRecognizer {
				Delegate = new SlideoutPanDelegate(),
				MaximumNumberOfTouches = 1,
				MinimumNumberOfTouches = 1
			};
			_panGesture.AddTarget (() => Pan (_contentAreaView));
			_contentAreaView.AddGestureRecognizer(_panGesture);
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

			MenuAreaController = newMenuView;
			SetVisibleMenuView();
		}

		#endregion

		#region Private Methods

		private void Initialize(UIViewController currentViewController, UIViewController navigationViewController)
		{
			ContentAreaController = currentViewController;
			MenuAreaController = navigationViewController;

            // set default fling percentage
            FlingPercentage = DefaultFlingPercentage;

            // set default fling velocity
            FlingVelocity = DefaultFlingVelocity;

			// set default gesture active area
			GestureActiveArea = DefaultGestureActiveArea;

			// place menu on right by default
			MenuLocation = MenuLocations.Right;

			// set default menu width
			MenuWidth = DefaultMenuWidth;

			// enable shadow by default
			HasShadowing = true;

			// enable menu reopening on rotate by default
			ReopenOnRotate = true;

			// make the status bar static
			StatusBarMoves = true;
			_statusImage = new UIImageView();

			// set iOS 7 flag
			var version = new System.Version(UIDevice.CurrentDevice.SystemVersion);
			_isIos7 = version.Major >= 7;

			// add the navigation view on the right
			RectangleF navigationFrame;
			navigationFrame = MenuAreaController.View.Frame;
			navigationFrame.X = navigationFrame.Width - MenuWidth;
			navigationFrame.Width = MenuWidth;
			MenuAreaController.View.Frame = navigationFrame;
	
			View.AddSubview(_menuAreaView);
			ChangeContentView(currentViewController);
		}

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
				GetStatusImage();
				var statusFrame = _statusImage.Frame;
				statusFrame.X = _contentAreaView.Frame.X;
				_statusImage.Frame = statusFrame;
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

		/// <summary>
		/// Pan the specified view.
		/// </summary>
		private void Pan(UIView view)
		{
			if (Disabled)
				return;
			if (_panGesture.State == UIGestureRecognizerState.Began) {
				_panOriginX = view.Frame.X;
				if (MenuLocation == MenuLocations.Left)
					_ignorePan = _panGesture.LocationInView(view).X > this.GestureActiveArea;
				else
					_ignorePan = _panGesture.LocationInView(view).X < view.Bounds.Width - this.GestureActiveArea;
			} else if (!_ignorePan && (_panGesture.State == UIGestureRecognizerState.Changed)) {
				var t = _panGesture.TranslationInView(view).X;
				if (MenuLocation == MenuLocations.Left) {
					if ((t > 0 && !IsOpen) || (t < 0 && IsOpen)) {
						if (t > MenuWidth)
							t = MenuWidth;
						else if (t < -MenuWidth && IsOpen)
							t = MenuWidth; 
						if (_panOriginX + t <= MenuWidth)
							view.Frame = new RectangleF(_panOriginX + t, view.Frame.Y, view.Frame.Width, view.Frame.Height);
						ShowShadowWhileDragging();
					}
				} else if (MenuLocation == MenuLocations.Right) {
					if ((t < 0 && !IsOpen) || (t > 0 && IsOpen)) {
						if (t < -MenuWidth)
							t = -MenuWidth;
						else if (t > MenuWidth)
							t = MenuWidth; 
						if (_panOriginX + t <= 0)
							view.Frame = new RectangleF(_panOriginX + t, view.Frame.Y, view.Frame.Width, view.Frame.Height);
						ShowShadowWhileDragging();
					}
				}
			} else if (!_ignorePan && (_panGesture.State == UIGestureRecognizerState.Ended || _panGesture.State == UIGestureRecognizerState.Cancelled)) {
				var t = _panGesture.TranslationInView(view).X;
				var velocity = _panGesture.VelocityInView(view).X;
				if ((MenuLocation == MenuLocations.Left && IsOpen && t < 0) || (MenuLocation == MenuLocations.Right && IsOpen && t > 0)) {
					if (view.Frame.X > -view.Frame.Width / 2) {
						CloseMenu();
					} else {
						UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
							() => {
								view.Frame = new RectangleF(-MenuWidth, view.Frame.Y, view.Frame.Width, view.Frame.Height);
							}, () => {
						});
					}
                } if ((MenuLocation == MenuLocations.Left && (velocity > FlingVelocity || view.Frame.X > (MenuWidth * FlingPercentage)))
                    || (MenuLocation == MenuLocations.Right && (velocity < -FlingVelocity || view.Frame.X < -(MenuWidth * FlingPercentage)))) {
                    OpenMenu();
                } else {
                    UIView.Animate(_slideSpeed, 0, UIViewAnimationOptions.CurveEaseInOut,
                        () =>
                        {
                            view.Frame = new RectangleF(0, 0, view.Frame.Width, view.Frame.Height);
                        }, () =>
                        {
                        });
                }
			}
		}

		/// <summary>
		/// Shows the shadow of the root view while dragging.
		/// </summary>
		private void ShowShadowWhileDragging()
		{
			if (!HasShadowing)
				return;

			// don't add an offset if the menu is on the left
			var xOffset = (MenuLocation == MenuLocations.Left) ? -5 : 5;

			_contentAreaView.Layer.ShadowOffset = new SizeF(xOffset, 0);
			_contentAreaView.Layer.ShadowPath = UIBezierPath.FromRect (_contentAreaView.Bounds).CGPath;
			_contentAreaView.Layer.ShadowRadius = 4.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.5f;
			_contentAreaView.Layer.ShadowColor = UIColor.Black.CGColor;
		}

		/// <summary>
		/// Shows the shadow.
		/// </summary>
		private void ShowShadow(float position)
		{
			//Dont need to call this twice if its already shown
			if (!HasShadowing || _shadowShown)
				return;

			// don't add an offset if the menu is on the left
			position = (MenuLocation == MenuLocations.Left) ? -position : position;

			_contentAreaView.Layer.ShadowOffset = new SizeF(position, 0);
			_contentAreaView.Layer.ShadowPath = UIBezierPath.FromRect (_contentAreaView.Bounds).CGPath;
			_contentAreaView.Layer.ShadowRadius = 4.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.5f;
			_contentAreaView.Layer.ShadowColor = UIColor.Black.CGColor;
			_shadowShown = true;
		}

		/// <summary>
		/// Hides the shadow of the root view.
		/// </summary>
		private void HideShadow()
		{
			//Dont need to call this twice if its already hidden
			if (!HasShadowing || !_shadowShown)
				return;
			_contentAreaView.Layer.ShadowOffset = new SizeF (0, 0);
			_contentAreaView.Layer.ShadowRadius = 0.0f;
			_contentAreaView.Layer.ShadowOpacity = 0.0f;
			_contentAreaView.Layer.ShadowColor = UIColor.Clear.CGColor;
			_shadowShown = false;
		}

		/// <summary>
		/// Places the static status image.
		/// </summary>
		private void GetStatusImage()
		{
			if (StatusBarMoves || !_isIos7 || _statusImage.Superview != null)
				return;
			this.View.AddSubview(_statusImage);
			_statusImage.Image = CaptureStatusBarImage();
			_statusImage.Frame = UIApplication.SharedApplication.StatusBarFrame;
			UIApplication.SharedApplication.StatusBarHidden = true;
		}

		/// <summary>
		/// Gets a static image of the status bar.
		/// </summary>
		private UIImage CaptureStatusBarImage()
		{
			var frame = UIApplication.SharedApplication.StatusBarFrame;
			frame.Width *= 2;
			frame.Height *= 2;
			var image = CGImage.ScreenImage;
			image = image.WithImageInRect(frame);
			var newImage = new UIImage(image).Scale(UIApplication.SharedApplication.StatusBarFrame.Size, 2f);
			return newImage;
		}

		/// <summary>
		/// Hides the static status bar image.
		/// </summary>
		private void HideStatusImage()
		{
			if (!_isIos7)
				return;
			_statusImage.RemoveFromSuperview();
			UIApplication.SharedApplication.StatusBarHidden = false;
		}

		/// <summary>
		/// Hides the static status bar when the close animation completes
		/// </summary>
		[Export("animationEnded")]
		private void HideComplete()
		{
			HideStatusImage();
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
			this.CloseMenu(false);
		}

		public override void DidRotate(UIInterfaceOrientation fromInterfaceOrientation)
		{
			base.DidRotate(fromInterfaceOrientation);
			if (_openWhenRotated && ReopenOnRotate)
				OpenMenu();
			_openWhenRotated = false;
		}
	}
}

