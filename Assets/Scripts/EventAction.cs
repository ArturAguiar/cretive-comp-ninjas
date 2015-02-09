using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class EventAction {

	public enum Type {
		WAIT,
		DIALOG,
		STORY_EVENT
	}

	private Text subtitles;
	private TextToSpeech speech;

	private const string REGEX = "(\\[[^\\[]*\\])|(\\([^\\(]*\\))";
	private const float TEXT_SPEED = 20.0f; // characters per second

	private Type type;
	private float amount;
	private Action<float> executeOnFinish;
	private string rawText;
	private string cleanText;
	private EventAction next = null;

	private float startTime = -1.0f;

	private int tickCount = 0;


	private float lastCharTimestamp;
	private int charCounter = 0;

	public EventAction(Controller controller, Type type, float amount, string text = "", Action<float> executeOnFinish = null)
	{
		this.subtitles = controller.GetSubtitles();
		this.speech = controller.GetTextToSpeech();

		this.type = type;
		this.rawText = text;
		this.cleanText = Regex.Replace(text, REGEX, "");
		this.amount = amount;
		this.executeOnFinish = executeOnFinish;
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
				lastCharTimestamp = Time.time;
			}
			else if (tickCount >= 30 && startTime <= 0.0f && speech.IsDoneSaying())
			{
				startTime = Time.time;
			}

			if (this.cleanText.Length > 0)
			{
				if (Time.time - lastCharTimestamp >= 1.0f / TEXT_SPEED)
				{
					charCounter++;
					lastCharTimestamp = Time.time;
				}
				//int numChars = (int)(tickCount * TEXT_SPEED);
				//numChars = Mathf.Clamp(numChars, 0, cleanText.Length);
				charCounter = Mathf.Clamp(charCounter, 0, cleanText.Length);
				
				SetSubtitle(this.cleanText.Substring(0, charCounter));
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

			if (executeOnFinish != null)
			{
				executeOnFinish.Invoke(Time.time);
			}
			
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
