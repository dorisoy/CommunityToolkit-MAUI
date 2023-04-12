using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Globalization;
using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace CommunityToolkit.Maui.Sample.ViewModels.Essentials;

public partial class SpeechToTextViewModel : BaseViewModel
{
	const string defaultLanguage = "en-US";

	readonly ITextToSpeech textToSpeech;
	readonly ISpeechToText speechToText;

	[ObservableProperty]
	Locale? locale;

	[ObservableProperty]
	string recognitionText = "Welcome to .NET MAUI Community Toolkit!";

	public SpeechToTextViewModel(ITextToSpeech textToSpeech, ISpeechToText speechToText)
	{
		this.textToSpeech = textToSpeech;
		this.speechToText = speechToText;

		Locales.CollectionChanged += HandleLocalesCollectionChanged;
	}

	public ObservableCollection<Locale> Locales { get; } = new();

	[RelayCommand]
	async Task SetLocales()
	{
		Locales.Clear();

		var locales = await textToSpeech.GetLocalesAsync();
		foreach (var locale in locales.OrderBy(x => x.Language).ThenBy(x => x.Name))
		{
			Locales.Add(locale);
		}

		Locale = Locales.FirstOrDefault(x => x.Language is defaultLanguage) ?? Locales.FirstOrDefault();
	}

	[RelayCommand]
	async Task Play(CancellationToken cancellationToken)
	{
		await textToSpeech.SpeakAsync(RecognitionText, new()
		{
			Locale = Locale,
			Pitch = 2,
			Volume = 1
		}, cancellationToken);
	}

	[RelayCommand(IncludeCancelCommand = true)]
	async Task Listen(CancellationToken cancellationToken)
	{
		const string beginSpeakingPrompt = "Begin speaking...";

		RecognitionText = beginSpeakingPrompt;

		try
		{
			RecognitionText = await speechToText.ListenAsync(CultureInfo.GetCultureInfo(Locale?.Language ?? defaultLanguage), new Progress<string>(partialText =>
			{
				if (RecognitionText is beginSpeakingPrompt)
				{
					RecognitionText = string.Empty;
				}

				RecognitionText += partialText + " ";
			}), cancellationToken);
		}
		catch (TaskCanceledException)
		{
			await Toast.Make("Listening Stopped by User").Show(CancellationToken.None);
		}
		catch (Exception e)
		{
			await Toast.Make(e.Message).Show(CancellationToken.None);
		}
		finally
		{
			if (RecognitionText is beginSpeakingPrompt)
			{
				RecognitionText = string.Empty;
			}
		}
	}

	void HandleLocalesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		OnPropertyChanged(nameof(Locale));
	}
}