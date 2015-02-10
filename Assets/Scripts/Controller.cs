using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Text.RegularExpressions;

public class Controller : MonoBehaviour 
{
	private Chatbot bot;

	private EventAction currentAction;
	private TextToSpeech speech;
	private Image blackout;
	private Text subtitles;
	private InputField inputField;
	private CanvasGroup inputFieldGroup;
	private bool canType = false;
	private bool typing = false;

	private const int NUM_INTERACTIONS = 30;
	private int interactions = 0;

	private const float FADE_IN_DUR = 5.0f;
	private bool fadeIn = false;
	private bool fadeOut = false;

	private bool isEnding = false;

	private struct Player {
		public GameObject gameObject;
		public Camera camera;
		public MouseLook look;
		public CharacterMotor motor;
		public SkinnedMeshRenderer renderer;
		public Animator animator;
	}

	private Player player;

	private Door labDoor;

	private float startOfEnding = -1.0f;
	private const float CAM_SPEED = 0.8f;
	private const float WALK_SPEED = 1.2f;
	private const float RUN_SPEED = 2.5f;
	private const float LOOKBACK_SPEED = 1.4f;
	private const float WALK_TIME = 3.0f;
	private const float RUN1_TIME = 2.0f;
	private const float LOOKBACK_TIME = 2.6f;
	private const float RUN2_TIME = 4.5f;

