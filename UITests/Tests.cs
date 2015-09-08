using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.iOS;
using Xamarin.UITest.Queries;
using System.Threading;

namespace UITests
{
	[TestFixture]
	public class Tests
	{
		iOSApp app;


		AppQuery ToggleButton(AppQuery app) { return app.All().Button("Toggle Side Menu"); }

		AppQuery IntroTitle(AppQuery app) { return app.Class("UILabel").Marked("Sidebar Navigation"); }

		AppQuery ContentTitle(AppQuery app) { return app.Class("UILabel").Marked("Content"); }

		AppQuery MenuIntroButton(AppQuery app) { return app.Button("Intro"); }
		AppQuery MenuContentButton(AppQuery app) { return app.Button("Content"); }

		[TestFixtureSetUp]
		public void BeforeAnyTests() {
			// TODO: If the iOS app being tested is included in the solution then open
			// the Unit Tests window, right click Test Apps, select Add App Project
			// and select the app projects that should be tested.
			app = ConfigureApp
				.iOS
				// TODO: Update this path to point to your iOS app and uncomment the
				// code if the app is not included in the solution.
				//.AppBundle ("../../../iOS/bin/iPhoneSimulator/Debug/UITests.iOS.app")
				.StartApp();
		}

		[SetUp]
		public void BeforeEachTest()
		{
			var introButtonVisible = app.Query(MenuIntroButton).Any();
			if (introButtonVisible)
				app.Tap(MenuIntroButton);
		}


		[Test]
		public void CanOpenSideMenuWithButton()
		{
			AssertMenuClosed();
			app.Tap(ToggleButton);
			AssertMenuOpen();
		}

		[Test]
		public void CanOpenSideMenuWithSlide()
		{
			AssertMenuClosed();
			app.DragCoordinates(15, 50, 305, 50, TimeSpan.FromMilliseconds(1400));
			AssertMenuOpen();
		}

		[Test]
		public void CanCloseMenuWithTap()
		{
			AssertMenuClosed();
			app.Tap(ToggleButton);
			AssertMenuOpen();
			app.TapCoordinates(315, 50);
			AssertMenuClosed();
		}

		[Test]
		public void CanCloseMenuWithSlide()
		{
			AssertMenuClosed();
			app.Tap(ToggleButton);
			AssertMenuOpen();
			app.DragCoordinates(305, 50, 15, 50, TimeSpan.FromMilliseconds(1000));
			AssertMenuClosed();
		}

		[Test]
		public void CanOpenMenuWithFlick()
		{
			AssertMenuClosed();
			app.DragCoordinates(15, 50, 200, 50, TimeSpan.FromMilliseconds(800));
			AssertMenuOpen();
		}

		[Test]
		public void CanCloseMenuWithFlick()
		{
			AssertMenuClosed();
			app.Tap(ToggleButton);
			AssertMenuOpen();
			app.DragCoordinates(305, 50, 200, 50, TimeSpan.FromMilliseconds(800));
			AssertMenuClosed();
		}

		[Test]
		public void CanSwitchToContentViewAndBackToIntro()
		{
			Assert.Greater(app.Query(IntroTitle).Length, 0);
			app.Tap(ToggleButton);
			app.Tap(MenuContentButton);
			Assert.Greater(app.Query(ContentTitle).Length, 0);
			app.Tap(ToggleButton);
			app.Tap(MenuIntroButton);
			Assert.Greater(app.Query(IntroTitle).Length, 0);
		}


		private void AssertMenuOpen() {
			Assert.AreEqual(310, app.Query(ToggleButton).First().Rect.X);
		}

		private void AssertMenuClosed() {
			Assert.AreEqual(50, app.Query(ToggleButton).First().Rect.X);
		}
	}
}

