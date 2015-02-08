using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Text.RegularExpressions;

public class EventAction {

	public enum Type {
		WAIT,
		DIALOG,
		CHOICE
	}

	private Text subtitles;
	private TextToSpeech speech;

	private const string REGEX = "(\\[.*\\])|(\".*\")|('.*')|(\\(.*\\))";
	private const float TEXT_SPEED = 0.05f;

	private Type type;
	private float amount;
	private string rawText;
	private string cleanText;
	private EventAction next = null;

	private float startTime = -1.0f;

	private int tickCount = 0;

	public EventAction(Controller controller, Type type, float amount, string text = "")
	{
		this.subtitles = controller.GetSubtitles();
		this.speech = controller.GetTextToSpeech();

		this.type = type;
		this.rawText = text;
		this.cleanText = Regex.Replace(text, REGEX, "");
		this.amount = amount;
	}

	public EventAction Tick()
	{
		switch (type)
		{
		case Type.DIALOG:
			if (tickCount == 0)
			{
				//SpeakAndSub();
				Speak(this.rawText);
			}
			else if (tickCount >= 30 && startTime <= 0.0f && speech.IsDoneSaying())
			{
				startTime = Time.time;
			}

			if (this.cleanText.Length > 0)
			{
				int numChars = (int)(tickCount * TEXT_SPEED);
				numChars = Mathf.Clamp(numChars, 0, cleanText.Length);
				
				SetSubtitle(this.cleanText.Substring(0, numChars));
			}
			break;

		case Type.WAIT:
			if (tickCount == 0)
				startTime = Time.time;
			break;

		default:
			break;
		}

		tickCount++;
		
		if (startTime >= 0.0f && Time.time - startTime >= amount)
		{
			SetSubtitle("");
			
			return this.next;
		}

		return this;
	}

	public EventAction Then(EventAction nextAction)
	{
		this.next = nextAction;

		return this.next;
	}

	private void SetSubtitle(string text)
	{
		subtitles.text = text;
	}

	private void Speak(string text)
	{
		speech.SayText(text);
	}

	private void SpeakAndSub()
	{
		SetSubtitle(cleanText);
		Speak(this.rawText);
	}
}