	// Use this for initialization
	void Start () 
	{
		bot = new Chatbot();

		// initialization
		bot.getOutput("my name is unknown");
		bot.getOutput("i live in England");


		speech = this.gameObject.GetComponent<TextToSpeech>();
		blackout = GameObject.Find("Canvas/Blackout").GetComponent<Image>();
		subtitles = GameObject.Find("Canvas/Subtitles").GetComponent<Text>();
		inputField = GameObject.Find("Canvas/InputField").GetComponent<InputField>();
		inputField.contentType = InputField.ContentType.Standard;
		inputFieldGroup = inputField.gameObject.GetComponent<CanvasGroup>();
		inputField.onEndEdit.AddListener((value) => SubmitInput(value));
		inputField.text = "";
		inputFieldGroup.alpha = 0;

		player.gameObject = GameObject.Find("Player");
		player.camera = player.gameObject.GetComponentInChildren<Camera>();
		player.look = player.gameObject.GetComponent<MouseLook>();
		player.motor = player.gameObject.GetComponent<CharacterMotor>();
		player.renderer = player.gameObject.GetComponentInChildren<SkinnedMeshRenderer>();
		player.animator = player.gameObject.GetComponentInChildren<Animator>();

		labDoor = GameObject.Find("Door").GetComponent<Door>();

		player.animator.Play("Idle");

		// start with the player invisible
		player.renderer.enabled = false;

		currentAction = new EventAction(
			this, EventAction.Type.WAIT, 3.5f);

		currentAction.Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "Are you (....)awake?", (float x) => this.fadeIn = true)).Then(new EventAction(
			this, EventAction.Type.WAIT, 2.0f)).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "Oh.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "You are finally here.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "I know you may me very... (....)confused.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "I am speaking to you through this lab's broadcast system.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "My name is Maximilian, and I am a robot with a very specific task.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "For many years I have been looking for a particular individual.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "And I think that may be you.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "But I need to be sure.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "Right now I know very little about who you are.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 2.0f, "So why don't we talk?")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "I must warn you that some of my circuitry did not age well.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "I'm afraid I don't have much time left.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "I will try to act as human as I can (....)when responding to you.")).Then(new EventAction(
			this, EventAction.Type.DIALOG, 1.0f, "So tell me... (....)what is your name?", (float x) => this.EnableTyping()));
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (currentAction != null)
			currentAction = currentAction.Tick();

		if (canType && Input.GetKeyDown(KeyCode.Return))
		{
			if (typing)
			{
				DisableTyping();
			}
			else
			{
				typing = true;
				player.motor.canControl = false;
				inputField.text = "";
				inputFieldGroup.alpha = 1;
				inputField.ActivateInputField();
				inputField.Select();
			}
		}

		float blackoutAlpha = blackout.color.a;

		if (fadeOut && blackoutAlpha < 255.0f)
		{
			fadeIn = false; // fade out has precedence
			blackoutAlpha += (1.0f / FADE_IN_DUR) * Time.deltaTime;
			blackoutAlpha = Mathf.Clamp(blackoutAlpha, 0.0f, 255.0f);
			blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
		}
		else if (fadeIn && blackoutAlpha > 0.0f)
		{
			blackoutAlpha -= (1.0f / FADE_IN_DUR) * Time.deltaTime;
			blackoutAlpha = Mathf.Clamp(blackoutAlpha, 0.0f, 255.0f);
			blackout.color = new Color(blackout.color.r, blackout.color.g, blackout.color.b, blackoutAlpha);
		}


		// ENDING
		if (isEnding && startOfEnding >= 0.0f)
		{
			player.camera.transform.position = player.camera.transform.position + new Vector3(0.0f, 0.0f, CAM_SPEED * Time.deltaTime);

			if (Time.time - startOfEnding < WALK_TIME)
			{
				player.animator.Play("Walk");
				player.gameObject.transform.position = player.gameObject.transform.position + new Vector3(0.0f, 0.0f, WALK_SPEED * Time.deltaTime);
			}
			else if (Time.time - startOfEnding < WALK_TIME + RUN1_TIME)
			{
				player.animator.Play("Run");
				player.gameObject.transform.position = player.gameObject.transform.position + new Vector3(0.0f, 0.0f, RUN_SPEED * Time.deltaTime);
			}
			else if (Time.time - startOfEnding < WALK_TIME + RUN1_TIME + LOOKBACK_TIME)
			{
				player.animator.Play("LookBack");
				player.gameObject.transform.position = player.gameObject.transform.position + new Vector3(0.0f, 0.0f, LOOKBACK_SPEED * Time.deltaTime);
			}
			else if (Time.time - startOfEnding < WALK_TIME + RUN1_TIME + LOOKBACK_TIME + RUN2_TIME)
			{
				player.animator.Play("Run");
				player.gameObject.transform.position = player.gameObject.transform.position + new Vector3(0.0f, 0.0f, RUN_SPEED * Time.deltaTime);
			}
			else
			{
				player.gameObject.transform.position = player.gameObject.transform.position + new Vector3(0.0f, 0.0f, RUN_SPEED * Time.deltaTime);
				this.fadeOut = true;
			}
		}
	}

	private void EnableTyping()
	{
		canType = true;
	}

	private void DisableTyping()
	{
		inputField.text = "";
		inputFieldGroup.alpha = 0;
		
		typing = false;
		player.motor.canControl = true;
		inputField.DeactivateInputField();
	}

	private void SubmitInput(string value)
	{
		value = value.Trim();

		if (value.Length == 0)
			return;

		string response = bot.getOutput(value);

		for (int i = 0; i < response.Length - 1; i++)
		{
			if ((response[i] == '.' || response[i] == ',') && response[i + 1] != ' ')
			{
				response = response.Substring(0, i + 1) + " " + response.Substring(i + 1, response.Length - i - 1);
			}
		}

		//Debug.Log(response);

		if (currentAction == null)
		{
			currentAction = new EventAction(this, EventAction.Type.DIALOG, 1.0f, response);
		}
		else
		{
			currentAction.Then(new EventAction(this, EventAction.Type.DIALOG, 1.0f, response));
		}

		interactions++;

		if (interactions >= NUM_INTERACTIONS)
		{
			canType = false;
			this.DisableTyping();

			currentAction.Then(new EventAction(this, EventAction.Type.WAIT, 1.0f)).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "I think that is enough.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "You have proven that you are a good example of a person.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 1.0f, "Better than all the others, and myself, at least.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "So you will have to do.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "I was built to find the best human mind.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "I have... (....)finally created you.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "Now my work has ended.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "You may now step out and go wherever your curiosity takes you.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "Your purpose... (....) is to be the last living human.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "All the others are gone. Including my creators.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "I bid you farewell.")).Then(new EventAction(
				this, EventAction.Type.DIALOG, 2.0f, "It was nice meeting you.", (float x) => this.labDoor.Open()));
		}
	}

	public void OnTriggerEnter(Collider other)
	{
		if (!isEnding)
		{
			isEnding = true;
			TriggerEndingSequence();
		}
	}

	public Text GetSubtitles()
	{
		return subtitles;
	}

	public TextToSpeech GetTextToSpeech()
	{
		return speech;
	}

	public void TriggerEndingSequence()
	{
		player.renderer.enabled = true;
		player.motor.canControl = false;
		player.look.enable = false;
		player.gameObject.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);
		player.camera.transform.SetParent(null);
		player.camera.transform.localEulerAngles = new Vector3(0.0f, 0.0f, 0.0f);

		startOfEnding = Time.time;
	}
}
