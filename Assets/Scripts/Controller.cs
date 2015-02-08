using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Controller : MonoBehaviour 
{
	private EventAction currentAction;
	private TextToSpeech speech;
	private Text subtitles;

	// Use this for initialization
	void Start () 
	{
		speech = this.gameObject.GetComponent<TextToSpeech>();
		subtitles = GameObject.Find("Canvas/Subtitles").GetComponent<Text>();

		currentAction = new EventAction(
			this, EventAction.Type.WAIT, 3.5f);
		
		currentAction.Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "Are you (....)awake?")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "Oh.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "You are finally here."));
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (currentAction != null)
			currentAction = currentAction.Tick();
	}

	public Text GetSubtitles()
	{
		return subtitles;
	}

	public TextToSpeech GetTextToSpeech()
	{
		return speech;
	}
}
