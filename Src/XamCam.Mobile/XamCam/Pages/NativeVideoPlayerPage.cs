﻿using System;
using System.Threading.Tasks;

using Xamarin.Forms;

using Plugin.MediaManager;
using Plugin.MediaManager.Forms;
using Plugin.MediaManager.Abstractions.Enums;
using Plugin.MediaManager.Abstractions.EventArguments;

namespace XamCam
{
	public class NativeVideoPlayerPage : BaseContentPage<NativeVideoPlayerViewModel>
	{
		#region Constant Fields
		readonly Grid grid;
		readonly Button playButton;
		readonly Slider progressBar;
		readonly VideoView videoView;
		readonly ContentView clickContainer;
		const uint fadeDuration = 500;
		#endregion

		#region Fields
		double fadedOpacity = 0.6;
		double videoProgress = 0;
		bool propertyCheck = false;
		#endregion

		#region Constructors
		public NativeVideoPlayerPage(string url)
		{
			Title = "Native Video Player";

			videoView = new VideoView
			{
				Source = url,
				AspectMode = VideoAspectMode.AspectFit
			};

			playButton = new Button
			{
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center
			};

			grid = new Grid
			{
				RowSpacing = 0,
				ColumnSpacing = 0,
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = App.ScreenWidth * (9.0 / 16.0) },
					new RowDefinition { Height = GridLength.Star }
				},
				ColumnDefinitions = new ColumnDefinitionCollection
				{
					new ColumnDefinition { Width = 20 },
					new ColumnDefinition { Width = GridLength.Star },
					new ColumnDefinition { Width = 20 }
				}
			};
			clickContainer = new ContentView
			{
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.FillAndExpand,
				BackgroundColor = Color.Transparent
			};
			progressBar = new Slider
			{
				Minimum = 0,
				Maximum = 1,
				Margin = new Thickness(10, 0, 10, 10),
				HorizontalOptions = LayoutOptions.FillAndExpand,
				VerticalOptions = LayoutOptions.End
			};

			grid.Children.Add(videoView, 0, 3, 0, 1);
			grid.Children.Add(clickContainer, 0, 3, 0, 1);
			grid.Children.Add(progressBar, 1, 2, 0, 1);
			grid.Children.Add(playButton, 1, 2, 0, 1);

			var tapGesture = new TapGestureRecognizer();
			tapGesture.Tapped += TogglePlaybackButtonClicked;

			clickContainer.GestureRecognizers.Add(tapGesture);
			playButton.SetBinding(Button.ImageProperty, nameof(NativeVideoPlayerViewModel.ButtonImageSource));

			Content = grid;

			ResetFade(true);
		}
		#endregion

		bool rotationLock;

		#region Methods
		protected override void OnSizeAllocated(double width, double height)
		{
			base.OnSizeAllocated(width, height);

			if (rotationLock) return;
			rotationLock = true;

			switch (width > height)
			{
				case true:
					SetLandscapeMode();
					break;

				default:
					SetPortraitMode();
					break;
			}

			rotationLock = false;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			playButton.Clicked += TogglePlaybackButtonClicked;
			progressBar.ValueChanged += ProgressBarValueChanged;
			progressBar.PropertyChanging += ProgressBar_PropertyChanging;
			progressBar.PropertyChanged += ProgressBar_PropertyChanged;
			CrossMediaManager.Current.PlayingChanged += VideoViewProgressChanged;
		}

		protected override async void OnDisappearing()
		{
			base.OnDisappearing();

			playButton.Clicked -= TogglePlaybackButtonClicked;
			progressBar.ValueChanged -= ProgressBarValueChanged;
			CrossMediaManager.Current.PlayingChanged -= VideoViewProgressChanged;
			progressBar.PropertyChanging -= ProgressBar_PropertyChanging;
			progressBar.PropertyChanged -= ProgressBar_PropertyChanged;

			ViewModel.CancelTokenSource.Cancel();
			await CrossMediaManager.Current.Stop();
			ViewExtensions.CancelAnimations(progressBar);
		}

		void ProgressBar_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e) => propertyCheck = false;
		void ProgressBar_PropertyChanging(object sender, PropertyChangingEventArgs e) => propertyCheck = true;

		void VideoViewProgressChanged(object sender, PlayingChangedEventArgs e)
		{
			//Need to cached video progress to prevent infinite loop when user drags Slider
			videoProgress = Math.Round(e.Progress, 2);

			Device.BeginInvokeOnMainThread(() => progressBar.Value = videoProgress);

			if (e.Progress == 0)
			{
				DisplayFadedContainer();
			}
		}

		async void ProgressBarValueChanged(object sender, ValueChangedEventArgs e)
		{
			var newValue = Math.Round(e.NewValue, 2);
			if (newValue == videoProgress)
				return;

			videoProgress = e.NewValue;

			System.Diagnostics.Debug.WriteLine($"Progress bar changed: {CrossMediaManager.Current.Status}");

			await ViewModel.SeekTo(videoProgress);

			if (!propertyCheck)
			{
				if (CrossMediaManager.Current.Status == MediaPlayerStatus.Playing)
					DisplayFadedContainer();
				await BeginToFadeContainer();
			}
		}

		async void TogglePlaybackButtonClicked(object sender, EventArgs e)
		{
			var isPlaying = await ViewModel.TogglePlayback();

			ResetFade(isPlaying);
		}

		void DisplayFadedContainer()
		{
			ViewExtensions.CancelAnimations(clickContainer);
			ViewExtensions.CancelAnimations(progressBar);
			ViewExtensions.CancelAnimations(playButton);

			ViewModel.UpdateButtonSource();

			Device.BeginInvokeOnMainThread(async () =>
			{
				{
					progressBar.Opacity = 1;
					playButton.Opacity = 1;

					if (CrossMediaManager.Current.VideoPlayer.Status != MediaPlayerStatus.Playing)
					{
						clickContainer.Opacity = 0;
						clickContainer.BackgroundColor = Color.Black;
					}

					await Task.WhenAll(
							playButton.FadeTo(1, fadeDuration),
							progressBar.FadeTo(1, fadeDuration),
							clickContainer.FadeTo(fadedOpacity, fadeDuration));
				}
			});
		}

		void ResetFade(bool restartFade = true)
		{
			if (CrossMediaManager.Current.VideoPlayer.Status == MediaPlayerStatus.Paused)
				DisplayFadedContainer();

			if (restartFade)
				Device.BeginInvokeOnMainThread(async () => await BeginToFadeContainer());
		}

		async Task BeginToFadeContainer()
		{
			int fadeDelay = 0;

			switch (Device.RuntimePlatform)
			{
				case Device.iOS:
					fadeDelay = 1000;
					break;
				case Device.Android:
					fadeDelay = 1500;
					break;
			}

			try
			{
				await Task.Delay(fadeDelay);

				if (CrossMediaManager.Current.VideoPlayer.Status != MediaPlayerStatus.Paused)
					Device.BeginInvokeOnMainThread(async () =>
					{
						await Task.WhenAll(
							playButton.FadeTo(0, fadeDuration),
							progressBar.FadeTo(0, fadeDuration),
							clickContainer.FadeTo(0, fadeDuration));

						if (CrossMediaManager.Current.VideoPlayer.Status != MediaPlayerStatus.Paused)
						{
							clickContainer.BackgroundColor = Color.Transparent;
							clickContainer.Opacity = 1;
						}
					});
			}
			catch (TaskCanceledException)
			{
			}
		}

		void SetPortraitMode()
		{
			NavigationPage.SetHasNavigationBar(this, true);

			Grid.SetRowSpan(videoView, 1);
			Grid.SetRowSpan(playButton, 1);
			Grid.SetRowSpan(progressBar, 1);
			Grid.SetRowSpan(clickContainer, 1);
		}

		void SetLandscapeMode()
		{
			NavigationPage.SetHasNavigationBar(this, false);

			Grid.SetRowSpan(videoView, 2);
			Grid.SetRowSpan(playButton, 2);
			Grid.SetRowSpan(progressBar, 2);
			Grid.SetRowSpan(clickContainer, 2);
		}
		#endregion
	}
}